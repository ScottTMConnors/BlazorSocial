using BlazorSocial.Data.Services;
using BlazorSocial.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlazorSocial.Api.Extensions;

public static class PostApiEndpoints
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder MapPostApiEndpoints()
        {
            endpoints.MapGet(ApiRoute.Templates.Posts,
                async (HttpContext httpContext,
                    IPostQueryService postQueryService,
                    CancellationToken ct,
                    [FromQuery] int startIndex = 0,
                    [FromQuery] int count = 10) =>
                {
                    try
                    {
                        var userId = httpContext.GetCurrentUserId();
                        var posts = await postQueryService.GetPostsAsync(userId, startIndex, count, ct);
                        return Results.Ok(posts);
                    }
                    catch (OperationCanceledException)
                    {
                        return Results.StatusCode(499);
                    }
                    catch (InvalidOperationException) when (ct.IsCancellationRequested)
                    {
                        return Results.StatusCode(499);
                    }
                });

            endpoints.MapGet(ApiRoute.Templates.PostById,
                async (PostId id, HttpContext httpContext, IPostQueryService postQueryService, CancellationToken ct) =>
                {
                    var userId = httpContext.GetCurrentUserId();
                    var post = await postQueryService.GetPostByIdAsync(id, userId, ct);

                    if (post is null)
                    {
                        return Results.NotFound();
                    }

                    return Results.Ok(post);
                });

            endpoints.MapGet(ApiRoute.Templates.PostComments,
                async (PostId id,
                    ICommentService commentService,
                    CancellationToken ct,
                    [FromQuery] int startIndex = 0,
                    [FromQuery] int count = 10) =>
                {
                    var comments = await commentService.GetCommentsAsync(id, startIndex, count, ct);
                    return Results.Ok(comments);
                });

            endpoints.MapPost(ApiRoute.Templates.PostComments,
                async (PostId id, CreateCommentDto request, HttpContext httpContext,
                    ICommentService commentService, CancellationToken ct) =>
                {
                    var user = httpContext.User;
                    if (user.Identity?.IsAuthenticated != true)
                    {
                        return Results.Unauthorized();
                    }

                    var userId = httpContext.GetCurrentUserId();
                    if (userId is null)
                    {
                        return Results.Unauthorized();
                    }

                    await commentService.AddCommentAsync(id, userId, request.Content, ct);
                    return Results.Ok();
                });

            endpoints.MapPost(ApiRoute.Templates.PostVote,
                async (PostId id, VoteRequestDto request, HttpContext httpContext,
                    IVoteService voteService, CancellationToken ct) =>
                {
                    var user = httpContext.User;
                    if (user.Identity?.IsAuthenticated != true)
                    {
                        return Results.Unauthorized();
                    }

                    var userId = httpContext.GetCurrentUserId();
                    if (userId is null)
                    {
                        return Results.Unauthorized();
                    }

                    await voteService.VoteAsync(id, userId, request.IsUpvote, ct);
                    return Results.Ok();
                });

            endpoints.MapPost(ApiRoute.Templates.Posts,
                async (
                    [FromBody] CreatePostDto dto,
                    HttpContext http,
                    IPostCommandService postCommandService,
                    CancellationToken ct) =>
                {
                    var userId = http.GetCurrentUserId();
                    if (userId is null) return Results.Unauthorized();

                    var postId = await postCommandService.CreatePostAsync(dto.Title, dto.Content, userId, ct);
                    return Results.Ok(postId);
                })
                .RequireAuthorization();

            endpoints.MapPost(ApiRoute.Templates.PostView,
                async (PostId id, HttpContext httpContext,
                    IViewTrackingService viewTrackingService, CancellationToken ct) =>
                {
                    var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                    var userId = httpContext.GetCurrentUserId();

                    await viewTrackingService.RecordViewAsync(id, ipAddress, userId, ct);
                    return Results.Ok();
                });

            return endpoints;
        }
    }
}
