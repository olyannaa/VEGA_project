
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

    public virtual DbSet<StepRole> StepRoles { get; set;}

    public virtual DbSet<Privilege> Privileges { get; set; }

    public virtual DbSet<RolePrivilege> RolePriveleges { get; set; }

    public virtual DbSet<Component> Storage { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo(Console.WriteLine);
        optionsBuilder.EnableSensitiveDataLogging();
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
            entity.HasKey(e => e.Id).HasName("areas_users_pk");

            entity.ToTable("areas_users");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.AreaId).HasColumnName("area_id");

            entity.HasOne(d => d.Area).WithMany(p => p.AreaUsers)
                .HasForeignKey(d => d.AreaId)
                .HasConstraintName("areas_users_area_id_fkey");

            entity.HasOne(d => d.User).WithOne(p => p.AreaUser)
                .HasForeignKey<AreaUser>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("areas_users_user_id_fkey");
        });

        modelBuilder.Entity<Privilege>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("id");

            entity.ToTable("privileges");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("privilege");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .HasColumnName("description");
        });

        modelBuilder.Entity<RolePrivilege>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("id");

            entity.ToTable("roles_privileges");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.PrivilegeId).HasColumnName("privilege_id");

            entity.HasOne(d => d.Role).WithMany(p => p.RolePrivileges)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("roles_privileges_role_id_fkey");

            entity.HasOne(d => d.Privilege).WithMany(p => p.Roles)
                .HasForeignKey(d => d.PrivilegeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("roles_privileges_privilege_id_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
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
            entity.Property(e => e.StepId).HasColumnName("step_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Path).HasColumnName("path");
            entity.Property(e => e.FileName).HasColumnName("file_name");
            entity.Property(e => e.IsNeededToChange).HasColumnName("status");
            entity.Property(e => e.UploadDate).HasColumnName("upload_date");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderFiles)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_files_order_id_fkey");

             entity.HasOne(d => d.Step).WithMany(p => p.OrderFiles)
                .HasForeignKey(d => d.StepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_files_steps_fkey");
        });

        modelBuilder.Entity<Step>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("steps_pkey");

            entity.ToTable("steps");

            entity.Property(e => e.Id).HasColumnName("step_id");
            entity.Property(e => e.Name).HasColumnName("step");
        });

        modelBuilder.Entity<StepRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_steps_pk");

            entity.ToTable("roles_steps");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.StepId).HasColumnName("step_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(d => d.Role).WithOne(p => p.StepRole)
                .HasForeignKey<StepRole>(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_steps_roles_fk");

            entity.HasOne(d => d.Step).WithOne(p => p.StepRole)
                .HasForeignKey<StepRole>(d => d.StepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_steps_steps_fk");
        });

        modelBuilder.Entity<OrderStep>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orders_steps_pk");

            entity.ToTable("orders_steps");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.StepId).HasColumnName("step_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.IsCompleted).HasColumnName("is_completed");
            entity.Property(e => e.Comment).HasColumnName("comment");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderSteps)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_steps_order_id_fkey");

            entity.HasOne(d => d.Step).WithMany(p => p.OrderSteps)
                .HasForeignKey(d => d.StepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_steps_step_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.OrderSteps)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_steps_user_id_fkey");

            entity.HasOne(d => d.Parent).WithMany(p => p.Children)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_steps_orders_steps_fkey");
            
        });

        modelBuilder.Entity<Component>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("storage_pk");

            entity.ToTable("storage");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Designation).HasColumnName("designation");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.Measure).HasColumnName("measure");
            entity.Property(e => e.Material).HasColumnName("material");
            entity.Property(e => e.ObjectType).HasColumnName("object_type");

            entity.HasOne(d => d.Order).WithMany(p => p.Storage)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("storage_orders_fk");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    

    public bool TryUpdateParentalStepCompletion(OrderStep initOrderStep)
    {
        var orderId = initOrderStep.OrderId;
        var stepId = initOrderStep.StepId;
        var orderStep = OrderSteps.Where(e => e.StepId == stepId && e.OrderId == orderId);
        var parent = orderStep.Select(e => e.Parent).SingleOrDefault();
        if (orderStep.SingleOrDefault() != null && parent != null)
        {
            var children = orderStep.Select(e => e.Parent).Select(e => e.Children).SingleOrDefault();
            if (children != null && children.All(e => e.IsCompleted))
            {
                parent.IsCompleted = true;
                SaveChanges();
                return true;
            }
            return false;
        }
        return false;
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
