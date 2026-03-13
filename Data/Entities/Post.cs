using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlazorSocial.Shared.Models;

namespace BlazorSocial.Data.Entities;

public class Post : BaseEntity<PostId>
{
    public Post(string title, string content, UserId authorId, DateTime postDate, PostType postType)
    {
        Title = title;
        Content = content;
        AuthorId = authorId;
        PostDate = postDate;
        PostType = postType;
        PostMetadata = new PostMetadata(Id);
    }

    [StringLength(100)] public string Title { get; set; }

    [StringLength(3999)] public string Content { get; set; }

    [ForeignKey(nameof(Author))] public UserId AuthorId { get; set; }

    public DateTime PostDate { get; set; }

    //public PostType? PostType { get; set; }
    public SocialUser? Author { get; set; }
    public PostMetadata PostMetadata { get; set; }

    public PostType PostType { get; set; }

    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}

public static class PostExtensions
{
    extension(IQueryable<Post> query)
    {
        public IQueryable<ViewPostDto> ToViewPostDtos(UserId? currentUserId = null) =>
            query.Select(post => new ViewPostDto
            {
                PostId = post.Id.Value,
                Title = post.Title,
                Content = post.Content,
                PostDate = post.PostDate,
                PostType = post.PostType.ToString(),
                AuthorName = post.Author != null ? post.Author!.UserName ?? "Unknown" : "Unknown",
                CommentCount = post.PostMetadata.CommentCount,
                Score = post.PostMetadata.NetVotes,
                CurrentUserVote = currentUserId == null
                    ? 0
                    : post.Votes
                        .Where(v => v.UserId == currentUserId)
                        .Select(v => v.IsActive ? v.IsUpvote ? 1 : -1 : 0)
                        .FirstOrDefault()
            });
    }
}