using BlazorSocial.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorSocial.Data;

public class ContentDbContext(DbContextOptions<ContentDbContext> options)
    : IdentityDbContext<SocialUser, IdentityRole<UserId>, UserId>(options)
{
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<PostGroup> PostGroups { get; set; }
    public DbSet<Vote> Votes { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<View> Views { get; set; }
    public DbSet<PostMetadata> PostMetadatas { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Post>(entity =>
        {
            entity.HasIndex(p => p.PostDate)
                .IsDescending()
                .HasDatabaseName("IX_Posts_PostDate_Desc");
        });

        builder.Entity<Vote>(entity =>
        {
            entity.HasOne(v => v.Post)
                .WithMany()
                .HasForeignKey(v => v.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.ClientCascade);

            entity.HasIndex(v => new { v.PostId, v.IsActive })
                .HasDatabaseName("IX_Votes_PostId_IsActive");
        });

        builder.Entity<View>(entity =>
        {
            entity.HasOne(v => v.Post)
                .WithMany()
                .HasForeignKey(v => v.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.ClientCascade);
        });

        builder.Entity<Comment>(entity =>
        {
            entity.HasOne(c => c.Post)
                .WithMany()
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Author)
                .WithMany()
                .HasForeignKey(c => c.AuthorID)
                .OnDelete(DeleteBehavior.ClientCascade);

            entity.HasIndex(c => new { c.PostId, c.PostDate })
                .IsDescending(false, true)
                .HasDatabaseName("IX_Comments_PostId_PostDate_Desc");
        });
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<PostType>().HaveConversion<SmartEnumConverter<PostType>>();
        configurationBuilder.Properties<UserId>().HaveConversion<UniqueIdConverter<UserId>>();
        configurationBuilder.Properties<PostId>().HaveConversion<UniqueIdConverter<PostId>>();
        configurationBuilder.Properties<CommentId>().HaveConversion<UniqueIdConverter<CommentId>>();
        configurationBuilder.Properties<GroupId>().HaveConversion<UniqueIdConverter<GroupId>>();
        configurationBuilder.Properties<ViewId>().HaveConversion<UniqueIdConverter<ViewId>>();
    }
}