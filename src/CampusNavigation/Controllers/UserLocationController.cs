// Controllers/UserLocationController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CampusNavigation.Data;
using CampusNavigation.Models;

namespace CampusNavigation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserLocationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserLocationController> _logger;

        public UserLocationController(ApplicationDbContext context, ILogger<UserLocationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/UserLocation
        [HttpPost]
        public async Task<IActionResult> PostUserLocation([FromBody] UserLocation userLocation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Generate a user ID if not provided
            if (string.IsNullOrEmpty(userLocation.UserId))
            {
                userLocation.UserId = Guid.NewGuid().ToString();
            }

            userLocation.Timestamp = DateTime.UtcNow;

            _context.UserLocations.Add(userLocation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserLocation), new { id = userLocation.Id }, userLocation);
        }

        // GET: api/UserLocation/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserLocation>> GetUserLocation(int id)
        {
            var userLocation = await _context.UserLocations.FindAsync(id);

            if (userLocation == null)
            {
                return NotFound();
            }

            return userLocation;
        }

        // GET: api/UserLocation/density
        [HttpGet("density")]
        public async Task<ActionResult<object>> GetUserDensity()
        {
            // Only consider locations from the past 10 minutes
            var cutoffTime = DateTime.UtcNow.AddMinutes(-10);
            
            var recentLocations = await _context.UserLocations
                .Where(ul => ul.Timestamp > cutoffTime)
                .ToListAsync();

            // Calculate node density (buildings)
            var nodeDensity = recentLocations
                .Where(loc => !string.IsNullOrEmpty(loc.CurrentNode))
                .GroupBy(loc => loc.CurrentNode)
                .ToDictionary(group => group.Key!, group => group.Count());

            // Calculate edge density (paths between buildings)
            var edgeDensity = recentLocations
                .Where(loc => !string.IsNullOrEmpty(loc.CurrentEdge))
                .GroupBy(loc => loc.CurrentEdge)
                .ToDictionary(group => group.Key!, group => group.Count());

            return new { nodes = nodeDensity, edges = edgeDensity };
        }

        // For development: Fill with dummy data
        [HttpPost("dummy-data")]
        public async Task<IActionResult> GenerateDummyData()
        {
            try
            {
                // Get all buildings and connections
                // We need to include the Building details for FromBuilding and ToBuilding to get their names
                var connections = await _context.BuildingConnections
                                                .Include(c => c.FromBuilding)
                                                .Include(c => c.ToBuilding)
                                                .Where(c => c.FromBuilding != null && c.FromBuilding.Name != null && 
                                                             c.ToBuilding != null && c.ToBuilding.Name != null &&
                                                             !c.FromBuilding.Name.StartsWith("Ar") && 
                                                             !c.ToBuilding.Name.StartsWith("Ar"))
                                                .ToListAsync();

                if (!connections.Any())
                {
                    _logger.LogWarning("No valid connections found to generate dummy edge data. Ensure connections exist between non-'Ar' buildings.");
                    return Ok(new { message = "No valid connections found to generate dummy data for edges." });
                }
                
                var random = new Random();
                var dummyLocations = new List<UserLocation>();
                
                // Generate 10 dummy users on edges
                int userCount = 10;
                
                for (int i = 0; i < userCount; i++)
                {
                    // Create a unique ID for each new dummy user location entry
                    // This avoids conflicts if the button is clicked multiple times.
                    // We can use a simpler counter or a more robust unique ID generation if needed.
                    string uniqueSuffix = DateTime.UtcNow.Ticks.ToString() + "_" + i.ToString();
                    string userId = $"edge_user_{uniqueSuffix}";
                    
                    // Place on a random valid path (edge)
                    var connection = connections[random.Next(connections.Count)];
                    
                    // Ensure FromBuilding and ToBuilding are not null (already filtered, but good practice)
                    if (connection.FromBuilding == null || connection.ToBuilding == null) continue;

                    var edge = $"{connection.FromBuilding.Name}|{connection.ToBuilding.Name}";
                    dummyLocations.Add(new UserLocation
                    {
                        UserId = userId,
                        CurrentNode = null,
                        CurrentEdge = edge,
                        Timestamp = DateTime.UtcNow
                    });
                }
                
                if (dummyLocations.Any())
                {
                    await _context.UserLocations.AddRangeAsync(dummyLocations);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Generated {dummyLocations.Count} new dummy user locations on edges.");
                    return Ok(new { message = $"Generated {dummyLocations.Count} new dummy user locations on edges." });
                }
                else
                {
                    _logger.LogInformation("No dummy locations were generated in this run (possibly no valid connections).");
                    return Ok(new { message = "No dummy locations were generated in this run." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating dummy data");
                return StatusCode(500, new { error = "Error generating dummy data" });
            }
        }
    }
}