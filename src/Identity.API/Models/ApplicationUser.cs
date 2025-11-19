using IronExchange.Identity.API.Models.Permissions;

namespace IronExchange.Identity.API.Models;

public class ApplicationUser : IdentityUser
{
    // Payment Card Information
    [Required]
    [StringLength(20)]
    public string CardNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string SecurityNumber { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"(0[1-9]|1[0-2])\/[0-9]{2}", ErrorMessage = "Expiration should match a valid MM/YY value")]
    [StringLength(5)]
    public string Expiration { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string CardHolderName { get; set; } = string.Empty;

    public int CardType { get; set; }

    // Address Information
    [Required]
    [StringLength(500)]
    public string Street { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string State { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;

    [Required]
    [StringLength(13)]
    public string ZipCode { get; set; } = string.Empty;

    // Personal Information
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    public DateTime? BirthDate { get; set; }

    // Account Status
    public bool IsActive { get; set; } = true;

    // Security & Audit
    public DateTime? LastLoginAt { get; set; }

    [StringLength(45)]
    public string LastLoginIp { get; set; }

    public int FailedLoginAttempts { get; set; } = 0;

    public DateTime? LockedUntil { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation Properties
    public virtual ICollection<UserPermissionGroup> UserPermissionGroups { get; set; }
        = new List<UserPermissionGroup>();
}
