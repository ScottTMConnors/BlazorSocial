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
    public DbSet<PostType> PostTypes { get; set; }
    public DbSet<PostGroup> PostGroups { get; set; }
    public DbSet<Vote> Votes { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<View> Views { get; set; }
    public DbSet<PostMetadata> PostMetadatas { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<UserId>().HaveConversion<UniqueIdConverter<UserId>>();
        configurationBuilder.Properties<PostId>().HaveConversion<UniqueIdConverter<PostId>>();
        configurationBuilder.Properties<CommentId>().HaveConversion<UniqueIdConverter<CommentId>>();
        configurationBuilder.Properties<GroupId>().HaveConversion<UniqueIdConverter<GroupId>>();
    }
}