using CampusNavigation.Data;
using CampusNavigation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CampusNavigation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampusController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CampusController> _logger;
        private readonly Services.IDatabaseService _databaseService;

        public CampusController(ApplicationDbContext context, ILogger<CampusController> logger, Services.IDatabaseService databaseService)
        {
            _context = context;
            _logger = logger;
            _databaseService = databaseService;
        }

        [HttpGet("buildings")]
        public async Task<ActionResult<IEnumerable<object>>> GetBuildings()
        {
            try
            {
                var buildings = await _context.Buildings
                    .Select(b => new {
                        id = b.Id,
                        name = b.Name,
                        latitude = b.Latitude,
                        longitude = b.Longitude
                    })
                    .ToListAsync();

                return Ok(buildings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving buildings");
                return Problem(
                    detail: ex.Message,
                    title: "Failed to retrieve buildings",
                    statusCode: 500
                );
            }
        }

        [HttpGet("connections")]
        public async Task<ActionResult<object>> GetConnections()
        {
            try
            {
                var buildings = await _context.Buildings.ToDictionaryAsync(b => b.Id, b => b);
                var connections = await _context.BuildingConnections.ToListAsync();

                var adjacencyDict = new Dictionary<string, Dictionary<string, object>>();

                foreach (var connection in connections)
                {
                    if (!buildings.TryGetValue(connection.FromBuildingId, out var fromBuilding) ||
                        !buildings.TryGetValue(connection.ToBuildingId, out var toBuilding))
                    {
                        _logger.LogWarning("Connection {id} references missing buildings: From={fromId}, To={toId}",
                            connection.Id, connection.FromBuildingId, connection.ToBuildingId);
                        continue;
                    }

                    var fromName = fromBuilding.Name;
                    var toName = toBuilding.Name;

                    if (string.IsNullOrEmpty(fromName) || string.IsNullOrEmpty(toName))
                    {
                        _logger.LogWarning("Connection {id} has buildings with null or empty names: From='{fromName}', To='{toName}'",
                            connection.Id, fromName, toName);
                        continue;
                    }

                    if (!adjacencyDict.ContainsKey(fromName))
                    {
                        adjacencyDict[fromName] = new Dictionary<string, object>();
                    }

                    adjacencyDict[fromName][toName] = new {
                        distance = connection.Distance,
                        traffic = connection.TrafficFactor
                    };
                }

                return Ok(adjacencyDict);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving connections");
                return Ok(new Dictionary<string, Dictionary<string, object>>());
            }
        }

        [HttpGet("building-user-counts")]
        public async Task<ActionResult<Dictionary<int, int>>> GetBuildingUserCounts()
        {
            try
            {
                var userCounts = await _databaseService.GetUserCountsPerBuildingAsync();
                return Ok(userCounts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving building user counts");
                return Problem(
                    detail: ex.Message,
                    title: "Failed to retrieve building user counts",
                    statusCode: 500
                );
            }
        }
    }
}