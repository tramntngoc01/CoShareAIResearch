using CoShare.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace CoShare.Infrastructure.Data;

/// <summary>
/// EF Core DbContext for USERS module.
/// Maps to tables defined in DB-USERS.md.
/// </summary>
public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserRefTierHistory> RefTierHistory => Set<UserRefTierHistory>();
    public DbSet<UserStatusHistory> StatusHistory => Set<UserStatusHistory>();
    public DbSet<UserImportJob> ImportJobs => Set<UserImportJob>();
    public DbSet<UserImportRow> ImportRows => Set<UserImportRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureUserRefTierHistory(modelBuilder);
        ConfigureUserStatusHistory(modelBuilder);
        ConfigureUserImportJob(modelBuilder);
        ConfigureUserImportRow(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users_user");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.ExternalEmployeeCode)
                .HasColumnName("external_employee_code")
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(e => e.FullName)
                .HasColumnName("full_name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.Phone)
                .HasColumnName("phone")
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(e => e.CompanyId)
                .HasColumnName("company_id")
                .IsRequired();

            entity.Property(e => e.PickupPointId)
                .HasColumnName("pickup_point_id")
                .IsRequired();

            entity.Property(e => e.Tier)
                .HasColumnName("tier")
                .HasMaxLength(16)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(e => e.Email)
                .HasColumnName("email")
                .HasMaxLength(255);

            entity.Property(e => e.AddressDetail)
                .HasColumnName("address_detail")
                .HasMaxLength(500);

            entity.Property(e => e.BirthDate)
                .HasColumnName("birth_date")
                .HasColumnType("date");

            entity.Property(e => e.CccdHash)
                .HasColumnName("cccd_hash")
                .HasMaxLength(256);

            entity.Property(e => e.KycMetadata)
                .HasColumnName("kyc_metadata")
                .HasColumnType("jsonb");

            // Audit fields
            entity.Property(e => e.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(e => e.CreatedBy)
                .HasColumnName("created_by");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.Property(e => e.UpdatedBy)
                .HasColumnName("updated_by");

            // Indexes per DB-USERS.md
            entity.HasIndex(e => new { e.CompanyId, e.ExternalEmployeeCode })
                .IsUnique()
                .HasDatabaseName("ux_users_user__company_employee");

            entity.HasIndex(e => new { e.Phone, e.CompanyId })
                .IsUnique()
                .HasDatabaseName("ux_users_user__phone_company");

            entity.HasIndex(e => e.CompanyId)
                .HasDatabaseName("ix_users_user__company");

            entity.HasIndex(e => e.PickupPointId)
                .HasDatabaseName("ix_users_user__pickup_point");

            entity.HasIndex(e => new { e.Tier, e.Status })
                .HasDatabaseName("ix_users_user__tier_status");

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    private static void ConfigureUserRefTierHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRefTierHistory>(entity =>
        {
            entity.ToTable("users_ref_tier_history");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.OldParentUserId)
                .HasColumnName("old_parent_user_id");

            entity.Property(e => e.NewParentUserId)
                .HasColumnName("new_parent_user_id");

            entity.Property(e => e.OldTier)
                .HasColumnName("old_tier")
                .HasMaxLength(16)
                .IsRequired();

            entity.Property(e => e.NewTier)
                .HasColumnName("new_tier")
                .HasMaxLength(16)
                .IsRequired();

            entity.Property(e => e.ChangedAt)
                .HasColumnName("changed_at")
                .IsRequired();

            entity.Property(e => e.ChangedBy)
                .HasColumnName("changed_by")
                .IsRequired();

            entity.Property(e => e.Note)
                .HasColumnName("note")
                .HasMaxLength(500);

            // Indexes per DB-USERS.md
            entity.HasIndex(e => new { e.UserId, e.ChangedAt })
                .HasDatabaseName("ix_users_ref_tier_history__user")
                .IsDescending(false, true);
        });
    }

    private static void ConfigureUserStatusHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserStatusHistory>(entity =>
        {
            entity.ToTable("users_status_history");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(e => e.OldStatus)
                .HasColumnName("old_status")
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(e => e.NewStatus)
                .HasColumnName("new_status")
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(e => e.Reason)
                .HasColumnName("reason")
                .HasMaxLength(500);

            entity.Property(e => e.ChangedAt)
                .HasColumnName("changed_at")
                .IsRequired();

            entity.Property(e => e.ChangedBy)
                .HasColumnName("changed_by")
                .IsRequired();

            // Indexes per DB-USERS.md
            entity.HasIndex(e => new { e.UserId, e.ChangedAt })
                .HasDatabaseName("ix_users_status_history__user")
                .IsDescending(false, true);
        });
    }

    private static void ConfigureUserImportJob(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserImportJob>(entity =>
        {
            entity.ToTable("users_import_job");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.ImportUuid)
                .HasColumnName("import_uuid")
                .IsRequired();

            entity.Property(e => e.Source)
                .HasColumnName("source")
                .HasMaxLength(100);

            entity.Property(e => e.FileName)
                .HasColumnName("file_name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(e => e.TotalRows)
                .HasColumnName("total_rows");

            entity.Property(e => e.CreatedRows)
                .HasColumnName("created_rows");

            entity.Property(e => e.UpdatedRows)
                .HasColumnName("updated_rows");

            entity.Property(e => e.FailedRows)
                .HasColumnName("failed_rows");

            entity.Property(e => e.StartedAt)
                .HasColumnName("started_at");

            entity.Property(e => e.CompletedAt)
                .HasColumnName("completed_at");

            entity.Property(e => e.RequestedBy)
                .HasColumnName("requested_by")
                .IsRequired();

            // Audit fields
            entity.Property(e => e.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(e => e.CreatedBy)
                .HasColumnName("created_by");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.Property(e => e.UpdatedBy)
                .HasColumnName("updated_by");

            // Indexes per DB-USERS.md
            entity.HasIndex(e => e.ImportUuid)
                .IsUnique()
                .HasDatabaseName("ix_users_import_job__import_uuid");

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    private static void ConfigureUserImportRow(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserImportRow>(entity =>
        {
            entity.ToTable("users_import_row");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.ImportJobId)
                .HasColumnName("import_job_id")
                .IsRequired();

            entity.Property(e => e.RowNumber)
                .HasColumnName("row_number")
                .IsRequired();

            entity.Property(e => e.LogicalKey)
                .HasColumnName("logical_key")
                .HasMaxLength(255);

            entity.Property(e => e.Result)
                .HasColumnName("result")
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(e => e.ErrorCode)
                .HasColumnName("error_code")
                .HasMaxLength(64);

            entity.Property(e => e.ErrorMessage)
                .HasColumnName("error_message")
                .HasMaxLength(500);

            entity.Property(e => e.RawPayload)
                .HasColumnName("raw_payload")
                .HasColumnType("jsonb");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(e => e.CreatedBy)
                .HasColumnName("created_by");

            // Indexes per DB-USERS.md
            entity.HasIndex(e => new { e.ImportJobId, e.RowNumber })
                .HasDatabaseName("ix_users_import_row__import_job");
        });
    }
}
