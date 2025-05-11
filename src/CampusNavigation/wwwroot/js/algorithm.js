let stepText = "";

document.addEventListener('DOMContentLoaded', function() {
    // Map initialization
    window.map = L.map('map', {
        minZoom: 15,
        maxBounds: [
            [40.21720889071898, 28.845295152664185], //alt, sol
            [40.235267194326756, 28.895806934356693]//üst, sağ
        ],
        maxBoundsViscosity: 0.9
    }).setView([40.22459954185981, 28.872349262237552], 12);

    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap contributors'
    }).addTo(window.map);

    // HTML elementleri (references)
    const startSelect = document.getElementById("startSelect");
    const endSelect = document.getElementById("endSelect");
    const routeTypeSelect = document.getElementById("routeTypeSelect");

    // Initial dropdown population from algorithm.js (if still needed)
    // This might conflict with populateDropdowns in index.html which is called by DataSyncManager
    // Review if this is necessary or should be removed / coordinated.
    if (window.buildings && typeof window.buildings.forEach === 'function' && startSelect && endSelect) {
        window.buildings.forEach(b => {
            if (!b.name.startsWith("Ar")) {
                const opt1 = new Option(b.name, b.name);
                const opt2 = new Option(b.name, b.name);
                startSelect.add(opt1);
                endSelect.add(opt2);
            }
        });
    }

    // Global variables for path display, initialized here as they depend on window.map
    // window.routeLine is already initialized in index.html
    // window.activeMarkers is already initialized in index.html
    let pathHistory = {}; // Local to algorithm.js scope, or window.pathHistory if needed globally
    window.segmentGroup = L.layerGroup().addTo(window.map);

    // ALL YOUR FUNCTIONS (showShortestPath, findBuildingByIdOrName, displayRoute, dijkstra etc.)
    // should be defined or moved here, INSIDE this DOMContentLoaded listener.
    // For example:
    function findBuildingByIdOrName(value) {
        if (!window.buildings || !Array.isArray(window.buildings)) {
            console.error("Buildings array not available", window.buildings);
            return null;
        }
        let building = window.buildings.find(b => 
            b.id === value || 
            b.id === parseInt(value) || 
            String(b.id) === String(value)
        );
        if (!building) {
            building = window.buildings.find(b => b.name === value);
        }
        if (!building) {
            console.warn(`Building not found with value: ${value}`);
        }
        return building;
    }

    function getTrafficDescription(trafficValue) {
        if (trafficValue <= 2) return "Düsük";
        if (trafficValue <= 5) return "Normal";
        if (trafficValue <= 8) return "Yogun";
        return "Cok yogun";
    }

    // (Other helper functions like Queue, dijkstra, dijkstraTrafficAware, dijkstraSimple)
    // should also be defined here or ensured they don't rely on DOM before it's ready.
    // Class Queue definition (if it's simple and doesn't interact with DOM)
    class Queue { 
        constructor() { this.items = []; }
        enqueue(item) { this.items.push(item); }
        dequeue() { return this.items.shift(); }
        isEmpty() { return this.items.length === 0; }
        toArray() { return [...this.items]; }
        delete(item) { 
            const index = this.items.indexOf(item);
            if (index > -1) this.items.splice(index, 1);
        }
    }

    // Function to fetch building user counts
    async function getBuildingUserCounts() {
        try {
            const response = await fetch('/api/campus/building-user-counts');
            if (!response.ok) {
                console.error('Failed to fetch building user counts:', response.status);
                return {}; // Return empty object on failure
            }
            return await response.json();
        } catch (error) {
            console.error('Error fetching building user counts:', error);
            return {}; // Return empty object on error
        }
    }

    function dijkstra(graph, start, end) {
        const distances = {};
        const previous = {};
        const queue = new Queue();
        for (let node in graph) {
            distances[node] = Infinity;
            previous[node] = null;
            queue.enqueue(node);
        }
        distances[start] = 0;

        while (!queue.isEmpty()) {
            const current = queue.toArray().reduce((a, b) => distances[a] < distances[b] ? a : b);
            queue.delete(current);
            if (current === end) break;

            for (let neighbor in graph[current]) {
                const alt = distances[current] + graph[current][neighbor];
                if (alt < distances[neighbor]) {
                    distances[neighbor] = alt;
                    previous[neighbor] = current;
                }
            }
        }

        const path = [];
        let u = end;
        while (u) {
            path.unshift(u);
            u = previous[u];
        }
        return path[0] === start ? path : [];
    }

    function dijkstraTrafficAware(graph, start, end, userDensityData) {
        const distances = {};
        const previous = {};
        const queue = new Set(Object.keys(graph));
        
        for (let node of queue) {
            distances[node] = Infinity;
            previous[node] = null;
        }
        distances[start] = 0;
        
        while (queue.size > 0) {
            const current = [...queue].reduce((a, b) => distances[a] < distances[b] ? a : b);
            if (current === end) break;
            if (distances[current] === Infinity) break;
            queue.delete(current);
            
            for (let neighbor in graph[current]) {
                if (queue.has(neighbor)) {
                    const segment = graph[current][neighbor];
                    let trafficFactor;
                    if (typeof segment === 'object') {
                        trafficFactor = segment.traffic;
                    } else {
                        trafficFactor = 3;
                    }
                    const routeKey = `${current}|${neighbor}`;
                    if (userDensityData && userDensityData[routeKey]) {
                        const userDensityFactor = Math.min(userDensityData[routeKey] / 2, 10);
                        trafficFactor += userDensityFactor;
                    }
                    const distance = typeof segment === 'object' ? segment.distance : segment;
                    const weightedDistance = distance * (1 + (trafficFactor / 10));
                    const alt = distances[current] + weightedDistance;
                    if (alt < distances[neighbor]) {
                        distances[neighbor] = alt;
                        previous[neighbor] = current;
                    }
                }
            }
        }
        if (distances[end] === Infinity) return [];
        const path = [];
        let currentPathNode = end;
        while (currentPathNode !== null) {
            path.unshift(currentPathNode);
            currentPathNode = previous[currentPathNode];
        }
        return path;
    }

    function dijkstraQuickestPath(graph, start, end, buildingUserCounts) {
        const distances = {}; // Stores the 'cost' (time) to reach each node
        const previous = {};  // Stores the previous node in the optimal path
        const queue = new Set(Object.keys(graph)); // Set of unvisited nodes

        for (let node of queue) {
            distances[node] = Infinity;
            previous[node] = null;
        }
        distances[start] = 0;

        while (queue.size > 0) {
            // Find the node with the smallest distance among unvisited nodes
            const current = [...queue].reduce((a, b) => distances[a] < distances[b] ? a : b);

            if (current === end) break; // Path found
            if (distances[current] === Infinity) break; // No path to remaining nodes

            queue.delete(current);

            for (let neighbor in graph[current]) {
                if (queue.has(neighbor)) {
                    const segment = graph[current][neighbor]; // Connection details (distance, traffic)
                    const baseDistance = (typeof segment === 'object' ? segment.distance : segment) || 0;
                    
                    // Penalty for passing through a building, proportional to user count
                    // Higher user count = higher penalty (more time to pass through)
                    // We use buildingUserCounts for the 'neighbor' node, as that's the building we are entering.
                    const buildingId = window.buildings.find(b => b.name === neighbor)?.id;
                    const userCountInNeighbor = buildingId && buildingUserCounts[buildingId] ? buildingUserCounts[buildingId] : 0;
                    
                    // Define a penalty factor. Adjust this based on desired impact.
                    // E.g., each user adds 0.1 to the "time" cost of traversing the building node.
                    // This is an arbitrary penalty; needs tuning.
                    // It's applied when *entering* a building (neighbor).
                    const buildingPenalty = userCountInNeighbor * 0.5; // Example: 0.5 "time units" per user

                    // The "cost" to travel to the neighbor is the path distance + penalty for entering the neighbor building
                    const alt = distances[current] + baseDistance + buildingPenalty;

                    if (alt < distances[neighbor]) {
                        distances[neighbor] = alt;
                        previous[neighbor] = current;
                    }
                }
            }
        }

        if (distances[end] === Infinity) return []; // No path found

        // Reconstruct the path
        const path = [];
        let currentPathNode = end;
        while (currentPathNode !== null) {
            path.unshift(currentPathNode);
            currentPathNode = previous[currentPathNode];
        }
        return path[0] === start ? path : [];
    }

    function dijkstraSimple(graph, startName, endName) {
        console.log("Running simplified Dijkstra from", startName, "to", endName);
        console.log("Graph has", Object.keys(graph).length, "nodes");
        if (!graph || !startName || !endName) {
            console.error("Missing required parameters");
            return [];
        }
        if (!graph[startName]) {
            console.error("Start node not in graph:", startName);
            return [];
        }
        const distances = {};
        const previous = {};
        const unvisited = new Set();
        for (const node in graph) {
            distances[node] = Infinity;
            previous[node] = null;
            unvisited.add(node);
        }
        distances[startName] = 0;
        while (unvisited.size > 0) {
            let current = null;
            let minDistance = Infinity;
            for (const node of unvisited) {
                if (distances[node] < minDistance) {
                    minDistance = distances[node];
                    current = node;
                }
            }
            if (current === null || current === endName) break;
            unvisited.delete(current);
            for (const neighbor in graph[current]) {
                if (unvisited.has(neighbor)) {
                    const distance = typeof graph[current][neighbor] === 'object' 
                        ? graph[current][neighbor].distance 
                        : graph[current][neighbor];
                    const alt = distances[current] + distance;
                    if (alt < distances[neighbor]) {
                        distances[neighbor] = alt;
                        previous[neighbor] = current;
                    }
                }
            }
        }
        if (distances[endName] === Infinity) {
            console.log("No path found to", endName);
            return [];
        }
        const path = [];
        let currentPathReconstruction = endName;
        while (currentPathReconstruction !== null) {
            path.unshift(currentPathReconstruction);
            currentPathReconstruction = previous[currentPathReconstruction];
        }
        console.log("Path found:", path);
        return path;
    }
    
    function displayRoute(path, routeType) {
        console.log("Displaying route:", path, "routeType:", routeType);
        const latlngs = [];
        const invalidBuildings = [];
        for (const name of path) {
            const building = window.buildings.find(b => b.name === name);
            if (!building) {
                console.warn(`Building "${name}" not found for path display`);
                invalidBuildings.push(name);
                continue;
            }
            if (typeof building.latitude !== 'number' || typeof building.longitude !== 'number') {
                console.warn(`Building "${name}" has invalid coordinates:`, building);
                invalidBuildings.push(name);
                continue;
            }
            latlngs.push([building.latitude, building.longitude]);
        }
        if (invalidBuildings.length > 0) {
            console.error("Some buildings have invalid coordinates:", invalidBuildings);
        }
        if (latlngs.length < 2) {
            console.error("Not enough valid coordinates to draw path:", latlngs);
            alert("Rota çizilemedi: Yeterli koordinat bulunamadı");
            return;
        }
        console.log("Path coordinates:", latlngs);
        const routeColor = routeType === 'leastTraffic' ? '#9C27B0' : '#2196F3';
        try {
            window.routeLine = L.polyline(latlngs, { 
                color: routeColor, 
                weight: 5,
                dashArray: routeType === 'leastTraffic' ? '10, 10' : null
            }).addTo(window.map);
            window.activeMarkers.forEach(marker => marker.remove()); // Clear previous step markers
            window.activeMarkers = []; // Reset active markers for new route steps
            path.forEach((stop, i) => {
                const building = window.buildings.find(b => b.name === stop);
                if (building && 
                    typeof building.latitude === 'number' && 
                    typeof building.longitude === 'number') {
                    const marker = L.marker([building.latitude, building.longitude])
                        .addTo(window.segmentGroup || window.map) // Ensure segmentGroup is used
                        .bindTooltip(stop, { permanent: true, direction: 'top', offset: [0, -10] });
                    window.activeMarkers.push(marker);
                }
            });
            if (window.routeLine && window.routeLine.getBounds) {
                window.map.fitBounds(window.routeLine.getBounds(), {
                    padding: [50, 50]
                });
            }
            if (typeof showStepList === 'function') {
                // showStepList(path, routeType); // Commented out to prevent display
            }
            console.log("Route display complete");
        } catch (error) {
            console.error("Error drawing route:", error);
            alert("Rota çizilirken bir hata oluştu: " + error.message);
        }
    }

    function showStepList(path, routeType) {
        const container = document.getElementById("stepList");
        if (!container) return;
        let totalDistance = 0;
        for (let i = 0; i < path.length - 1; i++) {
            if (routeType === "shortest" && window.legacyAdjacency && window.legacyAdjacency[path[i]] && window.legacyAdjacency[path[i]][path[i+1]]) {
                totalDistance += window.legacyAdjacency[path[i]][path[i+1]];
            } else if (routeType === "leastTraffic" && window.adjacency && window.adjacency[path[i]] && window.adjacency[path[i]][path[i+1]] && typeof window.adjacency[path[i]][path[i+1]].distance !== 'undefined') {
                totalDistance += window.adjacency[path[i]][path[i+1]].distance;
            } else if (window.adjacency && window.adjacency[path[i]] && window.adjacency[path[i]][path[i+1]] && typeof window.adjacency[path[i]][path[i+1]].distance !== 'undefined') {
                totalDistance += window.adjacency[path[i]][path[i+1]].distance;
            } else if (window.legacyAdjacency && window.legacyAdjacency[path[i]] && window.legacyAdjacency[path[i]][path[i+1]]) {
                totalDistance += window.legacyAdjacency[path[i]][path[i+1]];
            }
        }
        const routeTypeTitle = routeType === "shortest" ? "En Kısa Rota" : "Trafik Yoğunluğuna Göre Rota";
        let html = `<b>${routeTypeTitle} (${totalDistance}m)</b><ol>`;
        for (let i = 0; i < path.length - 1; i++) {
            let distanceInfo = "";
            if (routeType === "shortest" && window.legacyAdjacency && window.legacyAdjacency[path[i]] && window.legacyAdjacency[path[i]][path[i+1]]) {
                distanceInfo = `(${(window.legacyAdjacency[path[i]][path[i+1]]) || '?'}m)`;
            } else if (routeType === "leastTraffic" && window.adjacency && window.adjacency[path[i]] && window.adjacency[path[i]][path[i+1]]) {
                const segment = window.adjacency[path[i]][path[i+1]];
                if (segment && typeof segment.distance !== 'undefined') {
                    const trafficInfo = getTrafficDescription(segment.traffic);
                    distanceInfo = `(${segment.distance}m, Trafik: ${trafficInfo})`;
                } else {
                    distanceInfo = "(bilgi yok)";
                }
            } else if (window.adjacency && window.adjacency[path[i]] && window.adjacency[path[i]][path[i+1]] && typeof window.adjacency[path[i]][path[i+1]].distance !== 'undefined') {
                const segment = window.adjacency[path[i]][path[i+1]];
                distanceInfo = `(${segment.distance}m)`;
            } else if (window.legacyAdjacency && window.legacyAdjacency[path[i]] && window.legacyAdjacency[path[i]][path[i+1]]) {
                distanceInfo = `(${(window.legacyAdjacency[path[i]][path[i+1]]) || '?'}m)`;
            }
            html += `<li>${path[i]} → ${path[i + 1]} ${distanceInfo}</li>`;
        }
        html += "</ol>";
        container.innerHTML = html;
    }

    function showStepListAlert(path, routeType) {
        stepText = ""; // Reset global stepText
        let totalDistance = 0;
        for (let i = 0; i < path.length - 1; i++) {
            if (routeType === "shortest") {
                totalDistance += (window.legacyAdjacency || {})[path[i]][path[i + 1]];
            } else {
                const segment = (window.adjacency || {})[path[i]][path[i + 1]];
                if (segment) totalDistance += segment.distance;
            }
        }
        const routeTypeTitle = routeType === "shortest" ? "En Kısa Rota" : "Trafik Duyarlı Rota";
        stepText += `${routeTypeTitle} (${totalDistance}m)\n\n`;
        for (let i = 0; i < path.length - 1; i++) {
            if (routeType === "shortest") {
                const distance = (window.legacyAdjacency || {})[path[i]][path[i + 1]];
                stepText += `${i + 1}. ${path[i]} → ${path[i + 1]} (${distance || '?'}m)\n`;
            } else {
                const segment = (window.adjacency || {})[path[i]][path[i + 1]];
                if (segment) {
                    const trafficInfo = getTrafficDescription(segment.traffic);
                    stepText += `${i + 1}. ${path[i]} → ${path[i + 1]} (${segment.distance}m, Trafik: ${trafficInfo})\n`;
                }
            }
        }
    }
    
    // This function is called by the button in index.html
    // It needs to be global.
    function showShortestPathInternal() {
        console.log("=== Starting path calculation ===");
        if (!startSelect || !endSelect || !routeTypeSelect) {
            console.error("Required UI elements not found");
            alert("UI elements not found. Please refresh the page.");
            return;
        }
        const start = startSelect.value;
        const end = endSelect.value;
        const routeType = routeTypeSelect.value;
        console.log("From dropdown values:", { start, end, routeType });
        if (!start || !end) {
            alert("Lütfen başlangıç ve bitiş konumlarını seçin");
            return;
        }
        const startBuilding = findBuildingByIdOrName(start);
        const endBuilding = findBuildingByIdOrName(end);
        console.log("Found buildings:", { 
            startBuilding: startBuilding ? startBuilding.name : 'Not found', 
            endBuilding: endBuilding ? endBuilding.name : 'Not found'
        });
        if (!startBuilding || !endBuilding) {
            alert("Seçilen binalar bulunamadı");
            console.error("Buildings not found:", { start, end });
            return;
        }
        console.log("Finding path from", startBuilding.name, "to", endBuilding.name);
        if (window.routeLine) {
            window.map.removeLayer(window.routeLine);
            window.routeLine = null;
        }
        if (window.segmentGroup) {
            window.segmentGroup.clearLayers();
        }
        window.activeMarkers.forEach(marker => {
            if (marker && marker.remove) marker.remove();
        });
        window.activeMarkers = [];
        if (!window.adjacency || Object.keys(window.adjacency).length === 0) {
            if (typeof adjacency !== 'undefined') { // Fallback to datas.js adjacency
                console.log("Using fallback adjacency data from datas.js");
                window.adjacency = adjacency;
                window.legacyAdjacency = {};
                for (const source in adjacency) {
                    window.legacyAdjacency[source] = {};
                    for (const target in adjacency[source]) {
                        window.legacyAdjacency[source][target] = adjacency[source][target].distance;
                    }
                }
            } else {
                console.error("No adjacency data available");
                alert("Rota verileri yüklenemedi. Lütfen sayfayı yenileyip tekrar deneyin.");
                return;
            }
        }
        console.log("Adjacency data available:", {
            sourcesCount: Object.keys(window.adjacency).length,
            legacySourcesCount: window.legacyAdjacency ? Object.keys(window.legacyAdjacency).length : 0
        });
        if (!window.adjacency[startBuilding.name]) {
            console.error("Start building not found in adjacency data:", startBuilding.name);
            alert(`Başlangıç binası (${startBuilding.name}) için bağlantı verisi bulunamadı.`);
            return;
        }
        
        // Fetch all necessary data concurrently
        Promise.all([
            window.getUserDensityData ? window.getUserDensityData() : Promise.resolve({ edges: {} }),
            routeType === 'quickestPath' ? getBuildingUserCounts() : Promise.resolve({})
        ])
        .then(([userDensityData, buildingUserCounts]) => {
            let path;
            try {
                if (routeType === 'leastTraffic') {
                    console.log("Calculating 'Least Traffic' path using edge densities:", userDensityData.edges || {});
                    path = dijkstraTrafficAware(window.adjacency, startBuilding.name, endBuilding.name, userDensityData.edges || {});
                } else if (routeType === 'quickestPath') {
                    console.log("Calculating 'Quickest Path' using building user counts:", buildingUserCounts);
                    // Ensure buildingUserCounts keys are building IDs if that's what the backend provides
                    // The dijkstraQuickestPath function expects building names as keys in the graph,
                    // but uses building IDs to look up user counts.
                    path = dijkstraQuickestPath(window.adjacency, startBuilding.name, endBuilding.name, buildingUserCounts || {});
                } else { // 'shortest'
                    console.log("Calculating 'Shortest Distance' path using legacy adjacency:", window.legacyAdjacency || {});
                    path = dijkstra(window.legacyAdjacency || {}, startBuilding.name, endBuilding.name);
                }
                
                console.log("Path calculation result:", path);
                if (!path || path.length === 0) {
                    alert(`Bu iki nokta arasında yol bulunamadı: ${startBuilding.name} → ${endBuilding.name}`);
                    console.error("No path found between", startBuilding.name, "and", endBuilding.name);
                    return;
                }
                console.log("Path found:", path);
                pathHistory[routeType === 'leastTraffic' ? 'trafficAware' : 'shortest'] = path;
                displayRoute(path, routeType);
            } catch (error) {
                console.error("Error in path algorithm:", error);
                alert("Rota hesaplanırken bir hata oluştu: " + error.message);
            }
        })
        .catch(error => {
            console.error("Error calculating path:", error);
            alert("Rota hesaplanırken bir hata oluştu: " + error.message);
        });
    }
    window.showShortestPath = showShortestPathInternal; // Expose to global scope

    function speakPath(path, routeType) {
        let routeDesc = routeType === "shortest" ? "En kısa" : "Trafik duyarlı";
        const filteredPath = path.filter(step => !step.includes("Ar"));
        const utterance = new SpeechSynthesisUtterance(`${routeDesc} rota: ${filteredPath.join(" → ")}`);
        utterance.rate = 0.8;
        utterance.lang = "tr-TR";
        speechSynthesis.speak(utterance);
    }
    window.speakPath = speakPath; // Expose if called globally

    function showStepOnAlertBox() {
        if(stepText)
            alert(stepText);
        else
            alert("Rota bilgileri belirlenmedi. Lütfen rota belirleyiniz.");
    }
    window.showStepOnAlertBox = showStepOnAlertBox; // Expose if called globally

    // Mock getUserDensityData if not provided by site.js or elsewhere
    if (typeof window.getUserDensityData === 'undefined') {
        window.getUserDensityData = function() {
            console.log("Using mock getUserDensityData in algorithm.js");
            return Promise.resolve({ nodes: {}, edges: {} });
        };
    }
}); // End of DOMContentLoaded
