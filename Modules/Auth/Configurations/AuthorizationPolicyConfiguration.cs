namespace api.Modules.Auth.Configurations;

public class AuthorizationPolicyConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string[]? RequiredClaims { get; set; }
    public string[]? RequiredRoles { get; set; }
    public bool IsAssertion { get; set; }
    public string? AssertionRoleName { get; set; }
    public string? AssertionClaimValue { get; set; }
}