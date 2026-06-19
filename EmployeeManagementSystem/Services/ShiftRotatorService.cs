using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Azure.Identity;
using Microsoft.Graph;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Repositories;

namespace EmployeeManagementSystem.Services
{
    public class ShiftRotatorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ShiftRotatorService> _logger;
        private readonly IConfiguration _configuration;

        public ShiftRotatorService(IServiceProvider serviceProvider, ILogger<ShiftRotatorService> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Shift Rotator Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Shift Rotator is running at: {time}", DateTimeOffset.Now);

                    // We are skipping the Azure Graph API call because Admin Consent is missing.
                    // Instead, we will just rotate the shifts directly in the local database!

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var allEmployees = await unitOfWork.Employees.GetAllAsync();

                        var shifts = new[] { "Morning", "Evening", "Night" };

                        foreach (var employee in allEmployees)
                        {
                            // Randomly assign a shift
                            string newShift = shifts[Random.Shared.Next(shifts.Length)];

                            // Optional: Ensure it actually changes (doesn't randomly pick the same shift twice)
                            while (newShift == employee.CurrentShift)
                            {
                                newShift = shifts[Random.Shared.Next(shifts.Length)];
                            }

                            employee.CurrentShift = newShift;
                            unitOfWork.Employees.Update(employee);
                            _logger.LogInformation($"Randomly assigned shift for {employee.Name} to {newShift}.");
                        }

                        await unitOfWork.CompleteAsync();
                        _logger.LogInformation("Shift rotation completed successfully and saved to database.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during the shift rotation process.");
                }

                // Wait 24 hours before running again (set to 15 seconds for testing)
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
    }
}
