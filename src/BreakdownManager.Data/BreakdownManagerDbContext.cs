using BreakdownManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BreakdownManager.Data;

public class BreakdownManagerDbContext : DbContext
{
    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Breakdown> Breakdowns => Set<Breakdown>();
    public DbSet<SparePart> SpareParts => Set<SparePart>();
    public DbSet<BreakdownSparePart> BreakdownSpareParts => Set<BreakdownSparePart>();
    public DbSet<Attachment> Attachments => Set<Attachment>();

    public BreakdownManagerDbContext(DbContextOptions<BreakdownManagerDbContext> options)
        : base(options)
    {
    }

    /// <summary>Fallback so the context can be used outside DI (e.g. design-time tools) with the default file.</summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=BreakdownManager.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Machine>(e =>
        {
            e.HasIndex(m => m.Code).IsUnique();
            e.Property(m => m.Code).HasMaxLength(50).IsRequired();
            e.Property(m => m.Name).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Username).IsUnique();
            e.Property(u => u.Username).HasMaxLength(100).IsRequired();
            e.Property(u => u.FullName).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<Breakdown>(e =>
        {
            e.HasIndex(b => b.TicketNumber).IsUnique();
            e.Property(b => b.TicketNumber).HasMaxLength(30).IsRequired();
            e.Property(b => b.Description).HasMaxLength(2000).IsRequired();

            e.HasOne(b => b.Machine)
                .WithMany(m => m.Breakdowns)
                .HasForeignKey(b => b.MachineId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(b => b.ReportedBy)
                .WithMany(u => u.ReportedBreakdowns)
                .HasForeignKey(b => b.ReportedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(b => b.AssignedTechnician)
                .WithMany(u => u.AssignedBreakdowns)
                .HasForeignKey(b => b.AssignedTechnicianId)
                .OnDelete(DeleteBehavior.Restrict);

            // Computed properties aren't real columns.
            e.Ignore(b => b.ResponseTime);
            e.Ignore(b => b.Mttr);
            e.Ignore(b => b.Downtime);
            e.Ignore(b => b.IsOpen);
        });

        modelBuilder.Entity<SparePart>(e =>
        {
            e.HasIndex(p => p.PartNumber).IsUnique();
            e.Property(p => p.Cost).HasPrecision(10, 2);
            e.Ignore(p => p.IsBelowMinimum);
        });

        modelBuilder.Entity<BreakdownSparePart>(e =>
        {
            e.HasKey(bp => new { bp.BreakdownId, bp.SparePartId });

            e.HasOne(bp => bp.Breakdown)
                .WithMany(b => b.SparePartsUsed)
                .HasForeignKey(bp => bp.BreakdownId);

            e.HasOne(bp => bp.SparePart)
                .WithMany(p => p.BreakdownUsages)
                .HasForeignKey(bp => bp.SparePartId);
        });

        modelBuilder.Entity<Attachment>(e =>
        {
            e.HasOne(a => a.Breakdown)
                .WithMany(b => b.Attachments)
                .HasForeignKey(a => a.BreakdownId);
        });
    }
}
