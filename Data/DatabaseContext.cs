using Microsoft.EntityFrameworkCore;
using AppointmentSchedulerWeb.Models;

namespace AppointmentSchedulerWeb.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Explicitly map the table names
            modelBuilder.Entity<User>().ToTable("user");
            modelBuilder.Entity<Country>().ToTable("country");
            modelBuilder.Entity<City>().ToTable("city");
            modelBuilder.Entity<Address>().ToTable("address");
            modelBuilder.Entity<Customer>().ToTable("customer");
            modelBuilder.Entity<Appointment>().ToTable("appointment");

            modelBuilder.Entity<City>()
                .Property(c => c.CityName)
                .HasColumnName("city");

            // Define relationships

            // City → Country (Many-to-One)
            modelBuilder.Entity<City>()
                .HasOne(c => c.Country)
                .WithMany(cn => cn.Cities)
                .HasForeignKey(c => c.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Address → City (Many-to-One)
            modelBuilder.Entity<Address>()
                .HasOne(a => a.City)
                .WithMany(c => c.Addresses)
                .HasForeignKey(a => a.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Customer → Address (Many-to-One)
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.Address)
                .WithMany(a => a.Customers)
                .HasForeignKey(c => c.AddressId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment → Customer (Many-to-One)
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Customer)
                .WithMany(c => c.Appointments)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment → User (Many-to-One)
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for faster queries
            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.Start)
                .HasDatabaseName("idx_appointment_start");

            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.End)
                .HasDatabaseName("idx_appointment_end");
        }
    }
}
