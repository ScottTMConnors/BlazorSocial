﻿// <auto-generated />
using System;
using BlazorSocial.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BlazorSocial.Migrations.ContentDb
{
    [DbContext(typeof(ContentDbContext))]
    [Migration("20240109022335_Socialmigrations")]
    partial class Socialmigrations
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BlazorSocial.Data.Entities.AnonView", b =>
                {
                    b.Property<string>("PostId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("IPAddress")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("TimesViewed")
                        .HasColumnType("int");

                    b.Property<DateTime>("ViewDate")
                        .HasColumnType("datetime2");

                    b.HasKey("PostId", "IPAddress");

                    b.ToTable("AnonViews");
                });

            modelBuilder.Entity("BlazorSocial.Data.Entities.Comment", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AuthorID")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<DateTime>("PostDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("PostId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("AuthorID");

                    b.HasIndex("PostId");

                    b.ToTable("Comments");
                });

            modelBuilder.Entity("BlazorSocial.Data.Entities.Group", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("GroupName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("BlazorSocial.Data.Entities.Post", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AuthorID")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Content")
                        .HasMaxLength(3999)
                        .HasColumnType("nvarchar(3999)");

                    b.Property<DateTime?>("PostDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("PostTypeID")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("AuthorID");

                    b.HasIndex("PostTypeID");

                    b.ToTable("Posts");
                });

            modelBuilder.Entity("BlazorSocial.Data.Entities.PostGroup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("GroupId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("PostId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.HasIndex("PostId");

                    b.ToTable("PostGroups");
                });

            modelBuilder.Entity("BlazorSocial.Data.Entities.PostMetadata", b =>
                {
                    b.Property<string>("PostId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int?>("AnonViewCount")
                        .HasColumnType("int");

                    b.Property<int?>("Downvotes")
                        .HasColumnType("int");

                    b.Property<int?>("NetVotes")
                        .HasColumnType("int");

                    b.Property<int?>("TotalVotes")
                        .HasColumnType("int");

                    b.Property<int?>("Upvotes")
                        .HasColumnType("int");

                    b.Property<int?>("ViewCount")
                        .HasColumnType("int");

                    b.HasKey("PostId");

                    b.ToTable("PostMetadatas");
                });

            modelBuilder.Entity("BlazorSocial.Data.Entities.PostType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("TypeName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("PostTypes");
                });

            modelBuilder.Entity("BlazorSocial.Data.Entities.SocialUser", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("NormalizedUserName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.HasKey("UserId");

                    b.ToTable("SocialUsers");
                });

            modelBuilder.Entity("BlazorSocial.Data.Entities.View", b =>
                {
                    b.Property<string>("PostId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("TimesViewed")
                        .HasColumnType("int");

                    b.Property<DateTime>("ViewDate")
                        .HasColumnType("datetime2");

                    b.HasKey("PostId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("Views");
                });

            modelBuilder.Entity("BlazorSocial.Data.Entities.Vote", b =>
                {
                    b.Property<string>("PostId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool>("IsUpvote")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("VoteDate")
                        .HasColumnType("datetime2");

                    b.HasKey("PostId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("Votes");
                });

            modelBuilder.Entity("BlazorSocial.Data.Entities.AnonView", b =>
                {
                    b.HasOne("BlazorSocial.Data.Entities.Post", "Post")
                        .WithMany()
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Post");
                });

            modelBuilder.Entity("BlazorSocial.Data.Entities.Comment", b =>
                {
                    b.HasOne("BlazorSocial.Data.Entities.SocialUser", "Author")
                        .WithMany()
                        .HasForeignKey("AuthorID");

                    b.HasOne("BlazorSocial.Data.Entities.Post", "Post")
                        .WithMany()
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Author");

                    b.Navigation("Post");
                });

            modelBuilder.Entity("BlazorSocial.Data.Entities.Post", b =>
                {
                    b.HasOne("BlazorSocial.Data.Entities.SocialUser", "Author")
                        .WithMany()
                        .HasForeignKey("AuthorID");

                    b.HasOne("BlazorSocial.Data.Entities.PostType", "PostType")
                        .WithMany()
                        .HasForeignKey("PostTypeID");

                    b.Navigation("Author");

                    b.Navigation("PostType");
                });

            modelBuilder.Entity("BlazorSocial.Data.Entities.PostGroup", b =>
                {
                    b.HasOne("BlazorSocial.Data.Entities.Group", "Group")
                        .WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BlazorSocial.Data.Entities.Post", "Post")
                        .WithMany()
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("Post");
                });

            modelBuilder.Entity("BlazorSocial.Data.Entities.PostMetadata", b =>
                {
                    b.HasOne("BlazorSocial.Data.Entities.Post", "Post")
                        .WithOne("PostMetadata")
                        .HasForeignKey("BlazorSocial.Data.Entities.PostMetadata", "PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Post");
                });

            modelBuilder.Entity("BlazorSocial.Data.Entities.View", b =>
                {
                    b.HasOne("BlazorSocial.Data.Entities.Post", "Post")
                        .WithMany()
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BlazorSocial.Data.Entities.SocialUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Post");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BlazorSocial.Data.Entities.Vote", b =>
                {
                    b.HasOne("BlazorSocial.Data.Entities.Post", "Post")
                        .WithMany()
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BlazorSocial.Data.Entities.SocialUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Post");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BlazorSocial.Data.Entities.Post", b =>
                {
                    b.Navigation("PostMetadata");
                });
#pragma warning restore 612, 618
        }
    }
}
