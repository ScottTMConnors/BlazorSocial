using BlazorSocial.Data.Models;

namespace BlazorSocial.Data.Services;

public interface IPostService
{
    Task<List<PostRow>> GetPostsAsync(UserId? currentUserId, int startIndex, int count, CancellationToken ct);
    Task<PostDetailRow?> GetPostByIdAsync(PostId id, UserId? currentUserId, CancellationToken ct);
    Task<PostId> CreatePostAsync(string title, string content, UserId authorId, CancellationToken ct);
}
