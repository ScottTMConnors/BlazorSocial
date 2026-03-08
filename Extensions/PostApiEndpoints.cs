using System.Security.Claims;
using BlazorSocial.Client.Models;
using BlazorSocial.Data;
using BlazorSocial.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorSocial.Extensions;

public static class PostApiEndpoints
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder MapPostApiEndpoints()
        {
            endpoints.MapGet("/api/posts",
                async (int startIndex, int count, IDbContextFactory<ContentDbContext> dbContextFactory,
                    CancellationToken ct) =>
                {
                    try
                    {
                        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

                        //await Task.Delay(2000, ct);

                        var posts = await dbContext.Posts
                            .OrderByDescending(post => post.PostDate)
                            .Skip(startIndex)
                            .Take(count)
                            .ToViewPostDtos()
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

            endpoints.MapGet("/api/posts/{id:guid}",
                async (Guid id, HttpContext httpContext, IDbContextFactory<ContentDbContext> dbContextFactory,
                    CancellationToken ct) =>
                {
                    await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

                    var postId = PostId.Parse(id.ToString());
                    var post = await dbContext.Posts
                        .Include(p => p.PostMetadata)
                        .FirstOrDefaultAsync(p => p.Id == postId, ct);

                    if (post is null)
                    {
                        return Results.NotFound();
                    }

                    var user = httpContext.User;
                    var isAuthenticated = user.Identity?.IsAuthenticated ?? false;
                    var isUpvote = false;
                    var isVoteActive = false;

                    if (isAuthenticated)
                    {
                        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        if (userIdClaim is not null)
                        {
                            var userId = UserId.Parse(userIdClaim);
                            var vote = await dbContext.Votes
                                .FirstOrDefaultAsync(v => v.UserId == userId && v.PostId == postId, ct);
                            if (vote is not null)
                            {
                                isUpvote = vote.IsUpvote;
                                isVoteActive = vote.IsActive;
                            }
                        }
                    }

                    return Results.Ok(new PostDetailsDto
                    {
                        PostId = post.Id.Value,
                        Title = post.Title,
                        Content = post.Content,
                        PostDate = post.PostDate,
                        ViewCount = post.PostMetadata?.ViewCount ?? 0,
                        NetVotes = post.PostMetadata?.NetVotes ?? 0,
                        Upvotes = post.PostMetadata?.Upvotes ?? 0,
                        Downvotes = post.PostMetadata?.Downvotes ?? 0,
                        IsAuthenticated = isAuthenticated,
                        IsUpvote = isUpvote,
                        IsVoteActive = isVoteActive
                    });
                });

            endpoints.MapGet("/api/posts/{id:guid}/comments",
                async (Guid id, int startIndex, int count,
                    IDbContextFactory<ContentDbContext> dbContextFactory, CancellationToken ct) =>
                {
                    await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
                    var postId = PostId.Parse(id);

                    var comments = await dbContext.Comments
                        .Include(c => c.Author)
                        .Where(c => c.PostId == postId)
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

            endpoints.MapPost("/api/posts/{id:guid}/comments",
                async (Guid id, CreateCommentDto request, HttpContext httpContext,
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

                    var postId = PostId.Parse(id);
                    var userId = UserId.Parse(userIdClaim);

                    var postExists = await dbContext.Posts.AnyAsync(p => p.Id == postId, ct);
                    if (!postExists)
                    {
                        return Results.NotFound();
                    }

                    var comment = new Comment
                    {
                        PostId = postId,
                        AuthorID = userId,
                        Content = request.Content,
                        PostDate = DateTime.Now
                    };
                    dbContext.Comments.Add(comment);
                    await dbContext.SaveChangesAsync(ct);

                    await dbContext.PostMetadatas
                        .Where(m => m.PostId == postId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(m => m.CommentCount, m => m.CommentCount + 1), ct);

                    return Results.Ok();
                });

            endpoints.MapPost("/api/posts/{id:guid}/vote",
                async (Guid id, VoteRequestDto request, HttpContext httpContext,
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

                    var postId = PostId.Parse(id);
                    var userId = UserId.Parse(userIdClaim);

                    var vote = await dbContext.Votes
                        .FirstOrDefaultAsync(v => v.PostId == postId && v.UserId == userId, ct);

                    if (vote is null)
                    {
                        vote = new Vote
                        {
                            PostId = postId,
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

                    await dbContext.UpdateVoteMetadataAsync(postId);

                    var metadata = await dbContext.PostMetadatas.FirstOrDefaultAsync(m => m.PostId == postId, ct);

                    return Results.Ok(new VoteResponseDto
                    {
                        IsUpvote = vote.IsUpvote,
                        IsActive = vote.IsActive,
                        NetVotes = metadata?.NetVotes ?? 0,
                        Upvotes = metadata?.Upvotes ?? 0,
                        Downvotes = metadata?.Downvotes ?? 0
                    });
                });

            endpoints.MapPost("/api/posts/{id:guid}/view",
                async (Guid id, HttpContext httpContext,
                    IDbContextFactory<ContentDbContext> dbContextFactory, CancellationToken ct) =>
                {
                    await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

                    var postId = PostId.Parse(id);
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

                    await dbContext.RecordUserViewAsync(postId, ipAddress, userId);
                    return Results.Ok();
                });

            return endpoints;
        }
    }
}