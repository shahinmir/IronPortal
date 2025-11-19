using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace IronExchange.Authorization.AspNetCore;

public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options) { }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith("perm:", StringComparison.OrdinalIgnoreCase))
        {
            var (mode, permissions) = ParsePolicy(policyName);

            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(mode, permissions))
                .Build();

            return await Task.FromResult(policy);
        }

        return await base.GetPolicyAsync(policyName);
    }

    private static (PermissionMatchMode Mode, string[] Permissions) ParsePolicy(string name)
    {
        var tokens = name.Split(':', 3);
        var mode = PermissionMatchMode.All;
        string permsPart;

        if (tokens.Length >= 3)
        {
            mode = tokens[1].Equals("any", StringComparison.OrdinalIgnoreCase)
                ? PermissionMatchMode.Any
                : PermissionMatchMode.All;
            permsPart = tokens[2];
        }
        else
        {
            permsPart = tokens[^1];
        }

        var permissions = permsPart.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return (mode, permissions);
    }
}
