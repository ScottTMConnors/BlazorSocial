namespace BlazorSocial.Data.Services.Caching;

public static class CacheKeys
{
    public static string PostFeed(int start, int count) => $"posts:feed:{start}:{count}";
    public static string PostDetail(PostId id) => $"posts:{id}";
    public static string PostComments(PostId id, int start, int count) => $"posts:{id}:comments:{start}:{count}";
}
