using Api.Domain.Interfaces.Generators;
using Api.Domain.Interfaces;
using Api.Domain.Interfaces.Infraestructure;
using Api.Infrastructure.ErrorLog;
using Api.Infrastructure.EventSource;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Api.Application.Services;
using Api.Application.Services.FakeDataGenerators;
using Api.Domain.Entities;

namespace Api.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Registros de instancias singleton
            services.AddSingleton(new List<Affiliate>());
            services.AddSingleton(new List<Card>());
            services.AddSingleton<IEventSource, EventSourceService>();
            services.AddSingleton<IErrorLogService, ErrorLogService>();
            services.AddSingleton<IFailedPurchaseStore, FailedPurchaseStore>();

            // Registros de servicios scoped
            services.AddScoped<IErrorHandlingService, ErrorHandlingService>();
            services.AddScoped<IPurchaseSimulationService, PurchaseSimulationService>();
            services.AddScoped<IPurchaseRetryService, PurchaseRetryService>();
            services.AddScoped<IProductGeneratorService, FakeProductGeneratorService>();
            services.AddScoped<ICardGeneratorService, FakeCardGeneratorService>();
            services.AddScoped<ICardModificationService, CardModificationService>();
            services.AddScoped<IAffiliateGeneratorService, FakeAffiliateGeneratorService>();

            // Registro del servicio de fondo para la generación automática de compras
            services.AddHostedService<PurchaseGenerationBackgroundService>();

            return services;
        }
    }
}