using Api.Application.Services;
using Api.Application.Services.FakeDataGenerators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the custom services
builder.Services.AddSingleton<FakeAffiliateGeneratorService>();
builder.Services.AddSingleton<FakeCardGeneratorService>(); // Register the card generator service

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
app.UseCors("NgOrigins");

app.MapControllers();

app.Run();
