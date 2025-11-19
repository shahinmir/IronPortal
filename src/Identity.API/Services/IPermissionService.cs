using IronExchange.Identity.API.Models.Permissions;

namespace IronExchange.Identity.API.Services;

public interface IPermissionService
{
    Task<List<string>> GetUserPermissionsAsync(string userId);
    Task<bool> UserHasPermissionAsync(string userId, string permissionName);
    Task<bool> UserHasAnyPermissionAsync(string userId, params string[] permissionNames);
    Task<bool> UserHasAllPermissionsAsync(string userId, params string[] permissionNames);
    Task AssignGroupToUserAsync(string userId, int groupId, string assignedBy, DateTime? expiresAt = null);
    Task RemoveGroupFromUserAsync(string userId, int groupId, string removedBy);
    Task<List<PermissionGroup>> GetUserGroupsAsync(string userId);
    Task LogPermissionChangeAsync(string userId, string action, string entityType, string entityId, string changes, string performedBy, string ipAddress, string userAgent);
}
