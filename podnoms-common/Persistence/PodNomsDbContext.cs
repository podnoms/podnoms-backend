﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EntitySignal.Server.EFDbContext.Data;
using EntitySignal.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PodNoms.Common.Services.Caching;
using PodNoms.Data.Enums;
using PodNoms.Data.Configuration;
using PodNoms.Data.Extensions;
using PodNoms.Data.Interfaces;
using PodNoms.Data.Models;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Common.Persistence {
    public class PodNomsDbContextFactory : IDesignTimeDbContextFactory<PodNomsDbContext> {
        public PodNomsDbContext CreateDbContext(string[] args) {
            var TEMP_CONN =
                "Server=tcp:127.0.0.1,1433;Initial Catalog=PodNoms;Persist Security Info=False;User ID=podnomsweb;Password=podnomsweb;MultipleActiveResultSets=False;TrustServerCertificate=False;Connection Timeout=30;";
            var builder = new DbContextOptionsBuilder<PodNomsDbContext>();

            var connectionString = TEMP_CONN;
            builder.UseSqlServer(connectionString);

            return new PodNomsDbContext(builder.Options, null, null);
        }
    }

    public sealed class PodNomsDbContext : EntitySignalIdentityDbContext<ApplicationUser> {
        private readonly IResponseCacheService _cache;

        public PodNomsDbContext(
            DbContextOptions<PodNomsDbContext> options,
            EntitySignalDataProcess entitySignalDataProcess,
            IResponseCacheService cache) : base(options, entitySignalDataProcess) {
            Database.SetCommandTimeout(360);
            _cache = cache;
        }

        private IEnumerable<PropertyBuilder> __getColumn(ModelBuilder modelBuilder, string columnName) {
            return modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.Name == columnName)
                .Select(p => modelBuilder.Entity(p.DeclaringEntityType.ClrType).Property(p.Name));
        }

        public void ConfigureUser(EntityTypeBuilder<ApplicationUser> builder) {
            var navigation = builder.Metadata.FindNavigation(nameof(ApplicationUser.RefreshTokens));
            //EF access the RefreshTokens collection property through its backing field
            navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
            // builder.Ignore(b => b.Email);
            // builder.Ignore(b => b.PasswordHash);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new CategoryConfiguration());
            modelBuilder.ApplyConfiguration(new SubcategoryConfiguration());

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(p => p.Slug)
                .IsUnique(true);
            modelBuilder.Entity<ApplicationUser>()
                .Property(e => e.LastSeen)
                .HasDefaultValueSql("getdate()");

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(r => r.PatreonToken)
                .WithOne(a => a.AppUser)
                .HasForeignKey<PatreonToken>(t => t.AppUserId);

            modelBuilder.Entity<ApplicationUser>(ConfigureUser);

            modelBuilder.Entity<Podcast>()
                .HasIndex(p => p.Slug)
                .IsUnique(true);
            modelBuilder.Entity<Podcast>()
                .Property(p => p.AppUserId)
                .IsRequired();
            modelBuilder.Entity<Podcast>()
                .Property(p => p.Private)
                .IsRequired()
                .HasDefaultValue(false);

            modelBuilder.Entity<PodcastEntry>()
                .HasMany(e => e.SharingLinks)
                .WithOne(e => e.PodcastEntry)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PodcastEntry>()
                .HasMany(e => e.ActivityLogs)
                .WithOne(e => e.PodcastEntry)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PodcastEntry>()
                .Property(p => p.WaveformGenerated)
                .IsRequired()
                .HasDefaultValue(false);

            modelBuilder.Entity<EntryTag>()
                .HasIndex(b => b.TagName)
                .IsUnique();

            modelBuilder.Entity<PodcastEntrySharingLink>()
                .HasIndex(l => l.LinkIndex)
                .IsUnique();
            modelBuilder.Entity<PodcastEntrySharingLink>()
                .HasIndex(l => l.LinkId)
                .IsUnique();

            modelBuilder.Entity<Playlist>()
                .HasIndex(p => new {p.SourceUrl})
                .IsUnique(true);

            modelBuilder.Entity<BoilerPlate>()
                .HasIndex(p => new {p.Key})
                .IsUnique(true);

            var converter = new EnumToNumberConverter<NotificationOptions, int>();
            modelBuilder.Entity<ApplicationUser>()
                .Property(e => e.EmailNotificationOptions)
                .HasConversion(converter);

            foreach (var pb in __getColumn(modelBuilder, "CreateDate")) {
                pb.ValueGeneratedOnAdd()
                    .HasDefaultValueSql("getdate()");
            }

            foreach (var pb in __getColumn(modelBuilder, "UpdateDate")) {
                pb.ValueGeneratedOnAddOrUpdate()
                    .HasDefaultValueSql("getdate()");
            }
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default) {
            foreach (var entity in ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Where(e => e.Entity is ISluggedEntity)
                .Select(e => e.Entity as ISluggedEntity)
                .Where(e => string.IsNullOrEmpty(e.Slug))) {
                entity.Slug = entity.GenerateSlug(this);
            }

            //remove all caches referencing this item
            foreach (ICachedEntity entity in ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Where(e => e.Entity is ICachedEntity)
                .Select(e => e as ICachedEntity)) {
                foreach (CacheType type in Enum.GetValues(typeof(CacheType))) {
                    try {
                        if (entity != null) {
                            //entity could not be cast as above
                            var key = entity.GetCacheKey(type);
                            if (!string.IsNullOrEmpty(key)) {
                                _cache.InvalidateCacheResponseAsync(
                                    entity.GetCacheKey(type)
                                );
                            }
                        }
                    } catch (Exception) {
                        //hasn't been cached
                    }
                }
            }

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public DbSet<BoilerPlate> BoilerPlates { get; set; }
        public DbSet<AccountSubscription> AccountSubscriptions { get; set; }
        public DbSet<ApplicationUserSlugRedirects> ApplicationUserSlugRedirects { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PatreonToken> PatreonTokens { get; set; }

        public DbSet<EntryTag> EntryTags { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<NotificationLog> NotificationLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<PodcastEntry> PodcastEntries { get; set; }
        public DbSet<EntryComment> EntryComments { get; set; }
        public DbSet<ActivityLogPodcastEntry> ActivityLogPodcastEntry { get; set; }
        public DbSet<PodcastEntrySharingLink> PodcastEntrySharingLinks { get; set; }
        public DbSet<SiteMessages> SiteMessages { get; set; }
        public DbSet<UserRequest> UserRequests { get; set; }

        public DbSet<Podcast> Podcasts { get; set; }
        public DbSet<PodcastAggregator> PodcastAggregators { get; set; }
        public DbSet<ServerConfig> ServerConfig { get; set; }
        public DbSet<Subcategory> Subcategories { get; set; }
        public DbSet<IssuedApiKey> IssuedApiKeys { get; set; }

        public DbSet<ServiceApiKey> ServiceApiKeys { get; set; }
    }
}
