using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CampusNavigation.Data;
using CampusNavigation.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CampusNavigation.Services
{
    public class DatabaseSeedService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DatabaseSeedService> _logger;

        public DatabaseSeedService(
            IServiceScopeFactory scopeFactory,
            ILogger<DatabaseSeedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                _logger.LogInformation("Checking database connection and data...");
                
                // Try to connect to the database
                if (!await dbContext.Database.CanConnectAsync(cancellationToken))
                {
                    _logger.LogWarning("Cannot connect to database. Please check connection string and server.");
                    return;
                }

                // Ensure database is created
                await dbContext.Database.EnsureCreatedAsync(cancellationToken);
                
                // Only seed if the database is empty
                if (!await dbContext.Buildings.AnyAsync(cancellationToken))
                {
                    _logger.LogInformation("Database is empty, seeding initial data...");
                    await SeedDataAsync(dbContext, cancellationToken);
                }
                else
                {
                    _logger.LogInformation("Database already contains data. Skipping seed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database");
            }
        }

        private async Task SeedDataAsync(ApplicationDbContext context, CancellationToken cancellationToken)
        {
            try
            {
                // Buildings from datas.js
                var buildings = new List<Building>
                {
                    // --- Start of buildings from datas.js ---
                    new Building { Name = "üniversite ana giriş", Latitude = 40.21925, Longitude = 28.879861 },
                    new Building { Name = "İBF C Blok", Latitude = 40.227861, Longitude = 28.873917 },
                    new Building { Name = "Kampüs Kafe", Latitude = 40.229889, Longitude = 28.872639 },
                    new Building { Name = "Uü Güzel Sanatlar Fakültesi", Latitude = 40.221556, Longitude = 28.875694 },
                    new Building { Name = "Tıp Fakültesi", Latitude = 40.219778, Longitude = 28.869111 },
                    new Building { Name = "Göz Hastanesi", Latitude = 40.223306, Longitude = 28.868778 },
                    new Building { Name = "Afet Acil Durum", Latitude = 40.221611, Longitude = 28.872139 },
                    new Building { Name = "Uü Camii", Latitude = 40.223167, Longitude = 28.870667 },
                    new Building { Name = "Uü Derslik ve Merkez Birimler", Latitude = 40.220917, Longitude = 28.862083 },
                    new Building { Name = "Rektörlük", Latitude = 40.221222, Longitude = 28.867639 },
                    new Building { Name = "Metro", Latitude = 40.218667, Longitude = 28.870056 },
                    new Building { Name = "Uni+Sports", Latitude = 40.218972, Longitude = 28.865 },
                    new Building { Name = "Daichii  Arge", Latitude = 40.221417, Longitude = 28.860389 },
                    new Building { Name = "Ulutek", Latitude = 40.222806, Longitude = 28.859639 },
                    new Building { Name = "Uü Kütüphane", Latitude = 40.225889, Longitude = 28.872389 },
                    new Building { Name = "David People", Latitude = 40.226556, Longitude = 28.871944 },
                    new Building { Name = "Fen Fak", Latitude = 40.222556, Longitude = 28.863833 },
                    new Building { Name = "Mete Cengiz", Latitude = 40.222694, Longitude = 28.866389 },
                    new Building { Name = "Veterinerlik Fak", Latitude = 40.229639, Longitude = 28.876 },
                    new Building { Name = "Endüstri Müh", Latitude = 40.227694, Longitude = 28.876889 },
                    new Building { Name = "Makina Müh", Latitude = 40.227222, Longitude = 28.875944 },
                    new Building { Name = "Mimarlık Fak", Latitude = 40.227583, Longitude = 28.876694 },
                    new Building { Name = "Otomotiv Müh", Latitude = 40.227, Longitude = 28.875611 },
                    new Building { Name = "İBF A ve B blok", Latitude = 40.226611, Longitude = 28.874778 },
                    new Building { Name = "Sağlık yüksek Okulu", Latitude = 40.225444, Longitude = 28.875944 },
                    new Building { Name = "Elektrik Tekstil Bilgisayar Müh", Latitude = 40.225139, Longitude = 28.875278 },
                    new Building { Name = "Çevre Müh", Latitude = 40.226861, Longitude = 28.877472 },
                    new Building { Name = "Makina topl.", Latitude = 40.226444, Longitude = 28.876639 },
                    new Building { Name = "Eğitim Fak", Latitude = 40.2244445, Longitude = 28.8766945 },
                    new Building { Name = "İnşaat Fak", Latitude = 40.2227223, Longitude = 28.8768889 },
                    new Building { Name = "Yurtlar Bölg", Latitude = 40.228722, Longitude = 28.870861 },
                    new Building { Name = "Güzel sanatlar", Latitude = 40.227639, Longitude = 28.862083 },
                    new Building { Name = "Kestirme", Latitude = 40.2277778, Longitude = 28.8631389 },
                    new Building { Name = "BasımEviMüdürlüğü", Latitude = 40.2245556, Longitude = 28.8619722 },
                    new Building { Name = "ZiraatMühendisliği", Latitude = 40.2260834, Longitude = 28.86275 },
                    new Building { Name = "Besaş", Latitude = 40.2281945, Longitude = 28.8587778 },
                    new Building { Name = "Çıkış", Latitude = 40.228611, Longitude = 28.854833 },
                    new Building { Name = "Ar1", Latitude = 40.2195, Longitude = 28.87775 },
                    new Building { Name = "Ar2", Latitude = 40.222278, Longitude = 28.875056 },
                    new Building { Name = "Ar3", Latitude = 40.220972, Longitude = 28.872694 },
                    new Building { Name = "Ar4", Latitude = 40.223917, Longitude = 28.869861 },
                    new Building { Name = "Ar5", Latitude = 40.222111, Longitude = 28.866917 },
                    new Building { Name = "Ar6", Latitude = 40.219389, Longitude = 28.862167 },
                    new Building { Name = "Ar7", Latitude = 40.221194, Longitude = 28.862139 },
                    new Building { Name = "Ar8", Latitude = 40.222, Longitude = 28.862528 },
                    new Building { Name = "Ar9", Latitude = 40.2235, Longitude = 28.865528 },
                    new Building { Name = "Ar10", Latitude = 40.21925, Longitude = 28.8695 },
                    new Building { Name = "Ar11", Latitude = 40.223833, Longitude = 28.873861 },
                    new Building { Name = "Ar12", Latitude = 40.2262223, Longitude = 28.8721667 },
                    new Building { Name = "Ar13", Latitude = 40.22725, Longitude = 28.872278 },
                    new Building { Name = "Ar14", Latitude = 40.227028, Longitude = 28.87225 },
                    new Building { Name = "Ar15", Latitude = 40.230889, Longitude = 28.875028 },
                    new Building { Name = "Ar16", Latitude = 40.227972, Longitude = 28.877444 },
                    new Building { Name = "Ar17", Latitude = 40.22725, Longitude = 28.878222 },
                    new Building { Name = "UET", Latitude = 40.226222, Longitude = 28.876111 },
                    new Building { Name = "Ar18", Latitude = 40.226556, Longitude = 28.877 },
                    new Building { Name = "Ar19", Latitude = 40.2261111, Longitude = 28.8773889 },
                    new Building { Name = "Ar20", Latitude = 40.2248056, Longitude = 28.8746111 },
                    new Building { Name = "Ar21", Latitude = 40.224611, Longitude = 28.874222 },
                    new Building { Name = "Ar22", Latitude = 40.223944, Longitude = 28.873722 },
                    new Building { Name = "Ar23", Latitude = 40.2240278, Longitude = 28.8756389 },
                    new Building { Name = "Ar24", Latitude = 40.2297778, Longitude = 28.8721111 },
                    new Building { Name = "Ar25", Latitude = 40.2285, Longitude = 28.870806 },
                    new Building { Name = "Ar26", Latitude = 40.228611, Longitude = 28.870444 },
                    new Building { Name = "Ar27", Latitude = 40.227139, Longitude = 28.859639 },
                    new Building { Name = "Ar28", Latitude = 40.2274723, Longitude = 28.8590556 },
                    new Building { Name = "Ar30", Latitude = 40.2252223, Longitude = 28.8686111 },
                    new Building { Name = "Ar29", Latitude = 40.2239723, Longitude = 28.86225 },
                    new Building { Name = "Ar31", Latitude = 40.2264167, Longitude = 28.8707223 }
                    // --- End of buildings from datas.js ---
                };

                _logger.LogInformation("Adding {Count} buildings", buildings.Count);
                await context.Buildings.AddRangeAsync(buildings, cancellationToken);
                await context.SaveChangesAsync(cancellationToken); // Save buildings to get their IDs

                // Create a dictionary for quick lookup of buildings by name
                // Filter out buildings with null names before creating the dictionary
                var buildingDict = await context.Buildings
                                            .Where(b => b.Name != null)
                                            .ToDictionaryAsync(b => b.Name!, b => b, cancellationToken); // Add ! to b.Name to satisfy notnull constraint

                var connections = new List<BuildingConnection>();

                // Helper to add connections, ensuring buildings exist
                Action<string, string, int, int> addConn = (fromName, toName, distance, traffic) =>
                {
                    if (buildingDict.TryGetValue(fromName, out var fromBuilding) &&
                        buildingDict.TryGetValue(toName, out var toBuilding))
                    {
                        connections.Add(new BuildingConnection
                        {
                            FromBuildingId = fromBuilding.Id,
                            ToBuildingId = toBuilding.Id,
                            Distance = distance,
                            TrafficFactor = traffic
                        });
                    }
                    else
                    {
                        _logger.LogWarning($"Could not create connection: {fromName} -> {toName}. One or both buildings not found in dictionary.");
                    }
                };

                // Manually translated connections from datas.js adjacency object
                addConn("Ar11", "Uü Kütüphane", 150, 2);
                addConn("Ar11", "Ar22", 20, 1);
                addConn("Ar11", "Ar21", 100, 3);
                addConn("Ar11", "Ar2", 200, 5);
                addConn("Uü Kütüphane", "Ar11", 150, 2);
                addConn("Uü Kütüphane", "Ar12", 80, 1); // Corrected: David People was not in buildings list, assuming Ar12
                addConn("David People", "Ar12", 50, 1);
                addConn("David People", "Ar14", 150, 4);
                addConn("Ar12", "David People", 50, 1);
                addConn("Ar12", "Uü Kütüphane", 70, 2); // Corrected distance based on datas.js
                addConn("Ar13", "Ar14", 40, 1);
                addConn("Ar13", "İBF C Blok", 100, 7);
                addConn("Ar14", "Ar13", 40, 1);
                addConn("Ar14", "Ar12", 70, 2);
                addConn("Ar14", "Kampüs Kafe", 200, 6);
                addConn("Ar14", "David People", 150, 4);
                addConn("İBF C Blok", "Ar13", 100, 7);
                addConn("İBF C Blok", "Kampüs Kafe", 200, 8);
                addConn("Kampüs Kafe", "Ar14", 200, 6);
                addConn("Kampüs Kafe", "Ar15", 250, 3);
                addConn("Kampüs Kafe", "Ar24", 100, 3);
                addConn("Ar15", "Kampüs Kafe", 250, 3);
                addConn("Ar15", "Veterinerlik Fak", 100, 2);
                addConn("Veterinerlik Fak", "Ar15", 100, 2);
                addConn("Veterinerlik Fak", "Ar16", 300, 5);
                addConn("Ar16", "Veterinerlik Fak", 300, 5);
                addConn("Ar16", "Endüstri Müh", 120, 4);
                addConn("Ar16", "Ar17", 200, 2);
                addConn("Ar16", "Mimarlık Fak", 85, 4);
                addConn("Endüstri Müh", "Ar16", 120, 4);
                addConn("Endüstri Müh", "Makina Müh", 80, 6);
                addConn("Endüstri Müh", "Mimarlık Fak", 90, 3);
                addConn("Makina Müh", "Endüstri Müh", 80, 6);
                addConn("Makina Müh", "Otomotiv Müh", 70, 4);
                addConn("Makina Müh", "Mimarlık Fak", 70, 4);
                addConn("Mimarlık Fak", "Endüstri Müh", 90, 3);
                addConn("Mimarlık Fak", "Makina Müh", 70, 4);
                addConn("Mimarlık Fak", "Ar16", 85, 4);
                addConn("Otomotiv Müh", "Makina Müh", 70, 4);
                addConn("Otomotiv Müh", "İBF A ve B blok", 150, 7);
                addConn("İBF A ve B blok", "Otomotiv Müh", 150, 7);
                addConn("Ar17", "Çevre Müh", 80, 2);
                addConn("Ar17", "Ar16", 200, 2);
                addConn("Çevre Müh", "Ar17", 80, 2);
                addConn("Çevre Müh", "Makina topl.", 60, 3);
                addConn("Çevre Müh", "Ar18", 60, 2);
                addConn("Makina topl.", "Çevre Müh", 60, 3);
                addConn("Makina topl.", "UET", 70, 4);
                addConn("UET", "Makina topl.", 70, 4);
                addConn("UET", "Ar18", 90, 5);
                addConn("Ar18", "UET", 90, 5);
                addConn("Ar18", "Ar19", 50, 2);
                addConn("Ar18", "Makina topl.", 70, 3);
                addConn("Ar18", "Çevre Müh", 60, 2);
                addConn("Ar19", "Ar18", 50, 2);
                addConn("Ar19", "Sağlık yüksek Okulu", 120, 6);
                addConn("Sağlık yüksek Okulu", "Ar19", 120, 6);
                addConn("Sağlık yüksek Okulu", "Elektrik Tekstil Bilgisayar Müh", 100, 7);
                addConn("Elektrik Tekstil Bilgisayar Müh", "Sağlık yüksek Okulu", 100, 7);
                addConn("Elektrik Tekstil Bilgisayar Müh", "Ar20", 60, 3);
                addConn("Ar20", "Elektrik Tekstil Bilgisayar Müh", 60, 3);
                addConn("Ar20", "Ar21", 50, 2);
                addConn("Ar20", "Ar23", 80, 4);
                addConn("Ar21", "Ar20", 50, 2);
                addConn("Ar21", "Ar11", 100, 3);
                addConn("Ar22", "Ar11", 120, 1);
                addConn("Ar23", "Eğitim Fak", 80, 6);
                addConn("Ar23", "Ar20", 80, 4);
                addConn("Ar23", "İnşaat Fak", 300, 4);
                addConn("Eğitim Fak", "Ar23", 80, 6);
                addConn("İnşaat Fak", "Ar23", 300, 4);
                addConn("üniversite ana giriş", "Ar1", 200, 9);
                addConn("Ar1", "üniversite ana giriş", 200, 9);
                addConn("Ar1", "Uü Güzel Sanatlar Fakültesi", 250, 5);
                addConn("Ar1", "Ar2", 300, 2);
                addConn("Uü Güzel Sanatlar Fakültesi", "Ar1", 250, 5);
                addConn("Uü Güzel Sanatlar Fakültesi", "Ar2", 220, 3);
                addConn("Ar2", "Uü Güzel Sanatlar Fakültesi", 220, 3);
                addConn("Ar2", "Ar3", 270, 4);
                addConn("Ar2", "Ar1", 300, 2);
                addConn("Ar2", "Ar11", 200, 5);
                addConn("Ar3", "Ar2", 270, 4);
                addConn("Ar3", "Afet Acil Durum", 100, 1);
                addConn("Ar3", "Uü Camii", 200, 4);
                addConn("Ar3", "Ar10", 210, 6);
                addConn("Uü Camii", "Afet Acil Durum", 250, 4);
                addConn("Uü Camii", "Ar4", 200, 2);
                addConn("Uü Camii", "Ar3", 200, 4);
                addConn("Ar4", "Uü Camii", 200, 2);
                addConn("Ar4", "Göz Hastanesi", 180, 3);
                addConn("Ar4", "Ar5", 220, 2);
                addConn("Göz Hastanesi", "Ar4", 180, 3);
                addConn("Göz Hastanesi", "Ar5", 220, 5);
                addConn("Ar5", "Göz Hastanesi", 220, 5);
                addConn("Ar5", "Mete Cengiz", 180, 4);
                addConn("Ar5", "Rektörlük", 220, 6);
                addConn("Mete Cengiz", "Ar5", 180, 4);
                addConn("Mete Cengiz", "Ar9", 180, 3);
                addConn("Fen Fak", "Ar8", 160, 4);
                addConn("Fen Fak", "Ar9", 250, 3);
                addConn("Ar8", "Fen Fak", 160, 4);
                addConn("Ar8", "Uü Derslik ve Merkez Birimler", 170, 7);
                addConn("Ar8", "Ar29", 125, 2);
                addConn("Uü Derslik ve Merkez Birimler", "Ar8", 170, 7);
                addConn("Uü Derslik ve Merkez Birimler", "Ar6", 150, 5);
                addConn("Ar6", "Uü Derslik ve Merkez Birimler", 150, 5);
                addConn("Ar6", "Uni+Sports", 180, 3);
                addConn("Ar6", "Ar7", 200, 2);
                addConn("Uni+Sports", "Ar6", 180, 3);
                addConn("Uni+Sports", "Metro", 160, 7);
                addConn("Metro", "Uni+Sports", 160, 7);
                addConn("Metro", "Tıp Fakültesi", 150, 8);
                addConn("Metro", "Ar10", 150, 6);
                addConn("Tıp Fakültesi", "Metro", 150, 8);
                addConn("Tıp Fakültesi", "Rektörlük", 250, 7);
                addConn("Tıp Fakültesi", "Ar10", 90, 4);
                addConn("Rektörlük", "Tıp Fakültesi", 250, 7);
                addConn("Rektörlük", "Ar9", 200, 5);
                addConn("Rektörlük", "Ar5", 220, 6);
                addConn("Ar9", "Rektörlük", 200, 5);
                addConn("Ar9", "Mete Cengiz", 180, 3);
                addConn("Ar9", "Fen Fak", 250, 3);
                addConn("Daichii  Arge", "Ar7", 180, 1);
                addConn("Daichii  Arge", "Ulutek", 180, 2);
                addConn("Ar7", "Daichii  Arge", 180, 1);
                addConn("Ar7", "Ar6", 200, 2);
                addConn("Ulutek", "Daichii  Arge", 180, 2);
                addConn("Ar10", "Uü Camii", 120, 3);
                addConn("Ar10", "Tıp Fakültesi", 90, 4);
                addConn("Ar10", "Ar3", 210, 6);
                addConn("Ar10", "Metro", 150, 6);
                addConn("Ar24", "Kampüs Kafe", 100, 3);
                addConn("Ar24", "Yurtlar Bölg", 200, 8);
                addConn("Yurtlar Bölg", "Ar24", 200, 8);
                addConn("Yurtlar Bölg", "Ar25", 50, 9);
                addConn("Ar25", "Yurtlar Bölg", 50, 9);
                addConn("Ar25", "Ar26", 60, 4);
                addConn("Kestirme", "Ar26", 200, 2);
                addConn("Kestirme", "Güzel sanatlar", 100, 3);
                addConn("Kestirme", "ZiraatMühendisliği", 145, 4);
                addConn("Ar26", "Kestirme", 200, 2);
                addConn("Ar26", "Ar25", 60, 4);
                addConn("Güzel sanatlar", "Ar27", 100, 2);
                addConn("Güzel sanatlar", "Kestirme", 100, 3);
                addConn("Ar27", "Güzel sanatlar", 100, 2);
                addConn("Ar27", "Ar28", 100, 1);
                addConn("Ar28", "Ar27", 100, 1);
                addConn("Ar28", "Besaş", 50, 3);
                addConn("Besaş", "Ar28", 50, 3);
                addConn("Besaş", "Çıkış", 200, 9);
                addConn("Çıkış", "Besaş", 200, 9);
                addConn("Ar29", "BasımEviMüdürlüğü", 40, 1);
                addConn("Ar29", "Ar8", 125, 2);
                addConn("BasımEviMüdürlüğü", "Ar29", 40, 1);
                addConn("BasımEviMüdürlüğü", "ZiraatMühendisliği", 95, 2);
                addConn("ZiraatMühendisliği", "BasımEviMüdürlüğü", 95, 2);
                addConn("ZiraatMühendisliği", "Kestirme", 145, 4);
                addConn("Ar30", "Ar9", 100, 2);
                addConn("Ar30", "Ar31", 125, 3);
                addConn("Ar31", "Ar30", 125, 3);
                addConn("Ar31", "Ar25", 125, 5);
                addConn("Ar31", "Ar13", 120, 4);
                addConn("Afet Acil Durum", "Ar3", 100, 1);
                addConn("Afet Acil Durum", "Uü Camii", 250, 4);

                _logger.LogInformation("Adding {Count} building connections", connections.Count);
                await context.BuildingConnections.AddRangeAsync(connections, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                // Seed User Presence Data
                if (!await context.UserPresences.AnyAsync(cancellationToken))
                {
                    _logger.LogInformation("Seeding dummy user presence data...");
                    var userPresences = new List<UserPresence>();

                    // Helper to add user presences, ensuring buildings exist
                    Action<string, string> addUserPresence = (userId, buildingName) =>
                    {
                        if (buildingDict.TryGetValue(buildingName, out var building))
                        {
                            userPresences.Add(new UserPresence
                            {
                                UserId = userId,
                                CurrentBuildingId = building.Id,
                                LastSeen = DateTime.UtcNow
                            });
                        }
                        else
                        {
                            _logger.LogWarning($"Could not create user presence for {userId} at {buildingName}. Building not found.");
                        }
                    };

                    addUserPresence("User1", "Kampüs Kafe");
                    addUserPresence("User2", "Uü Kütüphane");
                    addUserPresence("User3", "İBF C Blok");
                    addUserPresence("User4", "Metro");
                    addUserPresence("User5", "Fen Fak");

                    if (userPresences.Any())
                    {
                        _logger.LogInformation("Adding {Count} user presence records", userPresences.Count);
                        await context.UserPresences.AddRangeAsync(userPresences, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        _logger.LogInformation("No user presence records to add (possibly due to missing buildings).");
                    }
                }
                else
                {
                    _logger.LogInformation("User presence data already exists. Skipping seed.");
                }

                _logger.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding data: {Message}", ex.Message);
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}