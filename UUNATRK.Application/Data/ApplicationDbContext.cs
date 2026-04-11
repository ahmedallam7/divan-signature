using Microsoft.EntityFrameworkCore;
using UUNATEK.Domain.Entities;

namespace UUNATRK.Application.Data;

public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<RequestLog> RequestLogs { get; set; }
    public DbSet<PenUsageLog> PenUsageLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RequestLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RequestId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);

            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            // Pen usage relationship
            entity.HasOne(e => e.PenUsageLog)
                .WithMany(p => p.RequestLogs)
                .HasForeignKey(e => e.PenUsageLogId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        modelBuilder.Entity<PenUsageLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PenNumber).IsUnique();
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.InstalledAt);
            
            entity.Property(e => e.PenNumber).IsRequired();
            entity.Property(e => e.InstalledAt).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.TotalDistanceMm).IsRequired();
            entity.Property(e => e.TotalPrintJobs).IsRequired();
            entity.Property(e => e.TotalStrokes).IsRequired();
            entity.Property(e => e.TotalDrawingTime).IsRequired();
        });
    }
}
