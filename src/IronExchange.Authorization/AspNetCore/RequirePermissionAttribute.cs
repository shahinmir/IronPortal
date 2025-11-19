using Microsoft.AspNetCore.Authorization;

namespace IronExchange.Authorization.AspNetCore;

/// <summary>
/// Attribute مخصوص کنترل دسترسی با Type-safe Permissions.
/// از PermissionConstants استفاده می‌کند تا خطاهای تایپی در زمان کامپایل شناسایی شوند.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    public string[] Permissions { get; }
    public PermissionMatchMode MatchMode { get; }

    public RequirePermissionAttribute(PermissionMatchMode matchMode, params string[] permissions)
    {
        MatchMode = matchMode;
        Permissions = permissions ?? [];
        Policy = BuildPolicyName();
    }

    public RequirePermissionAttribute(params string[] permissions)
    {
        Permissions = permissions ?? [];
        MatchMode = PermissionMatchMode.All;
        Policy = BuildPolicyName();
    }

    private string BuildPolicyName()
    {
        // ساخت اسم unified پالیسی، مثل: "perm:any:catalog.view,ordering.manage"
        var prefix = MatchMode == PermissionMatchMode.Any ? "perm:any:" : "perm:all:";
        return $"{prefix}{string.Join(",", Permissions)}";
    }
}

/// <summary>
/// حالت بررسی مجوزها (تمام یا هرکدام)
/// </summary>
public enum PermissionMatchMode
{
    All,
    Any
}
