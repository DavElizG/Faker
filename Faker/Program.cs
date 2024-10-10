using Azure.Messaging.ServiceBus;

var builder = WebApplication.CreateBuilder(args);


// Leer la cadena de conexión del Event Storage desde appsettings.json
var eventStorageConnectionString = builder.Configuration["EventStorage:ConnectionString"];

// Registrar ServiceBusClient para Azure Service Bus
builder.Services.AddSingleton(new ServiceBusClient(eventStorageConnectionString));


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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




// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors("NgOrigins");

app.Run();
