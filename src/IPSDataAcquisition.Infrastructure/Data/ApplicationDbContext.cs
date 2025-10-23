using IPSDataAcquisition.Application.Common.Interfaces;
using IPSDataAcquisition.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IPSDataAcquisition.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<ButtonPress> ButtonPresses => Set<ButtonPress>();
    public DbSet<IMUData> IMUData => Set<IMUData>();
    public DbSet<Bonus> Bonuses => Set<Bonus>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AppVersion> AppVersions => Set<AppVersion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ApplicationUser configuration
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("users");
            entity.Property(e => e.FullName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(20).IsRequired();
            entity.HasIndex(e => e.PhoneNumber).IsUnique();
        });

        // Session configuration
        modelBuilder.Entity<Session>(entity =>
        {
            entity.ToTable("sessions");
            entity.HasKey(e => e.SessionId);
            
            // Core session properties
            entity.Property(e => e.SessionId).HasMaxLength(36);
            entity.Property(e => e.UserId).HasMaxLength(36);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.PaymentStatus).HasMaxLength(20);
            entity.Property(e => e.BonusAmount).HasColumnType("decimal(10,2)");
            
            // Quality scoring properties
            entity.Property(e => e.QualityScore)
                .HasColumnType("decimal(5,2)")
                .HasComment("Overall quality score (0-100)");
            
            entity.Property(e => e.QualityStatus)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Quality check status: 0=pending, 1=completed, 2=failed");
            
            entity.Property(e => e.QualityCheckedAt)
                .HasComment("Timestamp when quality check was performed");
            
            entity.Property(e => e.QualityRemarks)
                .HasMaxLength(1000)
                .HasComment("Human-readable quality issues/notes");
            
            // Data volume metrics
            entity.Property(e => e.TotalIMUDataPoints)
                .HasComment("Total number of IMU data points");
            
            entity.Property(e => e.TotalButtonPresses)
                .HasComment("Total number of button press events");
            
            entity.Property(e => e.DurationMinutes)
                .HasColumnType("double precision")
                .HasComment("Session duration in minutes");
            
            // Sensor coverage percentages
            entity.Property(e => e.AccelDataCoverage)
                .HasColumnType("decimal(5,2)")
                .HasComment("Percentage of records with accelerometer data (0-100)");
            
            entity.Property(e => e.GyroDataCoverage)
                .HasColumnType("decimal(5,2)")
                .HasComment("Percentage of records with gyroscope data (0-100)");
            
            entity.Property(e => e.MagDataCoverage)
                .HasColumnType("decimal(5,2)")
                .HasComment("Percentage of records with magnetometer data (0-100)");
            
            entity.Property(e => e.GpsDataCoverage)
                .HasColumnType("decimal(5,2)")
                .HasComment("Percentage of records with GPS data (0-100)");
            
            entity.Property(e => e.BarometerDataCoverage)
                .HasColumnType("decimal(5,2)")
                .HasComment("Percentage of records with barometer/pressure data (0-100)");
            
            // Quality flags
            entity.Property(e => e.HasAnomalies)
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("Flag indicating if sensor anomalies were detected");
            
            entity.Property(e => e.HasDataGaps)
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("Flag indicating if data gaps were detected");
            
            entity.Property(e => e.DataGapCount)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Number of data gaps detected (>1 second)");
            
            // Flexible JSON storage
            entity.Property(e => e.QualityMetricsRawJson)
                .HasColumnType("jsonb")
                .HasComment("Extended quality metrics and ML features in JSON format");

            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartTimestamp);
            
            // Quality check indexes
            entity.HasIndex(e => e.QualityStatus)
                .HasDatabaseName("idx_sessions_quality_status");
            
            entity.HasIndex(e => e.QualityScore)
                .HasDatabaseName("idx_sessions_quality_score");
            
            entity.HasIndex(e => new { e.QualityScore, e.HasAnomalies, e.Status })
                .HasDatabaseName("idx_sessions_quality_analysis");

            // Relationships
            entity.HasOne(e => e.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ButtonPress configuration
        modelBuilder.Entity<ButtonPress>(entity =>
        {
            entity.ToTable("button_presses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).HasMaxLength(36).IsRequired();
            entity.Property(e => e.UserId).HasMaxLength(36);
            entity.Property(e => e.Action).HasMaxLength(50).IsRequired();

            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Timestamp);

            entity.HasOne(e => e.Session)
                .WithMany(s => s.ButtonPresses)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<ApplicationUser>()
                .WithMany(u => u.ButtonPresses)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // IMUData configuration
        modelBuilder.Entity<IMUData>(entity =>
        {
            entity.ToTable("imu_data");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).HasMaxLength(36);
            entity.Property(e => e.UserId).HasMaxLength(36);
            
            // All sensor fields are nullable - not all devices have all sensors
            // Float in PostgreSQL (REAL) supports sufficient precision for 3 decimal places

            entity.HasIndex(e => new { e.SessionId, e.Timestamp });
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.Session)
                .WithMany(s => s.IMUDataPoints)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);  // Allow IMU data without session

            entity.HasOne<ApplicationUser>()
                .WithMany(u => u.IMUDataRecords)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Bonus configuration
        modelBuilder.Entity<Bonus>(entity =>
        {
            entity.ToTable("bonuses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(36);
            entity.Property(e => e.Amount).HasColumnType("decimal(10,2)");

            entity.HasIndex(e => new { e.UserId, e.Date }).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Date);
        });

        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).HasMaxLength(500).IsRequired();
            entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.ReplacedByToken).HasMaxLength(500);

            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ExpiresAt);

            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AppVersion configuration
        modelBuilder.Entity<AppVersion>(entity =>
        {
            entity.ToTable("app_versions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VersionName).HasMaxLength(50).IsRequired();

            entity.HasIndex(e => e.VersionName).IsUnique();
            entity.HasIndex(e => e.Active);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Session session)
            {
                session.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is ButtonPress buttonPress)
            {
                buttonPress.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is IMUData imuData)
            {
                imuData.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

