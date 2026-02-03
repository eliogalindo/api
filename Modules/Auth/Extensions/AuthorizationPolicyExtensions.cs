using System.Text.Json;
using api.Modules.Auth.Configurations;

namespace api.Modules.Auth.Extensions;

public static class AuthorizationPolicyExtensions
{
    private const string PermissionClaimType = "permission";
    private const string PolicyFilePath = "Modules/Auth/Seeding/authorization-policies.json";

    // Cache the JsonSerializerOptions instance
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static IServiceCollection AddCustomAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            var policies = LoadPoliciesFromFile();

            foreach (var policy in policies)
                if (policy.IsAssertion)
                    options.AddPolicy(policy.Name, p =>
                        p.RequireAssertion(context =>
                            policy.AssertionRoleName != null
                            && (context.User.IsInRole(policy.AssertionRoleName) ||
                                context.User.HasClaim(claim =>
                                    claim.Type == PermissionClaimType &&
                                    claim.Value == policy.AssertionClaimValue))
                        ));
                else if (policy.RequiredRoles?.Length > 0)
                    options.AddPolicy(policy.Name, p =>
                        p.RequireRole(policy.RequiredRoles));
                else if (policy.RequiredClaims?.Length > 0)
                    options.AddPolicy(policy.Name, p =>
                        p.RequireClaim(PermissionClaimType, policy.RequiredClaims));
        });

        return services;
    }

    private static List<AuthorizationPolicyConfiguration> LoadPoliciesFromFile()
    {
        try
        {
            var json = File.ReadAllText(PolicyFilePath);
            var policies = JsonSerializer.Deserialize<List<AuthorizationPolicyConfiguration>>(json, JsonOptions);
            return policies ?? [];
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($@"Error loading policies: {ex.Message}");
            return [];
        }
    }
}