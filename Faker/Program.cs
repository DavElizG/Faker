using Api.Application.Services;
using Api.Application.Services.FakeDataGenerators;
using Api.Domain.Interfaces.Infraestructure;
using Api.Domain.Interfaces;
using Api.Infrastructure.DependencyInjection;
using Api.Infrastructure.ErrorLog;
using Api.Infrastructure.EventSource;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureServices(builder.Configuration);
//Funcional
// Registrar HttpClient para IErrorLogService
builder.Services.AddHttpClient<IErrorLogService, ErrorLogService>();



// Resto de la configuración de servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "NgOrigins",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        });
});

var app = builder.Build();

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