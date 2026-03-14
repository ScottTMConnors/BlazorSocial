using Ardalis.SmartEnum;

namespace BlazorSocial.Shared.Models;

public class ApiRoute : SmartEnum<ApiRoute, string>
{
    public static readonly ApiRoute GetPosts = new(nameof(GetPosts), "/api/posts");
    public static readonly ApiRoute GetPostById = new(nameof(GetPostById), "/api/posts/{id}");
    public static readonly ApiRoute CreatePost = new(nameof(CreatePost), "/api/posts");
    public static readonly ApiRoute UpdatePost = new(nameof(UpdatePost), "/api/posts/{id}");
    public static readonly ApiRoute DeletePost = new(nameof(DeletePost), "/api/posts/{id}");
    public static readonly ApiRoute VoteOnPost = new(nameof(VoteOnPost), "/api/posts/{id}/vote");
    public static readonly ApiRoute CommentOnPost = new(nameof(CommentOnPost), "/api/posts/{id}/comments");
    public static readonly ApiRoute GetCommentsForPost = new(nameof(GetCommentsForPost), "/api/posts/{id}/comments");

    private ApiRoute(string name, string value) : base(name, value)
    {
    }
}