using System.Security.Claims;
using BlazorSocial.Data;
using BlazorSocial.Data.Entities;
using BlazorSocial.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorSocial.Extensions;

public static class PostApiEndpoints
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder MapPostApiEndpoints()
        {
            endpoints.MapGet(ApiRoute.Templates.Posts,
                async (int startIndex, int count, HttpContext httpContext,
                    IDbContextFactory<ContentDbContext> dbContextFactory,
                    CancellationToken ct) =>
                {
                    try
                    {
                        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

                        var userId = httpContext.GetCurrentUserId();

                        var posts = await dbContext.Posts
                            .OrderByDescending(post => post.PostDate)
                            .Skip(startIndex)
                            .Take(count)
                            .ToViewPostDtos(userId)
                            .ToListAsync(ct);

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
                async (PostId id, HttpContext httpContext, IDbContextFactory<ContentDbContext> dbContextFactory,
                    CancellationToken ct) =>
                {
                    await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

                    var userId = httpContext.GetCurrentUserId();

                    var post = await dbContext.Posts
                        .Where(p => p.Id == id)
                        .Select(post => new PostDetailsDto
                        {
                            PostId = post.Id,
                            Title = post.Title,
                            Content = post.Content,
                            PostDate = post.PostDate,
                            ViewCount = post.PostMetadata.ViewCount,
                            NetVotes = post.PostMetadata.NetVotes,
                            Upvotes = post.PostMetadata.Upvotes,
                            Downvotes = post.PostMetadata.Downvotes,
                            CurrentUserVote = userId == null
                                ? 0
                                : post.Votes
                                    .Where(v => v.UserId == userId)
                                    .Select(v => v.IsActive ? v.IsUpvote ? 1 : -1 : 0)
                                    .FirstOrDefault()
                        })
                        .FirstOrDefaultAsync(ct);

                    if (post is null)
                    {
                        return Results.NotFound();
                    }

                    return Results.Ok(post);
                });

            endpoints.MapGet(ApiRoute.Templates.PostComments,
                async (PostId id, int startIndex, int count,
                    IDbContextFactory<ContentDbContext> dbContextFactory, CancellationToken ct) =>
                {
                    await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

                    var comments = await dbContext.Comments
                        .Include(c => c.Author)
                        .Where(c => c.PostId == id)
                        .OrderByDescending(c => c.PostDate)
                        .Skip(startIndex)
                        .Take(count)
                        .Select(c => new CommentDto
                        {
                            AuthorName = c.Author != null ? c.Author.DisplayName : "Unknown",
                            PostDate = c.PostDate,
                            Content = c.Content
                        })
                        .ToListAsync(ct);

                    return Results.Ok(comments);
                });

            endpoints.MapPost(ApiRoute.Templates.PostComments,
                async (PostId id, CreateCommentDto request, HttpContext httpContext,
                    IDbContextFactory<ContentDbContext> dbContextFactory, CancellationToken ct) =>
                {
                    var user = httpContext.User;
                    if (user.Identity?.IsAuthenticated != true)
                    {
                        return Results.Unauthorized();
                    }

                    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userIdClaim is null)
                    {
                        return Results.Unauthorized();
                    }

                    await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

                    var userId = UserId.Parse(userIdClaim);

                    var postExists = await dbContext.Posts.AnyAsync(p => p.Id == id, ct);
                    if (!postExists)
                    {
                        return Results.NotFound();
                    }

                    var comment = new Comment
                    {
                        PostId = id,
                        AuthorID = userId,
                        Content = request.Content,
                        PostDate = DateTime.Now
                    };
                    dbContext.Comments.Add(comment);
                    await dbContext.SaveChangesAsync(ct);

                    await dbContext.PostMetadatas
                        .Where(m => m.PostId == id)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(m => m.CommentCount, m => m.CommentCount + 1), ct);

                    return Results.Ok();
                });

            endpoints.MapPost(ApiRoute.Templates.PostVote,
                async (PostId id, VoteRequestDto request, HttpContext httpContext,
                    IDbContextFactory<ContentDbContext> dbContextFactory, CancellationToken ct) =>
                {
                    var user = httpContext.User;
                    if (user.Identity?.IsAuthenticated != true)
                    {
                        return Results.Unauthorized();
                    }

                    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userIdClaim is null)
                    {
                        return Results.Unauthorized();
                    }

                    await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

                    var userId = UserId.Parse(userIdClaim);

                    var vote = await dbContext.Votes
                        .FirstOrDefaultAsync(v => v.PostId == id && v.UserId == userId, ct);

                    if (vote is null)
                    {
                        vote = new Vote
                        {
                            PostId = id,
                            UserId = userId,
                            IsUpvote = request.IsUpvote,
                            IsActive = true,
                            VoteDate = DateTime.Now
                        };
                        dbContext.Votes.Add(vote);
                    }
                    else
                    {
                        if (vote.IsUpvote == request.IsUpvote && vote.IsActive)
                        {
                            vote.IsActive = false;
                        }
                        else
                        {
                            vote.IsActive = true;
                            vote.IsUpvote = request.IsUpvote;
                        }

                        vote.VoteDate = DateTime.Now;
                        dbContext.Votes.Update(vote);
                    }

                    await dbContext.SaveChangesAsync(ct);

                    await dbContext.UpdateVoteMetadataAsync(id);

                    return Results.Ok();
                });

            endpoints.MapPost(ApiRoute.Templates.PostView,
                async (PostId id, HttpContext httpContext,
                    IDbContextFactory<ContentDbContext> dbContextFactory, CancellationToken ct) =>
                {
                    await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

                    var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

                    var user = httpContext.User;
                    UserId? userId = null;
                    if (user.Identity?.IsAuthenticated == true)
                    {
                        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        if (userIdClaim is not null)
                        {
                            userId = UserId.Parse(userIdClaim);
                        }
                    }

                    await dbContext.RecordUserViewAsync(id, ipAddress, userId);
                    return Results.Ok();
                });

            return endpoints;
        }
    }
}