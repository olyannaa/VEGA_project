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

    public virtual DbSet<UserToken> UserTokens { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderFile> OrderFiles { get; set; }

    public virtual DbSet<OrderStep> OrderSteps { get; set; }

    public virtual DbSet<Step> Steps { get; set; }


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
            entity.HasKey(e => e.Id).HasName("role_user_pk");

            entity.ToTable("role_user");


            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(d => d.Role).WithMany(p => p.RoleUsers)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("role_user_role_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.RoleUsers)
                .HasForeignKey(d => d.UserId)
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
            entity.Property(e => e.FullName)
                .HasColumnName("full_name");
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("token_user_pk");

            entity.ToTable("token_user");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RefreshToken)
                .HasColumnName("refresh_token");
            entity.Property(e => e.ExpireTime)
                .HasColumnName("refresh_expire_time");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orders_pkey");

            entity.ToTable("orders");

            entity.Property(e => e.Id).HasColumnName("order_id");
            entity.Property(e => e.KKS).HasColumnName("kks");
        });

        modelBuilder.Entity<OrderFile>(entity =>
        {
            entity.HasKey(e => e.FileId).HasName("orders_files_pk");

            entity.ToTable("orders_files");

            entity.Property(e => e.FileId).HasColumnName("file_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.FileName).HasColumnName("file_name");
            entity.Property(e => e.IsNeededToChange).HasColumnName("status");
            entity.Property(e => e.UploadDate).HasColumnName("upload_date");
        });

        modelBuilder.Entity<Step>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("steps_pkey");

            entity.ToTable("steps");

            entity.Property(e => e.Id).HasColumnName("step_id");
            entity.Property(e => e.Name).HasColumnName("step");
        });

        modelBuilder.Entity<OrderStep>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("orders_steps_pk");

            entity.ToTable("orders_steps");

            entity.Property(e => e.StepId).HasColumnName("step_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.IsApproved).HasColumnName("status");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderSteps)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_steps_order_id_fkey");

            entity.HasOne(d => d.Step).WithMany(p => p.OrderSteps)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_steps_step_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.OrderSteps)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_steps_user_id_fkey");
            
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
