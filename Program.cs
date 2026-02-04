using api.Core.Extensions;
using api.Modules.Auth.Extensions;
using api.Modules.Email.Extensions;
using api.Modules.Localization.Extensions;
using api.Modules.Localization.Resources;
using api.Modules.Notifications.Interfaces.Services;
using api.Modules.Notifications.Services;
using api.Modules.Traces.Extensions;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddForwardedHeaders(builder.Configuration);

builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen();

// Add the queue and background services
builder.Services.AddSingleton<INotificationsQueueService, NotificationsQueueService>();
builder.Services.AddHostedService<NotificationsQueueProcessorService>();

// Add SignalR services
builder.Services.AddSignalR();
builder.Services.AddSignalRServices();

// Add Localization configuration
builder.Services.AddLocalizationConfiguration();
builder.Services.AddSingleton<SharedResource>();

// Add database configuration
builder.Services.AddDatabaseConfiguration(builder.Configuration);

// Add app services
builder.Services.AddApplicationServices();

// Cookie-based authentication configuration
builder.Services.AddCookieAuthentication(builder.Configuration);

//Add authorization
builder.Services.AddCustomAuthorizationPolicies();

// Add Cross-Site Resource Sharing configuration
builder.Services.AddCorsConfiguration(builder.Configuration, builder.Environment);

// Add email configuration
builder.Services.AddEmailConfiguration(builder.Configuration);

// Add Rate-Limiting configuration
builder.Services.AddRateLimiting();

// Add Output Cache configuration
//builder.Services.AddOutputCache(builder.Configuration);

var app = builder.Build(); // Build the app after adding services

await app.MigrateDatabase();

// Enables UseForwardedHeaders middleware
app.UseForwardedHeaders();

// Add the localization middleware
app.UseLocalizationConfiguration();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "storage")),
    RequestPath = "/uploads"
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger(); 
    app.UseSwaggerUI();
}

// Enables routing
app.UseRouting();

// Enables rate limiting
app.UseRateLimiter();

// Apply the Cross-Site Resource Sharing policy
app.UseCors("AllowSpecificOrigin");

// Enables authentication
app.UseAuthentication();

// Enables authorization
app.UseAuthorization();

// Map controllers
app.MapGroup("/api/v1/").MapControllers();

// Add Output Cache middleware
//app.UseOutputCache();

// Map SignalR hubs
app.UseSignalREndpoints();

app.Run();