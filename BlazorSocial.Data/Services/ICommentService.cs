using BlazorSocial.Shared.Models;

namespace BlazorSocial.Data.Services;

public interface ICommentService
{
    Task<List<CommentDto>> GetCommentsAsync(PostId postId, int startIndex, int count, CancellationToken ct);
    Task AddCommentAsync(PostId postId, UserId authorId, string content, CancellationToken ct);
}
