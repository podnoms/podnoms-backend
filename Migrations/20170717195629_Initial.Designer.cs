﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using PodNoms.Api.Models;
using PodNoms.Api.Persistence;
using System;

namespace PodNoms.Api.Migrations
{
    [DbContext(typeof(PodNomsDbContext))]
    [Migration("20170717195629_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.0-preview2-25794")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("PodNoms.Api.Models.Podcast", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreateDate");

                    b.Property<string>("Description");

                    b.Property<string>("ImageUrl");

                    b.Property<string>("Slug");

                    b.Property<string>("Title");

                    b.Property<DateTime>("UpdateDate");

                    b.Property<int?>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Podcasts");
                });

            modelBuilder.Entity("PodNoms.Api.Models.PodcastEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("AudioFileSize");

                    b.Property<float>("AudioLength");

                    b.Property<string>("AudioUrl");

                    b.Property<string>("Author");

                    b.Property<DateTime>("CreateDate");

                    b.Property<string>("Description");

                    b.Property<string>("ImageUrl");

                    b.Property<int>("PodcastId");

                    b.Property<bool>("Processed");

                    b.Property<string>("ProcessingPayload");

                    b.Property<int>("ProcessingStatus");

                    b.Property<string>("Slug");

                    b.Property<string>("SourceUrl");

                    b.Property<string>("Title");

                    b.Property<string>("Uid");

                    b.Property<DateTime>("UpdateDate");

                    b.HasKey("Id");

                    b.HasIndex("PodcastId");

                    b.ToTable("PodcastEntries");
                });

            modelBuilder.Entity("PodNoms.Api.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ApiKey");

                    b.Property<DateTime>("CreateDate");

                    b.Property<string>("EmailAddress");

                    b.Property<string>("FullName");

                    b.Property<string>("ProfileImage");

                    b.Property<string>("ProviderId");

                    b.Property<string>("Sid");

                    b.Property<DateTime>("UpdateDate");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("PodNoms.Api.Models.Podcast", b =>
                {
                    b.HasOne("PodNoms.Api.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("PodNoms.Api.Models.PodcastEntry", b =>
                {
                    b.HasOne("PodNoms.Api.Models.Podcast", "Podcast")
                        .WithMany("PodcastEntries")
                        .HasForeignKey("PodcastId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
