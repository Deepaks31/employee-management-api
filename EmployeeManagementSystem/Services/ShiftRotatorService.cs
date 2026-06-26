using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Azure.Identity;
using Azure.Core;

namespace EmployeeManagementSystem.Services
{
    public class ShiftRotatorService : BackgroundService
    {
        private readonly ILogger<ShiftRotatorService> _logger;
        private readonly IConfiguration _configuration;

        public ShiftRotatorService(ILogger<ShiftRotatorService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Shift Rotator Service (M2M) is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Shift Rotator requesting M2M token at: {time}", DateTimeOffset.Now);

                    string tenantId = _configuration["MicrosoftGraph:TenantId"];
                    string clientId = _configuration["MicrosoftGraph:ClientId"];
                    string clientSecret = _configuration["MicrosoftGraph:ClientSecret"];

                    // 1. Authenticate with Azure AD as the "Robot"
                    var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                    
                    // Request a token specifically scoped to our own API
                    var tokenRequestContext = new TokenRequestContext(new[] { $"api://{clientId}/.default" });
                    var token = await credential.GetTokenAsync(tokenRequestContext, stoppingToken);

                    // 2. Make an HTTP POST request to our Web API
                    using var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                    
                    // We read the URL dynamically from .env or appsettings!
                    string apiUrl = _configuration["ApiBaseUrl"]?.TrimEnd('/');
                    var response = await httpClient.PostAsync($"{apiUrl}/api/Employees/rotate-shifts", null, stoppingToken);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Shift rotation completed successfully via Web API!");
                    }
                    else
                    {
                        string errorBody = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Failed to rotate shifts via API. Status Code: {response.StatusCode}. Error: {errorBody}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during the M2M shift rotation process.");
                }

                // Wait 15 seconds before running again for testing purposes
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
    }
}
