using CoShare.Domain.Auth;
using Microsoft.EntityFrameworkCore;

namespace CoShare.Infrastructure.Data;

/// <summary>
/// EF Core DbContext for AUTH module.
/// Maps to tables defined in DB-AUTH.md.
/// </summary>
public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<OtpRequest> OtpRequests => Set<OtpRequest>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureOtpRequest(modelBuilder);
        ConfigureRefreshToken(modelBuilder);
    }

    private static void ConfigureOtpRequest(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OtpRequest>(entity =>
        {
            entity.ToTable("auth_otp_request");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.Phone)
                .HasColumnName("phone")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.Purpose)
                .HasColumnName("purpose")
                .HasMaxLength(50)
                .HasConversion(
                    v => v.ToString().ToUpperInvariant().Replace("ENDUSER", "END_USER"),
                    v => Enum.Parse<OtpPurpose>(v.Replace("END_USER", "EndUser"), true))
                .IsRequired();

            entity.Property(e => e.OtpCodeHash)
                .HasColumnName("otp_code_hash")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.ExpiresAt)
                .HasColumnName("expires_at")
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(30)
                .HasConversion(
                    v => v.ToString().ToUpperInvariant(),
                    v => Enum.Parse<OtpStatus>(v, true))
                .IsRequired();

            entity.Property(e => e.AttemptCount)
                .HasColumnName("attempt_count")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(e => e.LastAttemptAt)
                .HasColumnName("last_attempt_at");

            entity.Property(e => e.NotificationTemplateCode)
                .HasColumnName("notification_template_code")
                .HasMaxLength(100);

            entity.Property(e => e.NotificationMessageId)
                .HasColumnName("notification_message_id")
                .HasMaxLength(100);

            entity.Property(e => e.CorrelationId)
                .HasColumnName("correlation_id")
                .HasMaxLength(100);

            // Registration-specific fields (stored in same table for simplicity)
            entity.Property(e => e.FullName)
                .HasColumnName("full_name")
                .HasMaxLength(200);

            entity.Property(e => e.CompanyId)
                .HasColumnName("company_id");

            // Audit fields
            entity.Property(e => e.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(e => e.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(500);

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.Property(e => e.UpdatedBy)
                .HasColumnName("updated_by")
                .HasMaxLength(500);

            // Indexes per DB-AUTH.md
            entity.HasIndex(e => new { e.Phone, e.Purpose, e.Status })
                .HasDatabaseName("ix_auth_otp_request__phone_purpose_status");

            entity.HasIndex(e => e.ExpiresAt)
                .HasDatabaseName("ix_auth_otp_request__expires_at");

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    private static void ConfigureRefreshToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("auth_refresh_token");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.UserId)
                .HasColumnName("user_id");

            entity.Property(e => e.AdminUserId)
                .HasColumnName("admin_user_id");

            entity.Property(e => e.TokenHash)
                .HasColumnName("token_hash")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.DeviceId)
                .HasColumnName("device_id")
                .HasMaxLength(128);

            entity.Property(e => e.UserAgent)
                .HasColumnName("user_agent")
                .HasMaxLength(512);

            entity.Property(e => e.Platform)
                .HasColumnName("platform")
                .HasMaxLength(128);

            entity.Property(e => e.ExpiresAt)
                .HasColumnName("expires_at")
                .IsRequired();

            entity.Property(e => e.RevokedAt)
                .HasColumnName("revoked_at");

            entity.Property(e => e.RevokedReason)
                .HasColumnName("revoked_reason")
                .HasMaxLength(255);

            entity.Property(e => e.CorrelationId)
                .HasColumnName("correlation_id")
                .HasMaxLength(100);

            // Audit fields
            entity.Property(e => e.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(e => e.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(500);

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.Property(e => e.UpdatedBy)
                .HasColumnName("updated_by")
                .HasMaxLength(500);

            // Indexes per DB-AUTH.md
            entity.HasIndex(e => e.TokenHash)
                .IsUnique()
                .HasDatabaseName("ux_auth_refresh_token__token_hash");

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("ix_auth_refresh_token__user_id");

            entity.HasIndex(e => e.AdminUserId)
                .HasDatabaseName("ix_auth_refresh_token__admin_user_id");

            entity.HasIndex(e => e.ExpiresAt)
                .HasDatabaseName("ix_auth_refresh_token__expires_at");

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}
