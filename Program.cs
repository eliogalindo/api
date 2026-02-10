using Asp.Versioning;
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

builder.Services.AddApiVersioning(options =>
{
    // Sets the default version e.g. 1.0
    options.DefaultApiVersion = new ApiVersion(1, 0);

    // Reports the supported versions on the response headers (api-supported-versions)
    options.ReportApiVersions = true;

    // If the client doesn't specify the version, sets the default
    options.AssumeDefaultVersionWhenUnspecified = true;

    // Reads the version from the URL (e.g.: /v1/...) or from the Header (X-Api-Version)
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version")
    );
})
.AddApiExplorer(options =>
{
    // Format group for Swagger (e.g.: 'v1', 'v2')
    options.GroupNameFormat = "'v'VVV";

    // Replace the {version} parameter for Swagger routes
    options.SubstituteApiVersionInUrl = true;
});

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

// Runs migrations on application startup
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
app.MapControllers();

// Add Output Cache middleware
//app.UseOutputCache();

// Map SignalR hubs
app.UseSignalREndpoints();

app.Run();