using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Extensions;
using PodNoms.Data.Interfaces;
using PodNoms.Data.Models;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Common.Persistence {


    public class PodNomsDbContext : IdentityDbContext<ApplicationUser> {
        private readonly ILogger<PodNomsDbContext> _logger;

        public PodNomsDbContext(DbContextOptions<PodNomsDbContext> options, ILogger<PodNomsDbContext> logger) : base(options) {
            Database.SetCommandTimeout(360);
            _logger = logger;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(p => p.Slug)
                .IsUnique(true);

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

            modelBuilder.Entity<ParsedPlaylistItem>()
                .HasIndex(p => new { p.VideoId, p.PlaylistId })
                .IsUnique(true);

            foreach (var pb in __getColumn(modelBuilder, "CreateDate")) {
                pb.ValueGeneratedOnAdd()
                  .HasDefaultValueSql("getdate()");
            }
            foreach (var pb in __getColumn(modelBuilder, "UpdateDate")) {
                pb.ValueGeneratedOnAddOrUpdate()
                  .HasDefaultValueSql("getdate()");
            }
        }
        private IEnumerable<PropertyBuilder> __getColumn(ModelBuilder modelBuilder, string columnName) {
            return modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.Name == columnName)
                .Select(p => modelBuilder.Entity(p.DeclaringEntityType.ClrType).Property(p.Name));
        }

        public override int SaveChanges() {
            foreach (var entity in ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                    .Where(e => e.Entity is ISluggedEntity)
                    .Select(e => e.Entity as ISluggedEntity)
                    .Where(e => string.IsNullOrEmpty(e.Slug))) {
                entity.Slug = entity.GetSlug(this, _logger);
            }
            return base.SaveChanges();
        }
        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken)) {
            foreach (var entity in ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                    .Where(e => e.Entity is ISluggedEntity)
                    .Select(e => e.Entity as ISluggedEntity)
                    .Where(e => string.IsNullOrEmpty(e.Slug))) {
                entity.Slug = entity.GetSlug(this, _logger);
            }
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        public DbSet<Podcast> Podcasts { get; set; }
        public DbSet<PodcastEntry> PodcastEntries { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Subcategory> Subcategories { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<ParsedPlaylistItem> ParsedPlaylistItems { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ServerConfig> ServerConfig { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }
}
