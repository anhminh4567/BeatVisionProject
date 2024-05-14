using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.IdentityConfiguration;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Repository
{
    public class ApplicationDbContext : IdentityDbContext<CustomIdentityUser,
        CustomIdentityRole,
        int,
        CustomIdentityUserClaims,
        CustomIdentityUserRole,
        CustomIdentityUserLogins,
        CustomIdentityRoleClaim,
        CustomIdentityUserToken>
    {
        public ApplicationDbContext() : base()
        {
        }

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            //var config = new ConfigurationBuilder()
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddJsonFile("appsettings.json", false, true)
            //    .Build();
            //var getConnectionString = config.GetConnectionString("DefaultConnectionString");
            optionsBuilder.UseSqlServer("server=(local);Uid=sa;Pwd=12345;Database=BeatVision;TrustServerCertificate=true");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //Security Configuration
            //Security Configuration
            builder.Entity<CustomIdentityUser>().ToTable("User");
            builder.Entity<CustomIdentityUserRole>().ToTable("UserRoles");
            builder.Entity<CustomIdentityRole>().ToTable("Role");
            builder.Entity<CustomIdentityRoleClaim>().ToTable("RoleClaims");
            builder.Entity<CustomIdentityUserClaims>().ToTable("UserClaims");
            builder.Entity<CustomIdentityUserLogins>().ToTable("UserLogins");
            builder.Entity<CustomIdentityUserToken>().ToTable("UserToken");

            //Application Configuration
            //Application Configuration
            builder.Entity<UserProfile>(e =>
            {
                e.HasOne(u => u.IdentityUser)
                .WithOne(u => u.UserProfile)
                .HasForeignKey<UserProfile>(u => u.IdentityId)
                .OnDelete(DeleteBehavior.Cascade);
            });
        }

    }
}
