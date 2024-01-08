using BlazorSocial.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorSocial.Data {
    public class ContentDbContext : DbContext {

        public ContentDbContext(DbContextOptions<ContentDbContext> options) : base(options) {

        }




        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<PostType> PostTypes { get; set; }
        public DbSet<PostGroup> PostGroups { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<View> Views { get; set; }
        public DbSet<AnonView> AnonViews { get; set; }
        public DbSet<PostMetadata> PostMetadatas { get; set; }
        public DbSet<SocialUser> SocialUsers { get; set; }


        //protected override void OnModelCreating(ModelBuilder modelBuilder) {
        //    modelBuilder.Entity<Post>()
        //        .HasOne(p => p.PostMetadata)
        //        .WithOne(pm => pm.Post)
        //        .HasForeignKey<PostMetadata>(pm => pm.PostId);
        //}

    }
}
