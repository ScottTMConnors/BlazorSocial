using Refit;

namespace BlazorSocial.Shared.Models;

public interface IPostsApi
{
    [Get(ApiRoute.Templates.Posts)]
    Task<List<ViewPostDto>> GetPostsAsync(int startIndex, int count, CancellationToken cancellationToken = default);

    [Get(ApiRoute.Templates.PostById)]
    Task<PostDetailsDto> GetPostByIdAsync(PostId id, CancellationToken cancellationToken = default);

    [Post(ApiRoute.Templates.PostVote)]
    Task<VoteResponseDto> VoteOnPostAsync(PostId id, [Body] VoteRequestDto request, CancellationToken cancellationToken = default);

    [Post(ApiRoute.Templates.PostComments)]
    Task CommentOnPostAsync(PostId id, [Body] CreateCommentDto request, CancellationToken cancellationToken = default);

    [Get(ApiRoute.Templates.PostComments)]
    Task<List<CommentDto>> GetCommentsForPostAsync(PostId id, int startIndex, int count, CancellationToken cancellationToken = default);

    [Post(ApiRoute.Templates.PostView)]
    Task ViewPostAsync(PostId id, CancellationToken cancellationToken = default);
}
