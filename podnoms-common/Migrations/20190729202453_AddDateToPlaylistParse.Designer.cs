﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PodNoms.Common.Persistence;

namespace PodNoms.Comon.Migrations
{
    [DbContext(typeof(PodNomsDbContext))]
    [Migration("20190729202453_AddDateToPlaylistParse")]
    partial class AddDateToPlaylistParse
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("PodNoms.Data.Models.AccountSubscription", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("Amount");

                    b.Property<string>("AppUserId");

                    b.Property<DateTime>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("getdate()");

                    b.Property<DateTime>("EndDate");

                    b.Property<string>("ReceiptURL");

                    b.Property<DateTime>("StartDate");

                    b.Property<string>("TransactionId");

                    b.Property<int>("Type");

                    b.Property<DateTime>("UpdateDate")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasDefaultValueSql("getdate()");

                    b.Property<bool>("WasSuccessful");

                    b.HasKey("Id");

                    b.HasIndex("AppUserId");

                    b.ToTable("AccountSubscriptions");
                });

            modelBuilder.Entity("PodNoms.Data.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("City");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("CountryCode");

                    b.Property<string>("CountryName");

                    b.Property<long?>("DiskQuota");

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<int>("EmailNotificationOptions");

                    b.Property<long?>("FacebookId");

                    b.Property<string>("FirstName");

                    b.Property<string>("IpAddress");

                    b.Property<bool>("IsAdmin")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<string>("LastName");

                    b.Property<DateTime>("LastSeen")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("getdate()");

                    b.Property<double?>("Latitude");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<double?>("Longitude");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("PictureUrl");

                    b.Property<string>("RegionCode");

                    b.Property<string>("RegionName");

                    b.Property<string>("SecurityStamp");

                    b.Property<string>("Slug");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.Property<string>("Zip");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.HasIndex("Slug")
                        .IsUnique()
                        .HasFilter("[Slug] IS NOT NULL");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("PodNoms.Data.Models.ApplicationUserSlugRedirects", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ApplicationUserId");

                    b.Property<DateTime>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("getdate()");

                    b.Property<string>("OldSlug");

                    b.Property<DateTime>("UpdateDate")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasDefaultValueSql("getdate()");

                    b.HasKey("Id");

                    b.HasIndex("ApplicationUserId");

                    b.ToTable("ApplicationUserSlugRedirects");
                });

            modelBuilder.Entity("PodNoms.Data.Models.Category", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("getdate()");

                    b.Property<string>("Description");

                    b.Property<DateTime>("UpdateDate")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasDefaultValueSql("getdate()");

                    b.HasKey("Id");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("PodNoms.Data.Models.ChatMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("getdate()");

                    b.Property<string>("FromUserId");

                    b.Property<string>("Message");

                    b.Property<DateTime?>("MessageSeen");

                    b.Property<string>("ToUserId");

                    b.Property<DateTime>("UpdateDate")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasDefaultValueSql("getdate()");

                    b.HasKey("Id");

                    b.HasIndex("FromUserId");

                    b.HasIndex("ToUserId");

                    b.ToTable("ChatMessages");
                });

            modelBuilder.Entity("PodNoms.Data.Models.Donation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("Amount");

                    b.Property<string>("AppUserId");

                    b.Property<DateTime>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("getdate()");

                    b.Property<DateTime>("UpdateDate")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasDefaultValueSql("getdate()");

                    b.HasKey("Id");

                    b.HasIndex("AppUserId");

                    b.ToTable("Donations");
                });

            modelBuilder.Entity("PodNoms.Data.Models.Notifications.Notification", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Config");

                    b.Property<DateTime>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("getdate()");

                    b.Property<Guid>("PodcastId");

                    b.Property<int>("Type");

                    b.Property<DateTime>("UpdateDate")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasDefaultValueSql("getdate()");

                    b.HasKey("Id");

                    b.HasIndex("PodcastId");

                    b.ToTable("Notifications");
                });

            modelBuilder.Entity("PodNoms.Data.Models.Notifications.NotificationLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("getdate()");

                    b.Property<string>("Log");

                    b.Property<Guid>("NotificationId");

                    b.Property<DateTime>("UpdateDate")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasDefaultValueSql("getdate()");

                    b.HasKey("Id");

                    b.HasIndex("NotificationId");

                    b.ToTable("NotificationLogs");
                });

            modelBuilder.Entity("PodNoms.Data.Models.ParsedPlaylistItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("getdate()");

                    b.Property<bool>("IsProcessed");

                    b.Property<Guid>("PlaylistId");

                    b.Property<DateTime>("PlaylistItemDate");

                    b.Property<DateTime>("UpdateDate")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasDefaultValueSql("getdate()");

                    b.Property<string>("VideoId");

                    b.Property<string>("VideoType");

                    b.HasKey("Id");

                    b.HasIndex("PlaylistId");

                    b.HasIndex("VideoId", "PlaylistId")
                        .IsUnique()
                        .HasFilter("[VideoId] IS NOT NULL");

                    b.ToTable("ParsedPlaylistItems");
                });

            modelBuilder.Entity("PodNoms.Data.Models.Playlist", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("getdate()");

                    b.Property<Guid>("PodcastId");

                    b.Property<string>("SourceUrl");

                    b.Property<DateTime>("UpdateDate")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasDefaultValueSql("getdate()");

                    b.HasKey("Id");

                    b.HasIndex("PodcastId");

                    b.HasIndex("SourceUrl")
                        .IsUnique()
                        .HasFilter("[SourceUrl] IS NOT NULL");

                    b.ToTable("Playlists");
                });

            modelBuilder.Entity("PodNoms.Data.Models.Podcast", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AppUserId")
                        .IsRequired();

                    b.Property<byte[]>("AuthPassword");

                    b.Property<byte[]>("AuthPasswordSalt");

                    b.Property<string>("AuthUserName");

                    b.Property<Guid?>("CategoryId");

                    b.Property<DateTime>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("getdate()");

                    b.Property<string>("CustomDomain");

                    b.Property<string>("Description");

                    b.Property<bool?>("Private")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<string>("Slug");

                    b.Property<string>("Title");

                    b.Property<DateTime>("UpdateDate")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasDefaultValueSql("getdate()");

                    b.HasKey("Id");

                    b.HasIndex("AppUserId");

                    b.HasIndex("CategoryId");

                    b.HasIndex("Slug")
                        .IsUnique()
                        .HasFilter("[Slug] IS NOT NULL");

                    b.ToTable("Podcasts");
                });

            modelBuilder.Entity("PodNoms.Data.Models.PodcastEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("AudioFileSize");

                    b.Property<float>("AudioLength");

                    b.Property<string>("AudioUrl");

                    b.Property<string>("Author");

                    b.Property<DateTime>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("getdate()");

                    b.Property<string>("Description");

                    b.Property<string>("ImageUrl");

                    b.Property<Guid?>("PlaylistId");

                    b.Property<Guid>("PodcastId");

                    b.Property<bool>("Processed");

                    b.Property<string>("ProcessingPayload");

                    b.Property<int>("ProcessingStatus");

                    b.Property<int>("ShareOptions");

                    b.Property<string>("SourceUrl");

                    b.Property<string>("Title");

                    b.Property<DateTime>("UpdateDate")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasDefaultValueSql("getdate()");

                    b.HasKey("Id");

                    b.HasIndex("PlaylistId");

                    b.HasIndex("PodcastId");

                    b.ToTable("PodcastEntries");
                });

            modelBuilder.Entity("PodNoms.Data.Models.PodcastEntrySharingLink", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("getdate()");

                    b.Property<string>("LinkId");

                    b.Property<int>("LinkIndex")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<Guid?>("PodcastEntryId");

                    b.Property<DateTime>("UpdateDate")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasDefaultValueSql("getdate()");

                    b.Property<DateTime?>("ValidFrom");

                    b.Property<DateTime?>("ValidTo");

                    b.HasKey("Id");

                    b.HasIndex("LinkId")
                        .IsUnique()
                        .HasFilter("[LinkId] IS NOT NULL");

                    b.HasIndex("LinkIndex")
                        .IsUnique();

                    b.HasIndex("PodcastEntryId");

                    b.ToTable("PodcastEntrySharingLinks");
                });

            modelBuilder.Entity("PodNoms.Data.Models.ServerConfig", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("getdate()");

                    b.Property<string>("Key");

                    b.Property<DateTime>("UpdateDate")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasDefaultValueSql("getdate()");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.ToTable("ServerConfig","admin");
                });

            modelBuilder.Entity("PodNoms.Data.Models.Subcategory", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("CategoryId");

                    b.Property<DateTime>("CreateDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("getdate()");

                    b.Property<string>("Description");

                    b.Property<Guid?>("PodcastId");

                    b.Property<DateTime>("UpdateDate")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasDefaultValueSql("getdate()");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("PodcastId");

                    b.ToTable("Subcategories");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("PodNoms.Data.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("PodNoms.Data.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PodNoms.Data.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("PodNoms.Data.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PodNoms.Data.Models.AccountSubscription", b =>
                {
                    b.HasOne("PodNoms.Data.Models.ApplicationUser", "AppUser")
                        .WithMany("AccountSubscriptions")
                        .HasForeignKey("AppUserId");
                });

            modelBuilder.Entity("PodNoms.Data.Models.ApplicationUserSlugRedirects", b =>
                {
                    b.HasOne("PodNoms.Data.Models.ApplicationUser", "ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ApplicationUserId");
                });

            modelBuilder.Entity("PodNoms.Data.Models.ChatMessage", b =>
                {
                    b.HasOne("PodNoms.Data.Models.ApplicationUser", "FromUser")
                        .WithMany()
                        .HasForeignKey("FromUserId");

                    b.HasOne("PodNoms.Data.Models.ApplicationUser", "ToUser")
                        .WithMany()
                        .HasForeignKey("ToUserId");
                });

            modelBuilder.Entity("PodNoms.Data.Models.Donation", b =>
                {
                    b.HasOne("PodNoms.Data.Models.ApplicationUser", "AppUser")
                        .WithMany("Donations")
                        .HasForeignKey("AppUserId");
                });

            modelBuilder.Entity("PodNoms.Data.Models.Notifications.Notification", b =>
                {
                    b.HasOne("PodNoms.Data.Models.Podcast", "Podcast")
                        .WithMany("Notifications")
                        .HasForeignKey("PodcastId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PodNoms.Data.Models.Notifications.NotificationLog", b =>
                {
                    b.HasOne("PodNoms.Data.Models.Notifications.Notification", "Notification")
                        .WithMany()
                        .HasForeignKey("NotificationId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PodNoms.Data.Models.ParsedPlaylistItem", b =>
                {
                    b.HasOne("PodNoms.Data.Models.Playlist", "Playlist")
                        .WithMany("ParsedPlaylistItems")
                        .HasForeignKey("PlaylistId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PodNoms.Data.Models.Playlist", b =>
                {
                    b.HasOne("PodNoms.Data.Models.Podcast", "Podcast")
                        .WithMany()
                        .HasForeignKey("PodcastId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PodNoms.Data.Models.Podcast", b =>
                {
                    b.HasOne("PodNoms.Data.Models.ApplicationUser", "AppUser")
                        .WithMany("Podcasts")
                        .HasForeignKey("AppUserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PodNoms.Data.Models.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId");
                });

            modelBuilder.Entity("PodNoms.Data.Models.PodcastEntry", b =>
                {
                    b.HasOne("PodNoms.Data.Models.Playlist")
                        .WithMany("PodcastEntries")
                        .HasForeignKey("PlaylistId");

                    b.HasOne("PodNoms.Data.Models.Podcast", "Podcast")
                        .WithMany("PodcastEntries")
                        .HasForeignKey("PodcastId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PodNoms.Data.Models.PodcastEntrySharingLink", b =>
                {
                    b.HasOne("PodNoms.Data.Models.PodcastEntry", "PodcastEntry")
                        .WithMany("SharingLinks")
                        .HasForeignKey("PodcastEntryId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PodNoms.Data.Models.Subcategory", b =>
                {
                    b.HasOne("PodNoms.Data.Models.Category", "Category")
                        .WithMany("Subcategories")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PodNoms.Data.Models.Podcast")
                        .WithMany("Subcategories")
                        .HasForeignKey("PodcastId");
                });
#pragma warning restore 612, 618
        }
    }
}
