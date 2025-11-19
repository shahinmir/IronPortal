namespace IronExchange.Authorization;

/// <summary>
/// تعریف ساختار Type-safe برای تمام مجوزها، مناسب برای Autocomplete در زمان کامپایل.
/// </summary>
public static class Permissions
{
    public static class Catalog
    {
        public const string View = "catalog.view";
        public const string Create = "catalog.create";
        public const string Edit = "catalog.edit";
        public const string Delete = "catalog.delete";
        public const string Approve = "catalog.approve";
    }

    public static class Ordering
    {
        public const string View = "ordering.view";
        public const string Create = "ordering.create";
        public const string Manage = "ordering.manage";
        public const string Cancel = "ordering.cancel";
    }

    public static class Payment
    {
        public const string View = "payment.view";
        public const string Process = "payment.process";
        public const string Refund = "payment.refund";
    }

    public static class Users
    {
        public const string View = "users.view";
        public const string Edit = "users.edit";
        public const string Delete = "users.delete";
        public const string AssignPermissions = "users.assign_permissions";
    }

    public static class Admin
    {
        public const string FullAccess = "admin.full_access";
        public const string ViewLogs = "admin.view_logs";
    }
}
