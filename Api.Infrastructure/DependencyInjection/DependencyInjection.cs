using Api.Domain.Interfaces.Generators;
using Api.Domain.Interfaces;
using Api.Domain.Interfaces.Infraestructure;
using Api.Infrastructure.ErrorLog;
using Api.Infrastructure.EventSource;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
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
            // Registrar listas compartidas para datos ficticios
            services.AddSingleton(new List<Affiliate>());
            services.AddSingleton(new List<Card>());

            // Configurar servicios de infraestructura
            services.AddSingleton<IEventSource, EventSourceService>();
            services.AddSingleton<IErrorLogService, ErrorLogService>();

            // Configurar HttpClient para ErrorHandlingService
            services.AddHttpClient<IErrorHandlingService, ErrorHandlingService>()
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = new Uri(configuration["ErrorService:Url"]);
                });

            // Registrar los servicios de aplicación como Singleton
            services.AddSingleton<IPurchaseSimulationService, PurchaseSimulationService>();
            services.AddSingleton<IPurchaseRetryService, PurchaseRetryService>();
            services.AddSingleton<IProductGeneratorService, FakeProductGeneratorService>();
            services.AddSingleton<ICardGeneratorService, FakeCardGeneratorService>();
            services.AddSingleton<ICardModificationService, CardModificationService>();
            services.AddSingleton<IAffiliateGeneratorService, FakeAffiliateGeneratorService>();

            return services;
        }
    }
}
