using BlazorSocial.Shared.Models;

namespace BlazorSocial.Data.Services;

public interface IPostQueryService
{
    Task<List<ViewPostDto>> GetPostsAsync(UserId? currentUserId, int startIndex, int count, CancellationToken ct);
    Task<PostDetailsDto?> GetPostByIdAsync(PostId id, UserId? currentUserId, CancellationToken ct);
}
