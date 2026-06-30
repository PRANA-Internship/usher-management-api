using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

using Scalar.AspNetCore;

using UMS.Application;
using UMS.Infrastructure.BackgroundServices;
using UMS.Infrastructure.Hubs;
using UMS.Infrastructure.Persistance;
using UMS.Infrastructure.Persistance.Context;
using UMS.Infrastructure.Persistence.Seeder;
using UMS.Infrastructure.Settings;
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

var rateLimiterSettings = builder.Configuration
    .GetSection(RateLimiterSettings.SectionName)
    .Get<RateLimiterSettings>()
    ?? throw new InvalidOperationException("RateLimiter configuration is missing.");

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(
        rateLimiterSettings.PolicyName,
        opt =>
        {
            opt.PermitLimit = rateLimiterSettings.PermitLimit;
            opt.Window = TimeSpan.FromMinutes(rateLimiterSettings.WindowMinutes);
            opt.QueueLimit = rateLimiterSettings.QueueLimit;
        }
    );
});
builder.Services.AddApplication();
builder.Services.AddControllers();

builder.Services.AddSignalR();

builder.Services.AddHostedService<ReportCacheRefreshWorker>();

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
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications").DisableRateLimiting();
app.Run();