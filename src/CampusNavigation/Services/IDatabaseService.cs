using System.Collections.Generic;
using System.Threading.Tasks;
using CampusNavigation.Models;

namespace CampusNavigation.Services
{
    public interface IDatabaseService
    {
        Task<IEnumerable<Building>> GetAllItemsAsync();
        Task<Building?> GetItemByIdAsync(int id); // Changed to Building?
        Task<Building?> CreateItemAsync(Building item); // Changed to Building? as it might return null if creation fails or item is null
        Task<bool> UpdateItemAsync(Building item);
        Task<bool> DeleteItemAsync(int id);
        Task<Dictionary<int, int>> GetUserCountsPerBuildingAsync(); // Added for user presence
    }
}