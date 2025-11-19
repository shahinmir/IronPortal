namespace IronExchange.Identity.API.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : Attribute, IAuthorizationRequirement
{
    public string[] Permissions { get; }
    public PermissionMatchMode MatchMode { get; }

    public RequirePermissionAttribute(string permission)
    {
        Permissions = new[] { permission };
        MatchMode = PermissionMatchMode.All;
    }

    public RequirePermissionAttribute(params string[] permissions)
    {
        Permissions = permissions;
        MatchMode = PermissionMatchMode.All;
    }

    public RequirePermissionAttribute(PermissionMatchMode matchMode, params string[] permissions)
    {
        Permissions = permissions;
        MatchMode = matchMode;
    }
}

public enum PermissionMatchMode
{
    All,  // کاربر باید همه permissions را داشته باشد
    Any   // کاربر حداقل یکی از permissions را داشته باشد
}
