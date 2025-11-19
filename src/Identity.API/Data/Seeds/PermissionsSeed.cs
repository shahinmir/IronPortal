using IronExchange.Identity.API.Models.Permissions;

namespace IronExchange.Identity.API.Data.Seeds;

public class PermissionsSeed
{
    private readonly ILogger<PermissionsSeed> _logger;

    public PermissionsSeed(ILogger<PermissionsSeed> logger)
    {
        _logger = logger;
    }

    public async Task SeedAsync(ApplicationDbContext context)
    {
        if (!context.Permissions.Any())
        {
            var permissions = new List<Permission>
            {
                // Catalog Permissions
                new() { Name = "catalog.view", DisplayName = "مشاهده کاتالوگ", Category = "Catalog" },
                new() { Name = "catalog.create", DisplayName = "ایجاد محصول", Category = "Catalog" },
                new() { Name = "catalog.edit", DisplayName = "ویرایش محصول", Category = "Catalog" },
                new() { Name = "catalog.delete", DisplayName = "حذف محصول", Category = "Catalog" },
                new() { Name = "catalog.approve", DisplayName = "تایید محصول", Category = "Catalog" },
                
                // Ordering Permissions
                new() { Name = "ordering.view", DisplayName = "مشاهده سفارشات", Category = "Ordering" },
                new() { Name = "ordering.create", DisplayName = "ایجاد سفارش", Category = "Ordering" },
                new() { Name = "ordering.manage", DisplayName = "مدیریت سفارشات", Category = "Ordering" },
                new() { Name = "ordering.cancel", DisplayName = "لغو سفارش", Category = "Ordering" },
                
                // Payment Permissions
                new() { Name = "payment.view", DisplayName = "مشاهده پرداخت‌ها", Category = "Payment" },
                new() { Name = "payment.process", DisplayName = "پردازش پرداخت", Category = "Payment" },
                new() { Name = "payment.refund", DisplayName = "استرداد وجه", Category = "Payment" },
                
                // User Management
                new() { Name = "users.view", DisplayName = "مشاهده کاربران", Category = "UserManagement" },
                new() { Name = "users.edit", DisplayName = "ویرایش کاربران", Category = "UserManagement" },
                new() { Name = "users.delete", DisplayName = "حذف کاربران", Category = "UserManagement" },
                new() { Name = "users.assign_permissions", DisplayName = "تخصیص دسترسی", Category = "UserManagement" },
                
                // Admin
                new() { Name = "admin.full_access", DisplayName = "دسترسی کامل مدیریتی", Category = "Admin" },
                new() { Name = "admin.view_logs", DisplayName = "مشاهده گزارشات", Category = "Admin" },
            };

            context.Permissions.AddRange(permissions);
            await context.SaveChangesAsync();
            _logger.LogInformation("Permissions seeded successfully");
        }

        if (!context.PermissionGroups.Any())
        {
            var groups = new List<PermissionGroup>
            {
                new()
                {
                    Name = "Customer",
                    DisplayName = "مشتری عادی",
                    Description = "دسترسی‌های پایه برای خرید"
                },
                new()
                {
                    Name = "Seller",
                    DisplayName = "فروشنده",
                    Description = "دسترسی‌های مدیریت محصولات و سفارشات"
                },
                new()
                {
                    Name = "ContentManager",
                    DisplayName = "مدیر محتوا",
                    Description = "مدیریت کاتالوگ و محصولات"
                },
                new()
                {
                    Name = "FinancialManager",
                    DisplayName = "مدیر مالی",
                    Description = "مدیریت پرداخت‌ها و تراکنش‌ها"
                },
                new()
                {
                    Name = "Administrator",
                    DisplayName = "مدیر کل",
                    Description = "دسترسی کامل به سیستم"
                }
            };

            context.PermissionGroups.AddRange(groups);
            await context.SaveChangesAsync();
            _logger.LogInformation("Permission groups seeded successfully");

            // Assign Permissions to Groups
            var customerGroup = groups[0];
            var sellerGroup = groups[1];
            var contentManagerGroup = groups[2];
            var financialManagerGroup = groups[3];
            var adminGroup = groups[4];

            var allPermissions = await context.Permissions.ToListAsync();

            // Customer permissions
            var customerPermissions = allPermissions
                .Where(p => p.Name.StartsWith("catalog.view") || p.Name.StartsWith("ordering.view") || p.Name.StartsWith("ordering.create"))
                .Select(p => new PermissionGroupPermission { PermissionGroupId = customerGroup.Id, PermissionId = p.Id })
                .ToList();

            // Seller permissions
            var sellerPermissions = allPermissions
                .Where(p => p.Category == "Catalog" || p.Category == "Ordering")
                .Select(p => new PermissionGroupPermission { PermissionGroupId = sellerGroup.Id, PermissionId = p.Id })
                .ToList();

            // Content Manager permissions
            var contentManagerPermissions = allPermissions
                .Where(p => p.Category == "Catalog")
                .Select(p => new PermissionGroupPermission { PermissionGroupId = contentManagerGroup.Id, PermissionId = p.Id })
                .ToList();

            // Financial Manager permissions
            var financialManagerPermissions = allPermissions
                .Where(p => p.Category == "Payment" || p.Name.StartsWith("ordering.view"))
                .Select(p => new PermissionGroupPermission { PermissionGroupId = financialManagerGroup.Id, PermissionId = p.Id })
                .ToList();

            // Admin permissions (all)
            var adminPermissions = allPermissions
                .Select(p => new PermissionGroupPermission { PermissionGroupId = adminGroup.Id, PermissionId = p.Id })
                .ToList();

            context.PermissionGroupPermissions.AddRange(customerPermissions);
            context.PermissionGroupPermissions.AddRange(sellerPermissions);
            context.PermissionGroupPermissions.AddRange(contentManagerPermissions);
            context.PermissionGroupPermissions.AddRange(financialManagerPermissions);
            context.PermissionGroupPermissions.AddRange(adminPermissions);

            await context.SaveChangesAsync();
            _logger.LogInformation("Permissions assigned to groups successfully");
        }
    }
}
