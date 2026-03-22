namespace BlazorSocial.ContentRoutes;

public static class ContentRoutes
{
    public static class Posts
    {
        public const string Base     = "/api/posts";
        public const string ById     = "/api/posts/{id}";
        public const string Comments = "/api/posts/{id}/comments";
        public const string Vote     = "/api/posts/{id}/vote";
        public const string View     = "/api/posts/{id}/view";
    }

    public static class Internal
    {
        public const string Users = "/api/internal/users";
    }
}
