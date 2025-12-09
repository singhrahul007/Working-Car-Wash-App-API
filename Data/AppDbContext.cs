// Data/AppDbContext.cs
using CarWash.Api.Entities;
using CarWash.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarWash.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Register all entity classes
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<LoginSession> LoginSessions { get; set; }
        public DbSet<SocialAuth> SocialAuths { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<ServiceReview> ServiceReviews { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<OTP> OTPs { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
                entity.HasIndex(u => u.MobileNumber).IsUnique().HasFilter("[MobileNumber] IS NOT NULL");
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(u => u.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Relationships
                entity.HasMany(u => u.UserRoles)
                    .WithOne(ur => ur.User)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.LoginSessions)
                    .WithOne(ls => ls.User)
                    .HasForeignKey(ls => ls.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.SocialAuths)
                    .WithOne(sa => sa.User)
                    .HasForeignKey(sa => sa.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.Addresses)
                    .WithOne(a => a.User)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.Bookings)
                    .WithOne(b => b.User)
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.Reviews)
                    .WithOne(sr => sr.User)
                    .HasForeignKey(sr => sr.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Role configuration
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Name).IsRequired().HasMaxLength(50);
                entity.HasIndex(r => r.Name).IsUnique();
            });

            // UserRole configuration
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => ur.Id);
                entity.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();

                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Price).HasColumnType("decimal(10,2)");
                entity.Property(p => p.DiscountedPrice).HasColumnType("decimal(10,2)");
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(p => p.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Service configuration
            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
                entity.Property(s => s.Price).HasColumnType("decimal(10,2)");
                entity.Property(s => s.DiscountedPrice).HasColumnType("decimal(10,2)");
                entity.Property(s => s.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(s => s.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Relationships
                entity.HasMany(s => s.Bookings)
                    .WithOne(b => b.Service)
                    .HasForeignKey(b => b.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(s => s.ServiceReviews)
                    .WithOne(sr => sr.Service)
                    .HasForeignKey(sr => sr.ServiceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Booking configuration
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.BookingId).IsRequired().HasMaxLength(50);
                entity.HasIndex(b => b.BookingId).IsUnique();
                entity.Property(b => b.TotalAmount).HasColumnType("decimal(10,2)");
                entity.Property(b => b.Subtotal).HasColumnType("decimal(10,2)");
                entity.Property(b => b.DiscountAmount).HasColumnType("decimal(10,2)");
                entity.Property(b => b.TaxAmount).HasColumnType("decimal(10,2)");
                entity.Property(b => b.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Relationships
                entity.HasOne(b => b.User)
                    .WithMany(u => u.Bookings)
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(b => b.Service)
                    .WithMany(s => s.Bookings)
                    .HasForeignKey(b => b.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Address)
                    .WithMany()
                    .HasForeignKey(b => b.AddressId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Address configuration
            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.HasOne(a => a.User)
                    .WithMany(u => u.Addresses)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ServiceReview configuration
            modelBuilder.Entity<ServiceReview>(entity =>
            {
                entity.HasKey(sr => sr.Id);
                entity.HasIndex(sr => new { sr.UserId, sr.ServiceId });

                entity.HasOne(sr => sr.User)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(sr => sr.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sr => sr.Service)
                    .WithMany(s => s.ServiceReviews)
                    .HasForeignKey(sr => sr.ServiceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // OTP configuration
            modelBuilder.Entity<OTP>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.HasIndex(o => new { o.MobileNumber, o.Type, o.CreatedAt });
                entity.HasIndex(o => new { o.Email, o.Type, o.CreatedAt });
                entity.HasIndex(o => o.ExpiresAt);
                entity.Property(o => o.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(o => o.User)
                    .WithMany(u => u.OTPs)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // LoginSession configuration
            modelBuilder.Entity<LoginSession>(entity =>
            {
                entity.HasKey(ls => ls.Id);
                entity.HasIndex(ls => ls.SessionId).IsUnique();
                entity.HasIndex(ls => new { ls.UserId, ls.IsActive });
                entity.HasIndex(ls => ls.ExpiresAt);
                entity.Property(ls => ls.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(ls => ls.LastActivity).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(ls => ls.User)
                    .WithMany(u => u.LoginSessions)
                    .HasForeignKey(ls => ls.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // SocialAuth configuration
            modelBuilder.Entity<SocialAuth>(entity =>
            {
                entity.HasKey(sa => sa.Id);
                entity.HasIndex(sa => new { sa.Provider, sa.ProviderId }).IsUnique();
                entity.HasIndex(sa => new { sa.UserId, sa.Provider });
                entity.Property(sa => sa.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(sa => sa.LastUsed).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(sa => sa.User)
                    .WithMany(u => u.SocialAuths)
                    .HasForeignKey(sa => sa.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Offer configuration
            modelBuilder.Entity<Offer>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.HasIndex(o => o.Code).IsUnique();
                entity.Property(o => o.DiscountValue).HasColumnType("decimal(10,2)");
                entity.Property(o => o.MinAmount).HasColumnType("decimal(10,2)");
            });
        }
    }
}