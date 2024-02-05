using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace vega;

public partial class VegaContext : DbContext
{
    public VegaContext()
    {
    }

    public VegaContext(DbContextOptions<VegaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Area> Areas { get; set; }

    public virtual DbSet<AreaUser> AreaUsers { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RoleUser> RoleUsers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo(Console.WriteLine);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<Area>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("areas_pkey");

            entity.ToTable("areas");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AreaName).HasColumnName("area_name");
        });

        modelBuilder.Entity<AreaUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("areas_users_pk");

            entity.ToTable("areas_users");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.AreaId).HasColumnName("area_id");

            entity.HasOne(d => d.Area).WithMany(p => p.AreasUsers)
                .HasForeignKey(d => d.AreaId)
                .HasConstraintName("areas_users_area_id_fkey");

            entity.HasOne(d => d.User).WithOne(p => p.AreasUser)
                .HasForeignKey<AreaUser>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("areas_users_user_id_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Role1)
                .HasMaxLength(30)
                .HasColumnName("role");
        });

        modelBuilder.Entity<RoleUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("role_user_pk");

            entity.ToTable("role_user");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(d => d.Role).WithMany(p => p.RoleUsers)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("role_user_role_id_fkey");

            entity.HasOne(d => d.User).WithOne(p => p.RoleUser)
                .HasForeignKey<RoleUser>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("role_user_user_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("id");

            entity.ToTable("vega_users");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Login)
                .HasMaxLength(20)
                .HasColumnName("login");
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .HasColumnName("password");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
