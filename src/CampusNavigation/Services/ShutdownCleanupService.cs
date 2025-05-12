// filepath: c:\\Users\\ahsay\\OneDrive\\Masaüstü\\CampusNavigation\\src\\CampusNavigation\\Services\\ShutdownCleanupService.cs
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using CampusNavigation.Data;
using Microsoft.EntityFrameworkCore;

namespace CampusNavigation.Services
{
    public class ShutdownCleanupService : IHostedService
    {
        private readonly ILogger<ShutdownCleanupService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ShutdownCleanupService(ILogger<ShutdownCleanupService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Shutdown Cleanup Service is starting.");
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Shutdown Cleanup Service is stopping. Cleaning up data...");

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    int presencesCleared = await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM UserPresences", cancellationToken);
                    _logger.LogInformation($"Cleared {presencesCleared} records from UserPresences table.");

                    int locationsCleared = await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM UserLocations", cancellationToken);
                    _logger.LogInformation($"Cleared {locationsCleared} records from UserLocations table.");
                }
                _logger.LogInformation("Data cleanup completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during shutdown data cleanup.");
            }
        }
    }
}
