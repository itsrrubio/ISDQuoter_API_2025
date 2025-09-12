using Microsoft.EntityFrameworkCore;
using ISDQuoter_API.Models;

namespace ISDQuoter_API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            :base(options) 
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<PrintChargeMatrix> PrintChargeMatrix { get; set; }
        public DbSet<JobQuote> JobQuotes { get; set; }
        public DbSet<JobGraphic> JobGraphics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Fluent API (optional)

            modelBuilder.Entity<Product>()
                .HasKey(p => p.GarmentId);

            modelBuilder.Entity<JobQuote>()
                .HasOne(j => j.Garment)
                .WithMany(p => p.JobQuotes)
                .HasForeignKey(j => j.GarmentId);

            modelBuilder.Entity<JobQuote>()
                .HasMany(j => j.Graphics)
                .WithOne(g => g.JobQuote)
                .HasForeignKey(g => g.QuoteId);
        }


    }
}
