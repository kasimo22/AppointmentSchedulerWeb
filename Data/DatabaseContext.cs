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

            // Explicitly map table names
            modelBuilder.Entity<User>().ToTable("user");
            modelBuilder.Entity<Country>().ToTable("country");
            modelBuilder.Entity<City>().ToTable("city");
            modelBuilder.Entity<Address>().ToTable("address");
            modelBuilder.Entity<Customer>().ToTable("customer");
            modelBuilder.Entity<Appointment>().ToTable("appointment");

            // User Entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.Property(u => u.UserId).HasColumnName("userid");
                entity.Property(u => u.UserName).HasColumnName("username");
                entity.Property(u => u.Password).HasColumnName("password");
                entity.Property(u => u.Active).HasColumnName("active");
                entity.Property(u => u.CreateDate).HasColumnName("createdate");
                entity.Property(u => u.CreatedBy).HasColumnName("createdby");
                entity.Property(u => u.LastUpdate).HasColumnName("lastupdate");
                entity.Property(u => u.LastUpdateBy).HasColumnName("lastupdateby");
            });

            // Country Entity
            modelBuilder.Entity<Country>(entity =>
            {
                entity.HasKey(c => c.CountryId);
                entity.Property(c => c.CountryId).HasColumnName("countryid");
                entity.Property(c => c.Name).HasColumnName("country");
                entity.Property(c => c.CreateDate).HasColumnName("createdate");
                entity.Property(c => c.CreatedBy).HasColumnName("createdby");
                entity.Property(c => c.LastUpdate).HasColumnName("lastupdate");
                entity.Property(c => c.LastUpdateBy).HasColumnName("lastupdateby");
            });

            // City Entity
            modelBuilder.Entity<City>(entity =>
            {
                entity.HasKey(c => c.CityId);
                entity.Property(c => c.CityId).HasColumnName("cityid");
                entity.Property(c => c.CityName).HasColumnName("city");
                entity.Property(c => c.CountryId).HasColumnName("countryid");
                entity.Property(c => c.CreateDate).HasColumnName("createdate");
                entity.Property(c => c.CreatedBy).HasColumnName("createdby");
                entity.Property(c => c.LastUpdate).HasColumnName("lastupdate");
                entity.Property(c => c.LastUpdateBy).HasColumnName("lastupdateby");

                // Foreign Key: City → Country (Many-to-One)
                entity.HasOne(c => c.Country)
                      .WithMany(cn => cn.Cities)
                      .HasForeignKey(c => c.CountryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Address Entity
            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(a => a.AddressId);
                entity.Property(a => a.AddressId).HasColumnName("addressid");
                entity.Property(a => a.AddressLine1).HasColumnName("address");
                entity.Property(a => a.AddressLine2).HasColumnName("address2");
                entity.Property(a => a.CityId).HasColumnName("cityid");
                entity.Property(a => a.PostalCode).HasColumnName("postalcode");
                entity.Property(a => a.Phone).HasColumnName("phone");
                entity.Property(a => a.CreateDate).HasColumnName("createdate");
                entity.Property(a => a.CreatedBy).HasColumnName("createdby");
                entity.Property(a => a.LastUpdate).HasColumnName("lastupdate");
                entity.Property(a => a.LastUpdateBy).HasColumnName("lastupdateby");

                // Foreign Key: Address → City (Many-to-One)
                entity.HasOne(a => a.City)
                      .WithMany(c => c.Addresses)
                      .HasForeignKey(a => a.CityId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Customer Entity
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.CustomerId);
                entity.Property(c => c.CustomerId).HasColumnName("customerid");
                entity.Property(c => c.CustomerName).HasColumnName("customername");
                entity.Property(c => c.AddressId).HasColumnName("addressid");
                entity.Property(c => c.Active).HasColumnName("active");
                entity.Property(c => c.CreateDate).HasColumnName("createdate");
                entity.Property(c => c.CreatedBy).HasColumnName("createdby");
                entity.Property(c => c.LastUpdate).HasColumnName("lastupdate");
                entity.Property(c => c.LastUpdateBy).HasColumnName("lastupdateby");

                // Foreign Key: Customer → Address (Many-to-One)
                entity.HasOne(c => c.Address)
                      .WithMany(a => a.Customers)
                      .HasForeignKey(c => c.AddressId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Appointment Entity
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(a => a.AppointmentId);
                entity.Property(a => a.AppointmentId)
                    .HasColumnName("appointmentid")
                    .ValueGeneratedOnAdd();
                entity.Property(a => a.CustomerId).HasColumnName("customerid");
                entity.Property(a => a.UserId).HasColumnName("userid");
                entity.Property(a => a.Title).HasColumnName("title");
                entity.Property(a => a.Description).HasColumnName("description");
                entity.Property(a => a.Location).HasColumnName("location");
                entity.Property(a => a.Contact).HasColumnName("contact");
                entity.Property(a => a.Type).HasColumnName("type");
                entity.Property(a => a.Url).HasColumnName("url");
                entity.Property(a => a.Start)
                    .HasColumnName("start")
                    .HasColumnType("timestamp with time zone");
                entity.Property(a => a.End)
                    .HasColumnName("end")
                    .HasColumnType("timestamp with time zone");
                entity.Property(a => a.CreateDate).HasColumnName("createdate");
                entity.Property(a => a.CreatedBy).HasColumnName("createdby");
                entity.Property(a => a.LastUpdate).HasColumnName("lastupdate");
                entity.Property(a => a.LastUpdateBy).HasColumnName("lastupdateby");

                // Foreign Key: Appointment → Customer (Many-to-One)
                entity.HasOne(a => a.Customer)
                      .WithMany(c => c.Appointments)
                      .HasForeignKey(a => a.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Foreign Key: Appointment → User (Many-to-One)
                entity.HasOne(a => a.User)
                      .WithMany(u => u.Appointments)
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
                    .EnableSensitiveDataLogging(); // Enables detailed error logging
            }
        }
    }
}
