using Adq.Backend.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Adq.Backend.Infrastructure.DbContexts
{
    public class AcquisitionDbContext : DbContext
    {
        public AcquisitionDbContext(DbContextOptions<AcquisitionDbContext> options)
            : base(options) { }

        public DbSet<Acquisition> Acquisitions => Set<Acquisition>();
        public DbSet<HistoryEntry> History => Set<HistoryEntry>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Acquisition>().ToTable("Acquisitions");
            modelBuilder.Entity<HistoryEntry>().ToTable("History");

            modelBuilder.Entity<Acquisition>()
                .Ignore(a => a.TotalValue); // Ignorar propiedad calculada
        }
    }
}
