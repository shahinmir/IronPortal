namespace IronExchange.Identity.API.Models.Permissions;

public class PermissionAuditLog
{
    public long Id { get; set; }

    [Required]
    [StringLength(450)]
    public string UserId { get; set; } = string.Empty;

    [StringLength(450)]
    public string PerformedBy { get; set; }

    [Required]
    [StringLength(100)]
    public string Action { get; set; } = string.Empty;

    [StringLength(100)]
    public string EntityType { get; set; } = string.Empty;

    [StringLength(100)]
    public string EntityId { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; }

    public string Changes { get; set; }

    [StringLength(50)]
    public string IpAddress { get; set; }

    [StringLength(500)]
    public string UserAgent { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual ApplicationUser User { get; set; }
}
