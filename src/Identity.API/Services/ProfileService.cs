using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;

using IronExchange.Identity.API.Models;
using IronExchange.Identity.API.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IronExchange.Identity.API.Services;

public class ProfileService : IProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(
        UserManager<ApplicationUser> userManager,
        IPermissionService permissionService,
        ILogger<ProfileService> logger)
    {
        _userManager = userManager;
        _permissionService = permissionService;
        _logger = logger;
    }

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var userId = context.Subject.GetSubjectId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            _logger.LogWarning($"User {userId} not found");
            return;
        }

        var claims = new List<Claim>
        {
            new Claim(JwtClaimTypes.Subject, user.Id),
            new Claim(JwtClaimTypes.PreferredUserName, user.UserName),
            new Claim(JwtClaimTypes.Name, $"{user.Name} {user.LastName}"),
            new Claim(JwtClaimTypes.Email, user.Email),
            new Claim(JwtClaimTypes.EmailVerified, user.EmailConfirmed.ToString().ToLower())
        };

        // ✅ اضافه کردن Roles
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(JwtClaimTypes.Role, role));
        }

        // ✅ اضافه کردن Permissions
        var permissions = await _permissionService.GetUserPermissionsAsync(userId);
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        // ✅ فقط claimهایی که requested شده‌اند را برگردانید
        context.IssuedClaims = claims
            .Where(c => context.RequestedClaimTypes.Contains(c.Type))
            .ToList();

        _logger.LogInformation($"Profile data issued for user {userId} with {context.IssuedClaims.Count} claims");
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        var userId = context.Subject.GetSubjectId();
        var user = await _userManager.FindByIdAsync(userId);

        context.IsActive = user != null && user.EmailConfirmed;
    }
}
