using Microsoft.EntityFrameworkCore;
using Give_AID.Models;

namespace Give_AID
{
    // Primary database context for the GIVE-AID application
    public class Give_AIDContext : DbContext
    {
        // Constructor accepting database configuration options
        public Give_AIDContext(DbContextOptions<Give_AIDContext> options) : base(options)
        {
        }

        // DbSets representing each table in the database
        public DbSet<Users> Users { get; set; } = default!;
        public DbSet<Causes> Causes { get; set; } = default!;
        public DbSet<Ngos> Ngos { get; set; } = default!;
        public DbSet<Donations> Donations { get; set; } = default!;
        public DbSet<Partners> Partners { get; set; } = default!;
        public DbSet<Programmes> Programmes { get; set; } = default!;
        public DbSet<ProgrammeInterests> ProgrammeInterests { get; set; } = default!;
        public DbSet<GalleryImages> GalleryImages { get; set; } = default!;
        public DbSet<PageContents> PageContents { get; set; } = default!;
        public DbSet<Queries> Queries { get; set; } = default!;
        public DbSet<Invites> Invites { get; set; } = default!;
        public DbSet<PaymentSettings> PaymentSettings { get; set; } = default!;

        // Configure table mappings, constraints, and relationships
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Donations relationships
            // Restrict cascade delete to prevent multiple cascade paths in SQL Server
            modelBuilder.Entity<Donations>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(u => u.Donations)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Cause)
                    .WithMany(c => c.Donations)
                    .HasForeignKey(d => d.CauseId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Ngo)
                    .WithMany(n => n.Donations)
                    .HasForeignKey(d => d.NgoId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Programmes relationships
            modelBuilder.Entity<Programmes>(entity =>
            {
                entity.HasOne(p => p.Ngo)
                    .WithMany(n => n.Programmes)
                    .HasForeignKey(p => p.NgoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ProgrammeInterests relationships
            modelBuilder.Entity<ProgrammeInterests>(entity =>
            {
                entity.HasOne(pi => pi.User)
                    .WithMany(u => u.ProgrammeInterests)
                    .HasForeignKey(pi => pi.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(pi => pi.Programme)
                    .WithMany(p => p.ProgrammeInterests)
                    .HasForeignKey(pi => pi.ProgrammeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure GalleryImages relationships
            modelBuilder.Entity<GalleryImages>(entity =>
            {
                entity.HasOne(g => g.Programme)
                    .WithMany(p => p.GalleryImages)
                    .HasForeignKey(g => g.ProgrammeId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Queries relationships
            modelBuilder.Entity<Queries>(entity =>
            {
                entity.HasOne(q => q.User)
                    .WithMany(u => u.Queries)
                    .HasForeignKey(q => q.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Invites relationships
            modelBuilder.Entity<Invites>(entity =>
            {
                entity.HasOne(i => i.User)
                    .WithMany(u => u.Invites)
                    .HasForeignKey(i => i.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Ensure unique slug for page contents lookups
            modelBuilder.Entity<PageContents>(entity =>
            {
                entity.HasIndex(p => p.Slug).IsUnique();
            });
        }
    }
}
