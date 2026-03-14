namespace BlazorSocial.Shared.Models;

public static class ApiRoute
{
    /// <summary>
    /// Route templates used by both the Refit client interface and server-side endpoint mapping.
    /// </summary>
    public static class Templates
    {
        public const string Posts = "/api/posts";
        public const string PostById = Posts + "/{id}";
        public const string PostVote = PostById + "/vote";
        public const string PostComments = PostById + "/comments";
        public const string PostView = PostById + "/view";
    }
}