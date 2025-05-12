using System.Collections.Generic;
using System.Threading.Tasks;
using CampusNavigation.Models;

namespace CampusNavigation.Services
{
    public interface IDatabaseService
    {
        Task<IEnumerable<Building>> GetAllItemsAsync();
        Task<Building?> GetItemByIdAsync(int id);
        Task<Building?> CreateItemAsync(Building item);
        Task<bool> UpdateItemAsync(Building item);
        Task<bool> DeleteItemAsync(int id);
        Task<Dictionary<int, int>> GetUserCountsPerBuildingAsync();
    }
}