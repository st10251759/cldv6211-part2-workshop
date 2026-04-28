using Microsoft.EntityFrameworkCore;
using MediBook.Models;

/*
==============================Code Attribution==================================
Entity Framework Core DbContext
Author: Microsoft
Link: https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/
Date Accessed: 28 April 2026
==============================Code Attribution==================================
*/

namespace MediBook.Data
{
    // The database context for MediBook — acts as the bridge between
    // the application models and the underlying SQL database.
    public class MediBookDbContext : DbContext
    {
        public MediBookDbContext(DbContextOptions<MediBookDbContext> options)
            : base(options)
        {
        }

        // Represents the Facilities table in the database
        public DbSet<Facility> Facilities { get; set; }

        // Represents the MedicalSessions table in the database
        public DbSet<MedicalSession> MedicalSessions { get; set; }

        // Represents the Reservations table in the database
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configures the one-to-many relationship between Facility and Reservation.
            // Deleting a Facility that has Reservations is restricted at the DB level.
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Facility)
                .WithMany(f => f.Reservations)
                .HasForeignKey(r => r.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configures the one-to-many relationship between MedicalSession and Reservation.
            // Deleting a MedicalSession that has Reservations is restricted at the DB level.
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.MedicalSession)
                .WithMany(s => s.Reservations)
                .HasForeignKey(r => r.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}