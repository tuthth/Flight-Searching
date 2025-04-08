using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace FlightSearching_Library.Models
{
    public partial class FlightTicketSearchContext : DbContext
    {
        public FlightTicketSearchContext()
        {
        }

        public FlightTicketSearchContext(DbContextOptions<FlightTicketSearchContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Airline> Airlines { get; set; } = null!;
        public virtual DbSet<Airport> Airports { get; set; } = null!;
        public virtual DbSet<Flight> Flights { get; set; } = null!;
        public virtual DbSet<Request> Requests { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(GetConnectionString());
            }
        }
        public string GetConnectionString()
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();
            var strConn = config["ConnectionStrings:FlightTicketSearch"];
            return strConn;
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Airline>(entity =>
            {
                entity.HasKey(e => e.AirlineCode)
                    .HasName("PK__Airlines__79E77E12D4ACF9C4");

                entity.Property(e => e.AirlineCode)
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.AirlineName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Airport>(entity =>
            {
                entity.HasKey(e => e.AirportCode)
                    .HasName("PK__Airports__4B67735291ED97CA");

                entity.Property(e => e.AirportCode)
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.AirportName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.City)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Country)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Flight>(entity =>
            {
                entity.Property(e => e.FlightId)
                    .ValueGeneratedNever()
                    .HasColumnName("FlightID");

                entity.Property(e => e.Airline)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.AirlineCode)
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.ArrivalAirportCode)
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.ArrivalDate).HasColumnType("datetime");

                entity.Property(e => e.ArrivalLocation)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.DepartureAirportCode)
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.DepartureDate).HasColumnType("datetime");

                entity.Property(e => e.DepartureLocation)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Price).HasColumnType("decimal(15, 2)");

                entity.HasOne(d => d.AirlineCodeNavigation)
                    .WithMany(p => p.Flights)
                    .HasForeignKey(d => d.AirlineCode)
                    .HasConstraintName("FK__Flights__Airline__5070F446");

                entity.HasOne(d => d.ArrivalAirportCodeNavigation)
                    .WithMany(p => p.FlightArrivalAirportCodeNavigations)
                    .HasForeignKey(d => d.ArrivalAirportCode)
                    .HasConstraintName("FK__Flights__Arrival__4F7CD00D");

                entity.HasOne(d => d.DepartureAirportCodeNavigation)
                    .WithMany(p => p.FlightDepartureAirportCodeNavigations)
                    .HasForeignKey(d => d.DepartureAirportCode)
                    .HasConstraintName("FK__Flights__Departu__4E88ABD4");
            });

            modelBuilder.Entity<Request>(entity =>
            {
                entity.Property(e => e.RequestId)
                    .ValueGeneratedNever()
                    .HasColumnName("RequestID");

                entity.Property(e => e.ArrivalLocation)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ContactEmail)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.DepartureDate).HasColumnType("date");

                entity.Property(e => e.DepartureLocation)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ReturnDate).HasColumnType("date");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
