namespace BlazorSocial.Data.Services;

public interface IPostCommandService
{
    Task<PostId> CreatePostAsync(string title, string content, UserId authorId, CancellationToken ct);
}
