using IronExchange.Identity.API.Models.Permissions;
using Microsoft.Extensions.Caching.Memory;

namespace IronExchange.Identity.API.Services;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PermissionService> _logger;
    private const string CacheKeyPrefix = "user_permissions_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    public PermissionService(
        ApplicationDbContext context,
        IMemoryCache cache,
        ILogger<PermissionService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<string>> GetUserPermissionsAsync(string userId)
    {
        var cacheKey = $"{CacheKeyPrefix}{userId}";

        if (_cache.TryGetValue(cacheKey, out List<string> cachedPermissions))
        {
            return cachedPermissions;
        }

        var permissions = await _context.UserPermissionGroups
            .Where(upg => upg.UserId == userId
                && upg.PermissionGroup.IsActive
                && (upg.ExpiresAt == null || upg.ExpiresAt > DateTime.UtcNow))
            .SelectMany(upg => upg.PermissionGroup.PermissionGroupPermissions)
            .Where(pgp => pgp.Permission.IsActive)
            .Select(pgp => pgp.Permission.Name)
            .Distinct()
            .ToListAsync();

        _cache.Set(cacheKey, permissions, CacheDuration);

        return permissions;
    }

    public async Task<bool> UserHasPermissionAsync(string userId, string permissionName)
    {
        var permissions = await GetUserPermissionsAsync(userId);
        return permissions.Contains(permissionName);
    }

    public async Task<bool> UserHasAnyPermissionAsync(string userId, params string[] permissionNames)
    {
        var permissions = await GetUserPermissionsAsync(userId);
        return permissionNames.Any(p => permissions.Contains(p));
    }

    public async Task<bool> UserHasAllPermissionsAsync(string userId, params string[] permissionNames)
    {
        var permissions = await GetUserPermissionsAsync(userId);
        return permissionNames.All(p => permissions.Contains(p));
    }

    public async Task AssignGroupToUserAsync(string userId, int groupId, string assignedBy, DateTime? expiresAt = null)
    {
        var existingAssignment = await _context.UserPermissionGroups
            .FirstOrDefaultAsync(upg => upg.UserId == userId && upg.PermissionGroupId == groupId);

        if (existingAssignment == null)
        {
            var assignment = new UserPermissionGroup
            {
                UserId = userId,
                PermissionGroupId = groupId,
                AssignedBy = assignedBy,
                ExpiresAt = expiresAt
            };

            _context.UserPermissionGroups.Add(assignment);
            await _context.SaveChangesAsync();

            // Clear cache
            _cache.Remove($"{CacheKeyPrefix}{userId}");

            // Log
            await LogPermissionChangeAsync(userId, "GroupAssigned", "PermissionGroup", groupId.ToString(),
                $"Assigned by {assignedBy}, Expires: {expiresAt?.ToString() ?? "Never"}", assignedBy, null, null);

            _logger.LogInformation("Permission group {GroupId} assigned to user {UserId} by {AssignedBy}",
                groupId, userId, assignedBy);
        }
    }

    public async Task RemoveGroupFromUserAsync(string userId, int groupId, string removedBy)
    {
        var assignment = await _context.UserPermissionGroups
            .FirstOrDefaultAsync(upg => upg.UserId == userId && upg.PermissionGroupId == groupId);

        if (assignment != null)
        {
            _context.UserPermissionGroups.Remove(assignment);
            await _context.SaveChangesAsync();

            // Clear cache
            _cache.Remove($"{CacheKeyPrefix}{userId}");

            // Log
            await LogPermissionChangeAsync(userId, "GroupRemoved", "PermissionGroup", groupId.ToString(),
                $"Removed by {removedBy}", removedBy, null, null);

            _logger.LogInformation("Permission group {GroupId} removed from user {UserId} by {RemovedBy}",
                groupId, userId, removedBy);
        }
    }

    public async Task<List<PermissionGroup>> GetUserGroupsAsync(string userId)
    {
        return await _context.UserPermissionGroups
            .Where(upg => upg.UserId == userId
                && upg.PermissionGroup.IsActive
                && (upg.ExpiresAt == null || upg.ExpiresAt > DateTime.UtcNow))
            .Select(upg => upg.PermissionGroup)
            .ToListAsync();
    }

    public async Task LogPermissionChangeAsync(string userId, string action, string entityType,
        string entityId, string changes, string performedBy, string ipAddress, string userAgent)
    {
        var auditLog = new PermissionAuditLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Changes = changes,
            PerformedBy = performedBy,
            IpAddress = ipAddress ?? "Unknown",
            UserAgent = userAgent ?? "Unknown"
        };

        _context.PermissionAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }
}
