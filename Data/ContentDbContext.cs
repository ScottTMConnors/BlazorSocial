using BlazorSocial.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorSocial.Data {
    public class ContentDbContext : DbContext {

        public ContentDbContext(DbContextOptions<ContentDbContext> options) : base(options) {

        }




        public DbSet<Post> Posts { get; set; }
        public DbSet<PostType> PostTypes { get; set; }
        public DbSet<PostGroup> PostGroups { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<View> Views { get; set; }
        public DbSet<PostMetadata> PostMetadatas { get; set; }

    }
}
