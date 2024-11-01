using Api.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Application.Services
{
    public class PurchaseGenerationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PurchaseGenerationBackgroundService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);


        public PurchaseGenerationBackgroundService(IServiceProvider serviceProvider, ILogger<PurchaseGenerationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PurchaseGenerationBackgroundService iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Crear un nuevo scope para cada ejecución
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var purchaseSimulationService = scope.ServiceProvider.GetRequiredService<IPurchaseSimulationService>();
                        await purchaseSimulationService.SimulatePurchases();
                    }

                    _logger.LogInformation("Compras generadas automáticamente.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al generar compras automáticamente.");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("PurchaseGenerationBackgroundService detenido.");
        }
    }
}
