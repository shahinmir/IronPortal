using IronExchange.Identity.API.Models.Permissions;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Permission Management
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<PermissionGroup> PermissionGroups => Set<PermissionGroup>();
    public DbSet<PermissionGroupPermission> PermissionGroupPermissions => Set<PermissionGroupPermission>();
    public DbSet<UserPermissionGroup> UserPermissionGroups => Set<UserPermissionGroup>();
    public DbSet<PermissionAuditLog> PermissionAuditLogs => Set<PermissionAuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CardNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.SecurityNumber).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Expiration).IsRequired().HasMaxLength(5);
            entity.Property(e => e.CardHolderName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Street).IsRequired().HasMaxLength(500);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.State).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ZipCode).IsRequired().HasMaxLength(13);
            entity.Property(e => e.LastLoginIp).HasMaxLength(45);
        });

        // Permission
        builder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => new { e.ServiceName, e.ResourceType, e.Action });
        });

        // PermissionGroup
        builder.Entity<PermissionGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // PermissionGroupPermission
        builder.Entity<PermissionGroupPermission>(entity =>
        {
            entity.HasKey(e => new { e.PermissionGroupId, e.PermissionId });

            entity.HasOne(e => e.PermissionGroup)
                .WithMany(g => g.PermissionGroupPermissions)
                .HasForeignKey(e => e.PermissionGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Permission)
                .WithMany(p => p.PermissionGroupPermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserPermissionGroup
        builder.Entity<UserPermissionGroup>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.PermissionGroupId });

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserPermissionGroups)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PermissionGroup)
                .WithMany(g => g.UserPermissionGroups)
                .HasForeignKey(e => e.PermissionGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PermissionAuditLog
        builder.Entity<PermissionAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.PerformedBy);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

}
