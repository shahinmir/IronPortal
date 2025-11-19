using System;
using System.Linq;

namespace IronExchange.Authorization.AspNetCore;

public static class RequirePermissionExtensions
{
    /// <summary>
    /// Extracts the list of permissions encoded in the Policy field of the [RequirePermission] attribute.
    /// Example: "perm:catalog.view,ordering.manage" → [ "catalog.view", "ordering.manage" ]
    /// </summary>
    public static string[] GetPermissions(this RequirePermissionAttribute attr)
    {
        if (attr is null)
            return Array.Empty<string>();

        if (string.IsNullOrWhiteSpace(attr.Policy))
            return Array.Empty<string>();

        const string prefix = "perm:";
        if (!attr.Policy.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return Array.Empty<string>();

        return attr.Policy[prefix.Length..]
            .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    /// <summary>
    /// Converts permissions back into the dynamic policy name (e.g., "perm:catalog.view,ordering.manage")
    /// to maintain consistency when building policies for gRPC / Web API checks.
    /// </summary>
    public static string ToPolicyString(this string[] permissions)
    {
        if (permissions is null || permissions.Length == 0)
            return string.Empty;

        return $"perm:{string.Join(",", permissions)}";
    }
}
