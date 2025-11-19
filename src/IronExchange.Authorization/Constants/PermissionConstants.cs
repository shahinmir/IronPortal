namespace IronExchange.Authorization.Constants;

public static class PermissionConstants
{
    // Catalog
    public const string CatalogView = "catalog.view";
    public const string CatalogCreate = "catalog.create";
    public const string CatalogEdit = "catalog.edit";
    public const string CatalogDelete = "catalog.delete";
    public const string CatalogApprove = "catalog.approve";

    // Ordering
    public const string OrderingView = "ordering.view";
    public const string OrderingManage = "ordering.manage";

    // Payment
    public const string PaymentProcess = "payment.process";

    // Admin
    public const string AdminFullAccess = "admin.full_access";
}
