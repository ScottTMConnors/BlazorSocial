using Refit;
using Content = BlazorSocial.Shared.ContentRoutes;

namespace BlazorSocial.Shared.Models;

public interface IPostsApi
{
    [Get(Content.Posts.Base)]
    Task<List<ViewPostDto>> GetPostsAsync(int startIndex, int count, CancellationToken cancellationToken = default);

    [Get(Content.Posts.ById)]
    Task<PostDetailsDto> GetPostByIdAsync(PostId id, CancellationToken cancellationToken = default);

    [Post(Content.Posts.Vote)]
    Task VoteOnPostAsync(PostId id, [Body] VoteRequestDto request, CancellationToken cancellationToken = default);

    [Post(Content.Posts.Comments)]
    Task CommentOnPostAsync(PostId id, [Body] CreateCommentDto request, CancellationToken cancellationToken = default);

    [Get(Content.Posts.Comments)]
    Task<List<CommentDto>> GetCommentsForPostAsync(PostId id, int startIndex, int count, CancellationToken cancellationToken = default);

    [Post(Content.Posts.View)]
    Task ViewPostAsync(PostId id, CancellationToken cancellationToken = default);

    [Post(Content.Posts.Base)]
    Task<PostId> CreatePostAsync([Body] CreatePostDto request, CancellationToken cancellationToken = default);
}
