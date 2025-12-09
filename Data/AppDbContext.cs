// Data/AppDbContext.cs
using CarWash.Api.Entities;
using CarWash.Api.Models;
using CarWash.Api.Models.Entities;
using CarWash.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace CarWash.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Use explicit types to avoid ambiguity
        public DbSet<Entities.User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Entities.Booking> Bookings { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<OTP> OTPs { get; set; }
        public DbSet<ServiceReview> ServiceReviews { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<LoginSession> LoginSessions { get; set; }
        public DbSet<SocialAuth> SocialAuths { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<Entities.User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
                entity.HasIndex(u => u.MobileNumber).IsUnique().HasFilter("[MobileNumber] IS NOT NULL");

                entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(u => u.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // OTP configuration
            modelBuilder.Entity<OTP>(entity =>
            {
                entity.HasIndex(o => new { o.MobileNumber, o.Type, o.CreatedAt });
                entity.HasIndex(o => new { o.Email, o.Type, o.CreatedAt });
                entity.HasIndex(o => o.ExpiresAt);

                entity.Property(o => o.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
                entity.Property(p => p.DiscountedPrice).HasColumnType("decimal(18,2)");
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(p => p.UpdatedAt).HasDefaultValueSql("GETDATE()");
            });

            // LoginSession configuration
            modelBuilder.Entity<LoginSession>(entity =>
            {
                entity.HasIndex(ls => ls.SessionId).IsUnique();
                entity.HasIndex(ls => new { ls.UserId, ls.IsActive });
                entity.HasIndex(ls => ls.ExpiresAt);

                entity.Property(ls => ls.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(ls => ls.LastActivity).HasDefaultValueSql("GETUTCDATE()");
            });

            // SocialAuth configuration
            modelBuilder.Entity<SocialAuth>(entity =>
            {
                entity.HasIndex(sa => new { sa.Provider, sa.ProviderId }).IsUnique();
                entity.HasIndex(sa => new { sa.UserId, sa.Provider });

                entity.Property(sa => sa.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(sa => sa.LastUsed).HasDefaultValueSql("GETUTCDATE()");
            });

            // Relationships
            modelBuilder.Entity<LoginSession>()
                .HasOne(ls => ls.User)
                .WithMany(u => u.LoginSessions)
                .HasForeignKey(ls => ls.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SocialAuth>()
                .HasOne(sa => sa.User)
                .WithMany(u => u.SocialAuths)
                .HasForeignKey(sa => sa.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OTP>()
                .HasOne(o => o.User)
                .WithMany(u => u.OTPs)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
                entity.Property(s => s.Price).HasColumnType("decimal(18,2)");
                entity.Property(s => s.DiscountedPrice).HasColumnType("decimal(18,2)");
                entity.Property(s => s.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(s => s.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Relationships
                entity.HasMany(s => s.Reviews)
                    .WithOne(r => r.Service)
                    .HasForeignKey(r => r.ServiceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
                entity.Property(p => p.DiscountedPrice).HasColumnType("decimal(18,2)");
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(p => p.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}