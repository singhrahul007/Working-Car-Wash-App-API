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

                // Relationships (no cascade on Bookings to avoid multiple cascade paths)
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

                entity.HasMany(u => u.Reviews)
                    .WithOne(sr => sr.User)
                    .HasForeignKey(sr => sr.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // OTPs - soft delete (set null)
                entity.HasMany(u => u.OTPs)
                    .WithOne(o => o.User)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Role configuration
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Name).IsRequired().HasMaxLength(50);
                entity.HasIndex(r => r.Name).IsUnique();
                entity.Property(r => r.Description).HasMaxLength(200);
            });

            // UserRole configuration
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => ur.Id);
                entity.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
                entity.Property(ur => ur.AssignedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // LoginSession configuration
            modelBuilder.Entity<LoginSession>(entity =>
            {
                entity.HasKey(ls => ls.Id);
                entity.HasIndex(ls => ls.SessionId).IsUnique();
                entity.HasIndex(ls => new { ls.UserId, ls.IsActive });
                entity.HasIndex(ls => ls.ExpiresAt);

                entity.Property(ls => ls.SessionId).IsRequired().HasMaxLength(100);
                entity.Property(ls => ls.RefreshToken).IsRequired().HasMaxLength(500);
                entity.Property(ls => ls.DeviceType).HasMaxLength(50);
                entity.Property(ls => ls.DeviceName).HasMaxLength(100);
                entity.Property(ls => ls.OS).HasMaxLength(50);
                entity.Property(ls => ls.Browser).HasMaxLength(50);
                entity.Property(ls => ls.IpAddress).HasMaxLength(50);
                entity.Property(ls => ls.UserAgent).HasMaxLength(500);

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

                entity.Property(sa => sa.Provider).IsRequired().HasMaxLength(20);
                entity.Property(sa => sa.ProviderId).IsRequired().HasMaxLength(100);
                entity.Property(sa => sa.Email).HasMaxLength(255);
                entity.Property(sa => sa.AccessToken).HasMaxLength(500);
                entity.Property(sa => sa.RefreshToken).HasMaxLength(500);

                entity.Property(sa => sa.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(sa => sa.LastUsed).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(sa => sa.User)
                    .WithMany(u => u.SocialAuths)
                    .HasForeignKey(sa => sa.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Address configuration
            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.Property(a => a.FullAddress).IsRequired().HasMaxLength(200);
                entity.Property(a => a.City).IsRequired().HasMaxLength(100);
                entity.Property(a => a.State).IsRequired().HasMaxLength(100);
                entity.Property(a => a.Country).IsRequired().HasMaxLength(100);
                entity.Property(a => a.PostalCode).HasMaxLength(20);

                entity.Property(a => a.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(a => a.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(a => a.User)
                    .WithMany(u => u.Addresses)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Description).IsRequired();
                entity.Property(p => p.Price).HasColumnType("decimal(10,2)");
                entity.Property(p => p.DiscountedPrice).HasColumnType("decimal(10,2)");
                entity.Property(p => p.Category).IsRequired();
                entity.Property(p => p.StockQuantity).HasDefaultValue(0);
                entity.Property(p => p.IsActive).HasDefaultValue(true);
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(p => p.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Service configuration
            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
                entity.Property(s => s.Description).HasMaxLength(1000);
                entity.Property(s => s.Category).IsRequired().HasMaxLength(100);
                entity.Property(s => s.SubCategory).HasMaxLength(100);
                entity.Property(s => s.Price).HasColumnType("decimal(10,2)");
                entity.Property(s => s.DiscountedPrice).HasColumnType("decimal(10,2)");
                entity.Property(s => s.DurationInMinutes).IsRequired();
                entity.Property(s => s.ImageUrl).HasMaxLength(500);
                entity.Property(s => s.IsPopular).HasDefaultValue(false);
                entity.Property(s => s.IsActive).HasDefaultValue(true);
                entity.Property(s => s.DisplayOrder).HasDefaultValue(0);
                entity.Property(s => s.MaxBookingsPerSlot).HasDefaultValue(1);
                entity.Property(s => s.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(s => s.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Booking configuration - FIXED: Using Restrict instead of Cascade for User
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(b => b.Id);

                entity.HasIndex(b => b.BookingId).IsUnique();
                entity.Property(b => b.BookingId).IsRequired().HasMaxLength(50);
                entity.Property(b => b.VehicleType).IsRequired().HasMaxLength(50).HasDefaultValue("car");
                entity.Property(b => b.ACType).HasMaxLength(100);
                entity.Property(b => b.ACBrand).HasMaxLength(100);
                entity.Property(b => b.SofaType).HasMaxLength(100);
                entity.Property(b => b.ScheduledTime).IsRequired().HasMaxLength(20);
                entity.Property(b => b.Status).IsRequired().HasMaxLength(50).HasDefaultValue("pending");
                entity.Property(b => b.PaymentStatus).IsRequired().HasMaxLength(50).HasDefaultValue("pending");
                entity.Property(b => b.PaymentMethod).HasMaxLength(100);
                entity.Property(b => b.AppliedOfferCode).HasMaxLength(20);
                entity.Property(b => b.SpecialInstructions).HasMaxLength(500);

                entity.Property(b => b.Subtotal).HasColumnType("decimal(10,2)");
                entity.Property(b => b.DiscountAmount).HasColumnType("decimal(10,2)");
                entity.Property(b => b.TaxAmount).HasColumnType("decimal(10,2)");
                entity.Property(b => b.TotalAmount).HasColumnType("decimal(10,2)");

                entity.Property(b => b.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(b => b.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // CRITICAL FIX: Use Restrict for User to avoid multiple cascade paths
                entity.HasOne(b => b.User)
                    .WithMany(u => u.Bookings)
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Restrict); // Changed from Cascade

                entity.HasOne(b => b.Service)
                    .WithMany(s => s.Bookings)
                    .HasForeignKey(b => b.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Address)
                    .WithMany()
                    .HasForeignKey(b => b.AddressId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ServiceReview configuration
            modelBuilder.Entity<ServiceReview>(entity =>
            {
                entity.HasKey(sr => sr.Id);

                entity.HasIndex(sr => new { sr.UserId, sr.ServiceId });

                entity.Property(sr => sr.Rating).IsRequired();
                entity.Property(sr => sr.Comment).HasMaxLength(1000);
                entity.Property(sr => sr.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(sr => sr.User)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(sr => sr.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sr => sr.Service)
                    .WithMany(s => s.ServiceReviews)
                    .HasForeignKey(sr => sr.ServiceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Offer configuration
            modelBuilder.Entity<Offer>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.HasIndex(o => o.Code).IsUnique();

                entity.Property(o => o.Title).IsRequired().HasMaxLength(100);
                entity.Property(o => o.Description).HasMaxLength(500);
                entity.Property(o => o.Code).IsRequired().HasMaxLength(20);
                entity.Property(o => o.DiscountType).IsRequired().HasMaxLength(20);
                entity.Property(o => o.Category).HasMaxLength(50).HasDefaultValue("all");
                entity.Property(o => o.ApplicableServices).HasMaxLength(500).HasDefaultValue("[\"all\"]");
                entity.Property(o => o.Icon).HasMaxLength(100).HasDefaultValue("gift");
                entity.Property(o => o.ColorCode).HasMaxLength(50).HasDefaultValue("#2196F3");
                entity.Property(o => o.Terms).HasMaxLength(1000);

                entity.Property(o => o.DiscountValue).HasColumnType("decimal(10,2)");
                entity.Property(o => o.MinAmount).HasColumnType("decimal(10,2)");

                entity.Property(o => o.ValidFrom).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(o => o.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(o => o.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(o => o.IsActive).HasDefaultValue(true);
                entity.Property(o => o.UsageLimit).HasDefaultValue(1000);
                entity.Property(o => o.UsedCount).HasDefaultValue(0);
                entity.Property(o => o.IsOneTimePerUser).HasDefaultValue(false);
                entity.Property(o => o.IsDeleted).HasDefaultValue(false);
            });

            // OTP configuration
            modelBuilder.Entity<OTP>(entity =>
            {
                entity.ToTable("OTPs");

                entity.HasKey(o => o.Id);

                entity.HasIndex(o => new { o.MobileNumber, o.Type, o.CreatedAt });
                entity.HasIndex(o => new { o.Email, o.Type, o.CreatedAt });
                entity.HasIndex(o => o.ExpiresAt);

                entity.Property(o => o.Code).IsRequired().HasMaxLength(10);
                entity.Property(o => o.Type).IsRequired().HasMaxLength(20);
                entity.Property(o => o.Value).IsRequired().HasMaxLength(255);
                entity.Property(o => o.Email).HasMaxLength(255);
                entity.Property(o => o.MobileNumber).HasMaxLength(20);
                entity.Property(o => o.Flow).IsRequired().HasMaxLength(20);
                entity.Property(o => o.DeviceId).HasMaxLength(100);
                entity.Property(o => o.IpAddress).HasMaxLength(50);
                entity.Property(o => o.Purpose).HasMaxLength(50);

                entity.Property(o => o.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(o => o.ExpiresAt).HasDefaultValueSql("DATEADD(minute, 10, GETUTCDATE())");
                entity.Property(o => o.Attempts).HasDefaultValue(0);
                entity.Property(o => o.MaxAttempts).HasDefaultValue(3);
                entity.Property(o => o.IsUsed).HasDefaultValue(false);
                entity.Property(o => o.IsVerified).HasDefaultValue(false);
                entity.Property(o => o.IsActive).HasDefaultValue(true);

                entity.HasOne(o => o.User)
                    .WithMany(u => u.OTPs)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}