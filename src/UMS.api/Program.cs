using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using UMS.Application;
using UMS.Infrastructure.Persistance;
using UMS.Infrastructure.Persistance.Context;
using UMS.Infrastructure.Persistence.Seeder;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApiDocument(config =>
{
    //for documenting api
    config.DocumentName = "v1";
    config.Title = "UMS API";
    config.Version = "v1";
});

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {

        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters
            .Add(new JsonStringEnumConverter());
    });

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(
        "fixed",
        opt =>
        {
            opt.PermitLimit = 5;
            opt.Window = TimeSpan.FromMinutes(10);
            opt.QueueLimit = 0;
        }
    );
});
builder.Services.AddApplication();
builder.Services.AddControllers();


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    app.UseOpenApi(options =>
    {
        options.Path = "/openapi/{documentName}.json";
    });
    app.MapScalarApiReference();

    await AdminSeeder.SeedAsync(app.Services);
}
// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();

app.Run();
