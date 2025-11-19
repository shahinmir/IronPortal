namespace IronExchange.Identity.API.Models.Permissions;

/// <summary>
/// نشان دهنده یک دسترسی منحصر به فرد در سیستم (مثل: basket.read, catalog.write)
/// </summary>


public class Permission
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; }

    [Required]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ServiceName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ResourceType { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Action { get; set; } = string.Empty;

    public bool IsSystemPermission { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public virtual ICollection<PermissionGroupPermission> PermissionGroupPermissions { get; set; }
        = new List<PermissionGroupPermission>();
}





/// <summary>
/// گروه دسترسی - مثل Role ولی قدرتمندتر
/// </summary>



public class PermissionGroup
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; }

    public bool IsSystemGroup { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public virtual ICollection<PermissionGroupPermission> PermissionGroupPermissions { get; set; }
        = new List<PermissionGroupPermission>();

    public virtual ICollection<UserPermissionGroup> UserPermissionGroups { get; set; }
        = new List<UserPermissionGroup>();
}


/// <summary>
/// جدول واسط: PermissionGroup <-> Permission (Many-to-Many)
/// </summary>



public class PermissionGroupPermission
{
    public int PermissionGroupId { get; set; }
    public int PermissionId { get; set; }

    // Navigation Properties
    public virtual PermissionGroup PermissionGroup { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}


/// <summary>
/// جدول واسط: User <-> PermissionGroup (Many-to-Many)
/// </summary>




public class UserPermissionGroup
{
    [Required]
    [StringLength(450)]
    public string UserId { get; set; } = string.Empty;

    public int PermissionGroupId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    [StringLength(450)]
    public string AssignedBy { get; set; }

    public DateTime? ExpiresAt { get; set; }

    // Navigation Properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual PermissionGroup PermissionGroup { get; set; } = null!;
}




