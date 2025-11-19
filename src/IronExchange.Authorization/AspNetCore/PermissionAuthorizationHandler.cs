using Microsoft.AspNetCore.Authorization;

namespace IronExchange.Authorization.AspNetCore;


public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var hasClaim = requirement.MatchMode switch
        {
            PermissionMatchMode.All => requirement.Permissions.All(p => context.User.HasClaim("permission", p)),
            PermissionMatchMode.Any => requirement.Permissions.Any(p => context.User.HasClaim("permission", p)),
            _ => false
        };

        if (hasClaim)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}


public class PermissionRequirement : IAuthorizationRequirement
{
    public string[] Permissions { get; }
    public PermissionMatchMode MatchMode { get; }

    public PermissionRequirement(PermissionMatchMode matchMode, params string[] permissions)
    {
        Permissions = permissions;
        MatchMode = matchMode;
    }
}
