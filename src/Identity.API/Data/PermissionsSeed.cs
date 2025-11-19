using IronExchange.Identity.API.Models.Permissions;

namespace IronExchange.Identity.API.Data;

public class PermissionsSeed
{
    private readonly ILogger<PermissionsSeed> _logger;

    public PermissionsSeed(ILogger<PermissionsSeed> logger)
    {
        _logger = logger;
    }

    public async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Permissions.AnyAsync())
        {
            _logger.LogInformation("Permissions already seeded");
            return;
        }

        var permissions = new List<Permission>
        {
            // Admin Permissions
            new Permission
            {
                Name = "admin.users.view",
                DisplayName = "View Users",
                Description = "Can view user list and details",
                Category = "Admin",
                ServiceName = "Identity",
                ResourceType = "User",
                Action = "Read",
                IsSystemPermission = true
            },
            new Permission
            {
                Name = "admin.users.manage",
                DisplayName = "Manage Users",
                Description = "Can create, edit, and delete users",
                Category = "Admin",
                ServiceName = "Identity",
                ResourceType = "User",
                Action = "Write",
                IsSystemPermission = true
            },
            new Permission
            {
                Name = "admin.roles.manage",
                DisplayName = "Manage Roles",
                Description = "Can manage user roles and permissions",
                Category = "Admin",
                ServiceName = "Identity",
                ResourceType = "Role",
                Action = "Write",
                IsSystemPermission = true
            },

            // Catalog Permissions
            new Permission
            {
                Name = "catalog.products.view",
                DisplayName = "View Products",
                Description = "Can view product catalog",
                Category = "Catalog",
                ServiceName = "Catalog",
                ResourceType = "Product",
                Action = "Read",
                IsSystemPermission = false
            },
            new Permission
            {
                Name = "catalog.products.write",
                DisplayName = "Manage Products",
                Description = "Can create and edit products",
                Category = "Catalog",
                ServiceName = "Catalog",
                ResourceType = "Product",
                Action = "Write",
                IsSystemPermission = false
            },
            new Permission
            {
                Name = "catalog.products.delete",
                DisplayName = "Delete Products",
                Description = "Can delete products from catalog",
                Category = "Catalog",
                ServiceName = "Catalog",
                ResourceType = "Product",
                Action = "Delete",
                IsSystemPermission = false
            },

            // Basket Permissions
            new Permission
            {
                Name = "basket.view",
                DisplayName = "View Basket",
                Description = "Can view shopping basket",
                Category = "Basket",
                ServiceName = "Basket",
                ResourceType = "Basket",
                Action = "Read",
                IsSystemPermission = false
            },
            new Permission
            {
                Name = "basket.manage",
                DisplayName = "Manage Basket",
                Description = "Can add/remove items from basket",
                Category = "Basket",
                ServiceName = "Basket",
                ResourceType = "Basket",
                Action = "Write",
                IsSystemPermission = false
            },

            // Ordering Permissions
            new Permission
            {
                Name = "ordering.view",
                DisplayName = "View Orders",
                Description = "Can view order history",
                Category = "Ordering",
                ServiceName = "Ordering",
                ResourceType = "Order",
                Action = "Read",
                IsSystemPermission = false
            },
            new Permission
            {
                Name = "ordering.create",
                DisplayName = "Create Orders",
                Description = "Can place new orders",
                Category = "Ordering",
                ServiceName = "Ordering",
                ResourceType = "Order",
                Action = "Write",
                IsSystemPermission = false
            },
            new Permission
            {
                Name = "ordering.manage",
                DisplayName = "Manage Orders",
                Description = "Can manage and update order status",
                Category = "Ordering",
                ServiceName = "Ordering",
                ResourceType = "Order",
                Action = "Write",
                IsSystemPermission = false
            },
            new Permission
            {
                Name = "ordering.cancel",
                DisplayName = "Cancel Orders",
                Description = "Can cancel orders",
                Category = "Ordering",
                ServiceName = "Ordering",
                ResourceType = "Order",
                Action = "Delete",
                IsSystemPermission = false
            },

            // Payment Permissions
            new Permission
            {
                Name = "payment.process",
                DisplayName = "Process Payments",
                Description = "Can process payment transactions",
                Category = "Payment",
                ServiceName = "Payment",
                ResourceType = "Payment",
                Action = "Write",
                IsSystemPermission = false
            },

            // Reports Permissions
            new Permission
            {
                Name = "reports.sales.view",
                DisplayName = "View Sales Reports",
                Description = "Can view sales analytics and reports",
                Category = "Reports",
                ServiceName = "Reporting",
                ResourceType = "Report",
                Action = "Read",
                IsSystemPermission = false
            },
            new Permission
            {
                Name = "reports.financial.view",
                DisplayName = "View Financial Reports",
                Description = "Can view financial reports",
                Category = "Reports",
                ServiceName = "Reporting",
                ResourceType = "Report",
                Action = "Read",
                IsSystemPermission = false
            }
        };

        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();

        _logger.LogInformation($"✅ Seeded {permissions.Count} permissions");

        // Seed Permission Groups
        await SeedPermissionGroupsAsync(context);
    }

    private async Task SeedPermissionGroupsAsync(ApplicationDbContext context)
    {
        if (await context.PermissionGroups.AnyAsync())
        {
            _logger.LogInformation("Permission groups already seeded");
            return;
        }

        var groups = new List<PermissionGroup>
        {
            new PermissionGroup
            {
                Name = "Customer",
                DisplayName = "Customer",
                Description = "Standard customer permissions",
                IsSystemGroup = true
            },
            new PermissionGroup
            {
                Name = "Seller",
                DisplayName = "Seller",
                Description = "Seller/vendor permissions",
                IsSystemGroup = true
            },
            new PermissionGroup
            {
                Name = "OrderManager",
                DisplayName = "Order Manager",
                Description = "Order management permissions",
                IsSystemGroup = true
            },
            new PermissionGroup
            {
                Name = "ReportViewer",
                DisplayName = "Report Viewer",
                Description = "Can view reports and analytics",
                IsSystemGroup = true
            },
            new PermissionGroup
            {
                Name = "Administrator",
                DisplayName = "Administrator",
                Description = "Full system access",
                IsSystemGroup = true
            }
        };

        await context.PermissionGroups.AddRangeAsync(groups);
        await context.SaveChangesAsync();

        _logger.LogInformation($"✅ Seeded {groups.Count} permission groups");

        // Assign permissions to groups
        await AssignPermissionsToGroupsAsync(context);
    }

    private async Task AssignPermissionsToGroupsAsync(ApplicationDbContext context)
    {
        var allPermissions = await context.Permissions.ToListAsync();
        var groups = await context.PermissionGroups.ToListAsync();

        var assignments = new List<PermissionGroupPermission>();

        // Customer Group
        var customerGroup = groups.First(g => g.Name == "Customer");
        var customerPermissions = allPermissions.Where(p =>
            p.Name.StartsWith("basket.") ||
            p.Name.StartsWith("ordering.view") ||
            p.Name.StartsWith("ordering.create") ||
            p.Name == "catalog.products.view"
        ).ToList();

        foreach (var permission in customerPermissions)
        {
            assignments.Add(new PermissionGroupPermission
            {
                PermissionGroupId = customerGroup.Id,
                PermissionId = permission.Id
            });
        }

        // Seller Group
        var sellerGroup = groups.First(g => g.Name == "Seller");
        var sellerPermissions = allPermissions.Where(p =>
            p.Name.StartsWith("catalog.") ||
            p.Name == "ordering.view" ||
            p.Name == "reports.sales.view"
        ).ToList();

        foreach (var permission in sellerPermissions)
        {
            assignments.Add(new PermissionGroupPermission
            {
                PermissionGroupId = sellerGroup.Id,
                PermissionId = permission.Id
            });
        }

        // Administrator Group (all permissions)
        var adminGroup = groups.First(g => g.Name == "Administrator");
        foreach (var permission in allPermissions)
        {
            assignments.Add(new PermissionGroupPermission
            {
                PermissionGroupId = adminGroup.Id,
                PermissionId = permission.Id
            });
        }

        await context.PermissionGroupPermissions.AddRangeAsync(assignments);
        await context.SaveChangesAsync();

        _logger.LogInformation($"✅ Assigned {assignments.Count} permissions to groups");
    }
}
