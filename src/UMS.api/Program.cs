using UMS.Infrastructure.Persistance;
using UMS.Application;

using Microsoft.AspNetCore.RateLimiting;
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
    app.UseOpenApi();

    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/swagger/v1/swagger.json";
    });
}
// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();

app.Run();
