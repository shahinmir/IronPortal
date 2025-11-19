using IronExchange.ServiceDefaults;

namespace IronExchange.Identity.API.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<RequirePermissionAttribute>
{

    private readonly ILogger<PermissionAuthorizationHandler> _logger;
    private readonly IServiceProvider _serviceProvider;

    public PermissionAuthorizationHandler(

        IServiceProvider serviceProvider,
        ILogger<PermissionAuthorizationHandler> logger)
    {

        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RequirePermissionAttribute requirement)
    {
        using var scope = _serviceProvider.CreateScope();
        var userId = context.User.GetUserId();

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID not found in claims");
            context.Fail();
            return;
        }
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
        bool hasPermission = requirement.MatchMode == PermissionMatchMode.All
            ? await permissionService.UserHasAllPermissionsAsync(userId, requirement.Permissions)
            : await permissionService.UserHasAnyPermissionAsync(userId, requirement.Permissions);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("User {UserId} does not have required permissions: {Permissions}",
                userId, string.Join(", ", requirement.Permissions));
            context.Fail();
        }
    }
}
