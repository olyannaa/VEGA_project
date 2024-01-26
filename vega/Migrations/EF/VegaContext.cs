
using Microsoft.EntityFrameworkCore;

namespace vega;

public partial class VegaContext : DbContext
{
    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles {get; set; }
    
    public VegaContext()
    {
    }

    public VegaContext(DbContextOptions<VegaContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo(Console.WriteLine);
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Role1)
                .HasMaxLength(30)
                .HasColumnName("role");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("vega_users");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.Login)
                .HasMaxLength(20)
                .HasColumnName("login");
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .HasColumnName("password");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("role_user");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id");
            entity.Property(e => e.RoleId)
                .HasColumnName("role_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
