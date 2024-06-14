
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

    public virtual DbSet<StorageComponent> Storage { get; set; }

    public virtual DbSet<Component> Components { get; set; }

    public virtual DbSet<Designation> Designations { get; set; }

    public virtual DbSet<OrderComponent> OrderComponents { get; set; }

    public virtual DbSet<Scheme> Schemes { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<TechProcess> TechProccesses { get; set; }


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

            entity.Property(e => e.Id).HasColumnName("id");

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

        modelBuilder.Entity<StorageComponent>(entity =>
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

        modelBuilder.Entity<Component>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("components_pkey");

            entity.ToTable("components");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.DesignationId).HasColumnName("designation_id");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.IsDeveloped).HasColumnName("is_developed");

            entity.HasOne(d => d.Designation).WithOne(p => p.Component)
                .HasForeignKey<Component>(d => d.DesignationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("components_designation_id_fkey");

            entity.HasOne(d => d.Parent).WithMany(p => p.Children)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("components_components_fk");
        });

        modelBuilder.Entity<Designation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("designations_pkey");

            entity.ToTable("designations");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FullName).HasColumnName("full_name");
            entity.Property(e => e.ProcessId).HasColumnName("process_id");
            entity.Property(e => e.SchemesId).HasColumnName("schemes_id");

            entity.HasOne(d => d.Proccess).WithMany(p => p.Designations)
                .HasForeignKey(d => d.ProcessId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("designations_process_id_fkey");

            entity.HasOne(d => d.Scheme).WithOne(p => p.Designation)
                .HasForeignKey<Designation>(d => d.SchemesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("designations_schemes_id_fkey");
        });

        modelBuilder.Entity<OrderComponent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orders_components_pkey");

            entity.ToTable("orders_components");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ComponentId).HasColumnName("component_id");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderComponents)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_components_order_id_fkey");

            entity.HasOne(d => d.Component).WithOne(p => p.OrderComponent)
                .HasForeignKey<OrderComponent>(d => d.ComponentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_components_component_id_fkey");
        });

        modelBuilder.Entity<Scheme>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("schemes_pkey");

            entity.ToTable("schemes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Path).HasColumnName("scheme_path");
        });
        
        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tasks_pkey");

            entity.ToTable("tasks");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ComponentId).HasColumnName("component_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AreaId).HasColumnName("area_id");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.IsAvaliable).HasColumnName("is_avaliable");

            entity.HasOne(d => d.Component).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.ComponentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tasks_component_id_fkey");

            entity.HasOne(d => d.Area).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.AreaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tasks_area_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tasks_status_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tasks_user_id_fkey");

            entity.HasOne(d => d.Parent).WithOne(p => p.Child)
                .HasForeignKey<Task>(d => d.ParentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tasks_tasks_fk");
        });
        
        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("statuses_pkey");

            entity.ToTable("statuses");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<TechProcess>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tech_process_pkey");

            entity.ToTable("tech_process");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Process).HasColumnName("process");
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
