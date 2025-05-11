// Sync and cache management for buildings and adjacency data

class DataSyncManager {
    constructor(options = {}) {
        this.localBuildings = window.buildings || [];
        this.localAdjacency = window.adjacency || {};
        this.legacyAdjacency = window.legacyAdjacency || {};
        this.syncInterval = options.syncInterval || 20000; // 20 seconds default
        this.buildingsEndpoint = options.buildingsEndpoint || '/api/Campus/buildings';
        this.connectionsEndpoint = options.connectionsEndpoint || '/api/Campus/connections';
        this.onDataChanged = options.onDataChanged || function() {};
        this.lastSyncTime = null;
        this.syncStatus = options.syncStatusElement || document.getElementById('syncStatus');
        
        // Store the original data for comparison
        this.originalBuildingsJson = JSON.stringify(this.localBuildings);
        this.originalAdjacencyJson = JSON.stringify(this.localAdjacency);
    }

    startSync() {
        // Initialize with local data first
        if (window.buildings && window.buildings.length > 0) {
            console.log('Using initial local data from datas.js');
            this.updateSyncStatus('Yerel veri yüklendi');
            this.onDataChanged(this.localBuildings, this.localAdjacency);
        }
        
        // Perform initial sync
        this.syncData();
        
        // Setup periodic sync
        this.syncTimer = setInterval(() => this.syncData(), this.syncInterval);
    }

    stopSync() {
        if (this.syncTimer) {
            clearInterval(this.syncTimer);
            this.syncTimer = null;
        }
    }

    async syncData() {
        try {
            this.updateSyncStatus('Veri güncelleniyor...');
            
            // Fetch buildings data
            const buildingsResponse = await fetch(this.buildingsEndpoint);
            if (!buildingsResponse.ok) throw new Error('Failed to load buildings data');
            const newBuildings = await buildingsResponse.json();
            
            // Fetch connections data
            const connectionsResponse = await fetch(this.connectionsEndpoint);
            if (!connectionsResponse.ok) throw new Error('Failed to load connections data');
            const newAdjacency = await connectionsResponse.json();
            
            // Check if data has changed
            const newBuildingsJson = JSON.stringify(newBuildings);
            const newAdjacencyJson = JSON.stringify(newAdjacency);
            
            const buildingsChanged = newBuildingsJson !== JSON.stringify(this.localBuildings);
            const adjacencyChanged = newAdjacencyJson !== JSON.stringify(this.localAdjacency);
            
            if (buildingsChanged || adjacencyChanged) {
                console.log('Data changes detected, updating local cache');
                
                // Update local data
                this.localBuildings = newBuildings;
                this.localAdjacency = newAdjacency;
                
                // Generate legacy adjacency format
                this.legacyAdjacency = this.convertToLegacyFormat(newAdjacency);
                
                // Update globals for compatibility
                window.buildings = this.localBuildings;
                window.adjacency = this.localAdjacency;
                window.legacyAdjacency = this.legacyAdjacency;
                
                // Notify about the change
                this.onDataChanged(this.localBuildings, this.localAdjacency);
                this.updateSyncStatus('Veri güncellendi', 'success');
            } else {
                this.updateSyncStatus('Veri güncel', 'info');
                console.log('No changes in data detected');
            }
            
            this.lastSyncTime = new Date();
        } catch (error) {
            console.error('Error syncing data:', error);
            this.updateSyncStatus('Senkronizasyon hatası', 'error');
            
            // If we have no data at all, and local data exists, use it
            if (!this.localBuildings.length && window.buildings && window.buildings.length) {
                this.localBuildings = window.buildings;
                this.localAdjacency = window.adjacency;
                this.legacyAdjacency = window.legacyAdjacency;
                this.onDataChanged(this.localBuildings, this.localAdjacency);
            }
        }
    }

    convertToLegacyFormat(adjacency) {
        const legacyFormat = {};
        for (const source in adjacency) {
            legacyFormat[source] = {};
            for (const target in adjacency[source]) {
                legacyFormat[source][target] = adjacency[source][target].distance || adjacency[source][target];
            }
        }
        return legacyFormat;
    }

    updateSyncStatus(message, type = 'info') {
        if (this.syncStatus) {
            this.syncStatus.textContent = message;
            
            // Update styling based on status type
            this.syncStatus.style.color = type === 'error' ? 'red' : 
                                          type === 'success' ? 'green' : 
                                          'gray';
        }
    }

    forceSync() {
        return this.syncData();
    }
}

// Send user location to server (simplified version)
async function sendUserLocation(nodeName, edgeName) {
    try {
        // Get a user ID from local storage or create a new one
        let userId = localStorage.getItem('campusNavUserId');
        if (!userId) {
            userId = 'user_' + Math.random().toString(36).substring(2, 15);
            localStorage.setItem('campusNavUserId', userId);
        }
        
        // Send to server
        const response = await fetch('/api/UserLocation', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                userId: userId,
                currentNode: nodeName || null,
                currentEdge: edgeName || null
            })
        });
        
        if (!response.ok) {
            throw new Error('Failed to send location data');
        }
        
        console.log('Location data sent successfully');
    } catch (error) {
        console.error('Error sending location:', error);
    }
}

// Get user density data from server
async function getUserDensityData() {
    try {
        const response = await fetch('/api/UserLocation/density');
        if (!response.ok) {
            throw new Error('Failed to fetch user density data');
        }
        
        return await response.json();
    } catch (error) {
        console.error('Error fetching user density:', error);
        return { nodes: {}, edges: {} };
    }
}

// Visualize user density on the map
function visualizeUserDensity(userDensityData) {
    if (!window.trafficLayer) {
        if (window.map) {
            console.warn('Traffic layer was not initialized, attempting now.');
            window.trafficLayer = L.layerGroup().addTo(window.map);
        } else {
            console.error('Map not available. Skipping density visualization.');
            return;
        }
    }
    window.trafficLayer.clearLayers(); // Clear previous traffic visualizations
    
    const edgeDensity = (userDensityData && userDensityData.edges) ? userDensityData.edges : {};
    const defaultEdgeColor = '#A9A9A9'; // DarkGray for no traffic
    const defaultWeight = 2;
    const defaultOpacity = 0.4;

    if (window.adjacency && window.buildings) {
        for (const fromBuildingName in window.adjacency) {
            for (const toBuildingName in window.adjacency[fromBuildingName]) {
                const routeKey = `${fromBuildingName}|${toBuildingName}`;
                const density = edgeDensity[routeKey] || 0;
                
                const fromBuildingData = window.buildings.find(b => b.name === fromBuildingName);
                const toBuildingData = window.buildings.find(b => b.name === toBuildingName);
                
                if (!fromBuildingData || !toBuildingData || !fromBuildingData.coords || !toBuildingData.coords) {
                    console.warn(`Coordinates missing for edge: ${routeKey}`);
                    continue;
                }
                const fromCoords = fromBuildingData.coords;
                const toCoords = toBuildingData.coords;

                let color, weight, opacity, tooltipText;

                if (density > 0) {
                    const hue = Math.max(0, 120 - (density * 12)); 
                    color = `hsl(${hue}, 100%, 50%)`;
                    weight = Math.min(2 + density, 8);
                    opacity = 0.75;
                    tooltipText = `${routeKey}: ${density} kullanıcı`;
                } else {
                    color = defaultEdgeColor;
                    weight = defaultWeight;
                    opacity = defaultOpacity;
                    tooltipText = `${routeKey}: Trafik yok`;
                }

                const polyline = L.polyline([fromCoords, toCoords], {
                    color: color,
                    weight: weight,
                    opacity: opacity
                });

                if (tooltipText) {
                    polyline.bindTooltip(tooltipText);
                }
                
                polyline.addTo(window.trafficLayer);
            }
        }
    }
}

// Add a button to generate dummy data for testing
function addDummyDataButton() {
    const controls = document.getElementById('controls');
    
    const dummyButton = document.createElement('button');
    dummyButton.textContent = 'Dummy Veri Oluştur';
    dummyButton.style.marginLeft = '10px'; 
    dummyButton.onclick = async function() {
        try {
            const response = await fetch('/api/UserLocation/dummy-data', {
                method: 'POST'
            });
            
            if (response.ok) {
                const result = await response.json();
                alert(result.message || 'Dummy veri oluşturuldu');
                
                // Refresh the visualization
                const userDensityData = await getUserDensityData(); // Ensure this is awaited
                visualizeUserDensity(userDensityData);
            } else {
                alert('Dummy veri oluşturma başarısız');
            }
        } catch (error) {
            console.error('Error:', error);
            alert('Bir hata oluştu');
        }
    };
    
    controls.appendChild(dummyButton);
}

// Update the data sync manager to periodically fetch user density
document.addEventListener('DOMContentLoaded', function() {
    // Ensure map is initialized before adding layers
    if (window.map) {
        window.trafficLayer = L.layerGroup().addTo(window.map);
        console.log('Traffic layer added to map.');
    } else {
        console.warn("Map not initialized when trying to add trafficLayer in site.js DOMContentLoaded. Will attempt init in visualizeUserDensity if needed.");
    }

    if (window.dataSyncManager) {
        const originalSyncData = window.dataSyncManager.syncData;
        window.dataSyncManager.syncData = async function() {
            await originalSyncData.call(this);
            
            // Also fetch and visualize user density
            const userDensityData = await getUserDensityData(); // Ensure this is awaited
            visualizeUserDensity(userDensityData);
        };
    }
});

// Initialize the dummy data button when the page loads
document.addEventListener('DOMContentLoaded', function() {
    addDummyDataButton();
});

// To be used in the main script
window.DataSyncManager = DataSyncManager;