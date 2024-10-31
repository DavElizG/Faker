using Api.Application.Services;
using Api.Application.Services.FakeDataGenerators;
using Api.Domain.Interfaces.Infraestructure;
using Api.Domain.Interfaces;
using Api.Infrastructure.DependencyInjection;
using Api.Infrastructure.ErrorLog;
using Api.Infrastructure.EventSource;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureServices(builder.Configuration);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuración de servicios
builder.Services.AddScoped<IPurchaseSimulationService, PurchaseSimulationService>();
builder.Services.AddHostedService<PurchaseGenerationBackgroundService>();
builder.Services.AddSingleton<IEventSource, EventSourceService>();

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "NgOrigins",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        });
});

var app = builder.Build();

// Configuración del pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors("NgOrigins");

app.MapControllers();

app.Run();
