using api.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace api.Core.Extensions;

public static class DbContextExtensions
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlServerOptions =>
                    sqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
            ));

        return services;
    }

    public static async Task MigrateDatabase(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
    }
}