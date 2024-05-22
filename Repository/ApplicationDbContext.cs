﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Enums;
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
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Message> Messages { get; set; }

        public DbSet<Comment> Comments { get; set; }
        public DbSet<TrackComment> TrackComments { get; set; }        
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<TrackLicense> TrackLicenses { get;set;}
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
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
			
			builder.Entity<UserProfile>(entity =>
            {
				entity.HasOne(u => u.IdentityUser)
                .WithOne(u => u.UserProfile)
                .HasForeignKey<UserProfile>(u => u.IdentityId)
                .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.CartItems)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId);
                //entity.HasMany(e => e.CreatedMessage)
                //.WithMany(m => m.Receivers)
                //.UsingEntity<Notification>(noti =>
                //{
                //    noti.HasOne(n => n.Sender).WithMany(s => s.Notifications);
                //    noti.HasOne(n => n.Message).WithMany(m => m.Notifications);
                //});
                //entity.HasMany(e => e.Comments)
                //.WithOne(c => c.Author)
                //.HasForeignKey(c => c.AuthorId);
                //entity
                //.HasOne(u => u.BannerBlobFile)
                //.WithOne()
                //.HasForeignKey<UserProfile>(u => u.BannerBlobId);
			});
			builder.Entity<Message>(entity =>
			{
				entity.HasOne(m => m.Creator)
				.WithMany(u => u.CreatedMessage).HasForeignKey(m => m.CreatorId);
			});
            builder.Entity<Notification>(entity => 
            {
                entity.HasOne(n => n.Receiver)
                .WithMany(u => u.Notifications).HasForeignKey(n => n.ReceiverId).OnDelete(DeleteBehavior.NoAction) ;
				entity.HasOne(n => n.Message)
				.WithMany(u => u.Notifications).HasForeignKey(n => n.MessageId);
                entity.HasKey( nameof(Notification.MessageId), nameof(Notification.ReceiverId));

			});
			builder.Entity<Track>(entity =>
            {
                entity.HasMany(t => t.Licenses)
                .WithMany(l => l.TracksRelated);
                entity.HasMany(t => t.Tags)
                .WithMany(t => t.Tracks);
				
                entity.HasOne(t => t.Owner)
                .WithMany(u => u.OwnedTracks).HasForeignKey(t => t.OwnerId).OnDelete(DeleteBehavior.Cascade);
				//entity
				//.HasOne(u => u.BannerBlobFile)
				//.WithOne()
				//.HasForeignKey<Track>(u => u.BannerBlobId);
				entity
				.HasOne(u => u.AudioFile)
				.WithOne()
				.HasForeignKey<Track>(u => u.AudioBlobId);
			});
            
            builder.Entity<Comment>(entity =>
            {
                entity//.UseTphMappingStrategy()
               .HasDiscriminator(c => c.CommentType)
               .HasValue<TrackComment>(CommentType.TRACK);
				entity.HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorId);
                entity.HasOne(c => c.ReplyToComment)
                .WithMany().HasForeignKey(c => c.ReplyToCommentId).OnDelete(DeleteBehavior.NoAction);
            });
            builder.Entity<TrackComment>(entity =>
            {
                entity.HasOne(tc => tc.Track)
                .WithMany(t => t.Comments)
                .HasForeignKey(tc => tc.TrackId).OnDelete(DeleteBehavior.Cascade);
            });


            builder.Entity<BlobFileData>(entity =>
            {

            });
            builder.Entity<Coupon>(entity => { 
            
            });
        }

    }
}
