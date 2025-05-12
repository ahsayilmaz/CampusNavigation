using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CampusNavigation.Data;
using CampusNavigation.Models;

namespace CampusNavigation.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(ApplicationDbContext context, ILogger<DatabaseService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Building>> GetAllItemsAsync()
        {
            _logger.LogInformation("Getting all buildings");
            return await _context.Buildings.ToListAsync();
        }

        public async Task<Building?> GetItemByIdAsync(int id) 
        {
            _logger.LogInformation("Getting building with id {Id}", id);
            return await _context.Buildings.FindAsync(id);
        }

        public async Task<Building?> CreateItemAsync(Building item) 
        {
            if (item == null)
            {
                _logger.LogWarning("Attempted to create a null building item.");
                return null; 
            }
            _logger.LogInformation("Creating new building with Name: {Name}", item.Name);
            _context.Buildings.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> UpdateItemAsync(Building item)
        {
            _logger.LogInformation("Updating building with id {Id}", item.Id);
            _context.Entry(item).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Update failed for building {Id}", item.Id);
                return false;
            }
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            _logger.LogInformation("Deleting building with id {Id}", id);
            var item = await _context.Buildings.FindAsync(id);
            if (item == null)
            {
                return false;
            }

            _context.Buildings.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Dictionary<int, int>> GetUserCountsPerBuildingAsync()
        {
            _logger.LogInformation("Getting user counts per building");
            try
            {
                return await _context.UserPresences
                    .Where(up => up.CurrentBuildingId.HasValue) 
                    .Select(up => new { BuildingId = up.CurrentBuildingId!.Value, UserId = up.UserId }) 
                    .GroupBy(up => up.BuildingId) 
                    .Select(g => new { BuildingId = g.Key, UserCount = g.Count() })
                    .ToDictionaryAsync(x => x.BuildingId, x => x.UserCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user counts per building");
                return new Dictionary<int, int>(); 
            }
        }
    }
}