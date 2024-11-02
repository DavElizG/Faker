using Api.Domain.Interfaces.Generators;
using Api.Domain.Interfaces;
using Api.Domain.Interfaces.Infraestructure;
using Api.Infrastructure.ErrorLog;
using Api.Infrastructure.EventSource;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api.Application.Services;
using Api.Application.Services.FakeDataGenerators;
using Api.Domain.Entities;

namespace Api.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(new List<Api.Domain.Entities.Affiliate>());
            services.AddSingleton(new List<Api.Domain.Entities.Card>());

            services.AddSingleton<IEventSource, EventSourceService>();
            services.AddSingleton<IErrorLogService, ErrorLogService>();
          
            services.AddScoped<IErrorHandlingService, ErrorHandlingService>();
            services.AddScoped<IPurchaseRetryService, PurchaseRetryService>();
            services.AddScoped<IPurchaseSimulationService, PurchaseSimulationService>();
            services.AddScoped<IProductGeneratorService, FakeProductGeneratorService>();
            services.AddScoped<ICardGeneratorService, FakeCardGeneratorService>();
            services.AddScoped<ICardModificationService, CardModificationService>();
            services.AddScoped<IAffiliateGeneratorService, FakeAffiliateGeneratorService>();
            services.AddSingleton(new List<Affiliate>());
            services.AddSingleton(new List<Card>());

            services.AddScoped<IPurchaseRetryService, PurchaseRetryService>();
            services.AddScoped<ICardModificationService, CardModificationService>();

            /////faltaban estos servicios
            services.AddScoped<IPurchaseSimulationService, PurchaseSimulationService>();
            services.AddHostedService<PurchaseGenerationBackgroundService>();

            return services;
        }
    }
}
