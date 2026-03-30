using Microsoft.EntityFrameworkCore;
using Medication_Tracker.Models;

namespace Medication_Tracker.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Medication> Medications { get; set; } = null!;
        public DbSet<MedicationSchedule> MedicationSchedules { get; set; } = null!;
        public DbSet<MedicationLog> MedicationLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure Medication entity
            modelBuilder.Entity<Medication>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Dosage).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Frequency).IsRequired().HasMaxLength(100);
            });

            // Configure MedicationSchedule entity
            modelBuilder.Entity<MedicationSchedule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ScheduleTime).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Notes).HasMaxLength(500);
                
                // Foreign keys
                entity.HasOne(e => e.User)
                    .WithMany(u => u.MedicationSchedules)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Medication)
                    .WithMany(m => m.MedicationSchedules)
                    .HasForeignKey(e => e.MedicationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure MedicationLog entity
            modelBuilder.Entity<MedicationLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Notes).HasMaxLength(500);

                // Foreign keys
                entity.HasOne(e => e.User)
                    .WithMany(u => u.MedicationLogs)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Medication)
                    .WithMany(m => m.MedicationLogs)
                    .HasForeignKey(e => e.MedicationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.MedicationSchedule)
                    .WithMany(s => s.MedicationLogs)
                    .HasForeignKey(e => e.MedicationScheduleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
