using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorSocial.Data.Services;

/// <summary>
///     Represents the top-level Reddit API response. Every endpoint that returns a list of things
///     wraps the result in a Listing envelope.
/// </summary>
public class RedditData
{
    public string Kind { get; set; } = string.Empty;
    public RedditListingData Data { get; set; } = new();
}

public class RedditListingData
{
    public string? After { get; set; }
    public int Dist { get; set; }
    public string Modhash { get; set; } = string.Empty;
    public string? GeoFilter { get; set; }
    public List<RedditChild> Children { get; set; } = [];
}

public class RedditChild
{
    public string Kind { get; set; } = string.Empty;
    public RedditPostData Data { get; set; } = new();
}

public class RedditPostData
{
    public string Subreddit { get; set; } = string.Empty;
    public string Selftext { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsSelf { get; set; }
    public bool IsVideo { get; set; }
    public double CreatedUtc { get; set; }
    public int Score { get; set; }
    public int NumComments { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Permalink { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    [JsonPropertyName("over_18")]
    public bool Over18 { get; set; }
}
