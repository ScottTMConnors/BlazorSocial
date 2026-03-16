using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorSocial.Services.DataGeneratorService;

/// <summary>
///     Represents the top-level Reddit API response. Every endpoint that returns a list of things
///     wraps the result in a Listing envelope.
/// </summary>
public class RedditData
{
    /// <summary>
    ///     The type-prefixed fullname of this object. For listing responses this is always "Listing".
    /// </summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>
    ///     The payload of the listing, containing pagination cursors and the array of children.
    /// </summary>
    public RedditListingData Data { get; set; } = new();
}

/// <summary>
///     The data envelope inside a <see cref="RedditData" />. Contains pagination info and the
///     actual collection of items returned by the API.
/// </summary>
public class RedditListingData
{
    /// <summary>
    ///     The fullname of the last item in this listing, used as a cursor for pagination.
    ///     Pass this value as the <c>after</c> query parameter to fetch the next page.
    ///     Example: <c>"t3_1rkv443"</c>. Null when there are no more pages.
    /// </summary>
    public string? After { get; set; }

    /// <summary>
    ///     The number of children (items) returned in this listing page.
    ///     Typically matches the <c>limit</c> query parameter (default 25, max 100).
    ///     Example: <c>100</c>.
    /// </summary>
    public int Dist { get; set; }

    /// <summary>
    ///     A modhash string used for CSRF protection when making write requests via the web UI.
    ///     Empty string for unauthenticated/read-only API calls.
    /// </summary>
    public string Modhash { get; set; } = string.Empty;

    /// <summary>
    ///     Geographic filter applied to the listing results, if any. Null when no geo-filter is active.
    ///     Used by Reddit to restrict content by region.
    /// </summary>
    public string? GeoFilter { get; set; }

    /// <summary>
    ///     The list of child items in the listing. For subreddit endpoints each child is a link (post).
    /// </summary>
    public List<RedditChild> Children { get; set; } = [];
}

/// <summary>
///     Represents a single child item inside a Reddit listing. Each child wraps a kind identifier
///     and the actual data payload for that thing.
/// </summary>
public class RedditChild
{
    /// <summary>
    ///     The type prefix for this thing. For link posts this is <c>"t3"</c>.
    ///     Other types include t1 (comment), t2 (account), t4 (message), t5 (subreddit).
    /// </summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>
    ///     The full data payload for this child. When Kind is "t3", this contains the post data.
    /// </summary>
    public RedditPostData Data { get; set; } = new();
}

/// <summary>
///     Represents the full data payload of a Reddit link/post (t3 thing).
///     Maps every field returned by the Reddit JSON API for a post.
/// </summary>
public class RedditPostData
{
    /// <summary>
    ///     UTC epoch timestamp of when a moderator approved this post.
    ///     Null for posts that have not been explicitly approved.
    ///     Example: <c>1772821114.0</c> or <c>null</c>.
    /// </summary>
    public double? ApprovedAtUtc { get; set; }

    /// <summary>
    ///     The name of the subreddit this post belongs to, without the "r/" prefix.
    ///     Example: <c>"confession"</c>.
    /// </summary>
    public string Subreddit { get; set; } = string.Empty;

    /// <summary>
    ///     The raw Markdown body text of a self (text) post. Empty string for link posts.
    ///     Example: <c>"Long story short. Bought a car..."</c>.
    /// </summary>
    public string Selftext { get; set; } = string.Empty;

    /// <summary>
    ///     The type-prefixed fullname of the post author's account.
    ///     Example: <c>"t2_9sxo2fjy"</c>. Prefixed with "t2_" indicating a user account.
    /// </summary>
    public string AuthorFullname { get; set; } = string.Empty;

    /// <summary>
    ///     Whether the authenticated user has saved this post. Always <c>false</c> for
    ///     unauthenticated requests.
    /// </summary>
    public bool Saved { get; set; }

    /// <summary>
    ///     The title/reason a moderator gave when taking action on this post.
    ///     Null when no moderator action has been taken or no reason was provided.
    /// </summary>
    public string? ModReasonTitle { get; set; }

    /// <summary>
    ///     The number of times this post has been gilded (given Reddit Gold).
    ///     Gilding is a legacy awarding system. Example: <c>0</c>.
    /// </summary>
    public int Gilded { get; set; }

    /// <summary>
    ///     Whether the authenticated user has clicked on this link post.
    ///     Always <c>false</c> for unauthenticated requests. Used for tracking "visited" state.
    /// </summary>
    public bool Clicked { get; set; }

    /// <summary>
    ///     The title of the post as displayed on the subreddit listing.
    ///     Limited to 300 characters by Reddit.
    ///     Example: <c>"Car dealer never cashed $7500 down payment check I gave them"</c>.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     An array of rich-text flair objects for the post's link flair. Each element can contain
    ///     text or emoji objects with type, text, and optional URL fields. Empty array when no
    ///     rich-text flair is set.
    /// </summary>
    public JsonElement LinkFlairRichtext { get; set; }

    /// <summary>
    ///     The subreddit name with the "r/" prefix included.
    ///     Example: <c>"r/confession"</c>.
    /// </summary>
    public string SubredditNamePrefixed { get; set; } = string.Empty;

    /// <summary>
    ///     Whether the authenticated user has hidden this post from their feed.
    ///     Always <c>false</c> for unauthenticated requests.
    /// </summary>
    public bool Hidden { get; set; }

    /// <summary>
    ///     "Pro-whitelist status" — an internal Reddit ad-safety score for the subreddit (0-6).
    ///     Higher values indicate the subreddit is considered safer for advertisers.
    ///     Example: <c>6</c>.
    /// </summary>
    public int Pwls { get; set; }

    /// <summary>
    ///     The CSS class applied to the post's link flair in old Reddit's stylesheet.
    ///     Null when no CSS flair class is assigned.
    /// </summary>
    public string? LinkFlairCssClass { get; set; }

    /// <summary>
    ///     The number of downvotes. Reddit fuzzes this value; it is typically <c>0</c> in API
    ///     responses. Use <see cref="Score" /> or <see cref="UpvoteRatio" /> for accurate metrics.
    /// </summary>
    public int Downs { get; set; }

    /// <summary>
    ///     The type of the top (most prestigious) award received on this post.
    ///     Null when the post has received no awards.
    /// </summary>
    public string? TopAwardedType { get; set; }

    /// <summary>
    ///     Whether the post's score is currently hidden. Subreddits can configure a time window
    ///     during which scores are hidden after posting. Example: <c>false</c>.
    /// </summary>
    public bool HideScore { get; set; }

    /// <summary>
    ///     The fullname (type-prefixed unique ID) of this post, used in API calls.
    ///     Example: <c>"t3_1rmlohf"</c>. The "t3_" prefix indicates a link/post.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Whether the subreddit this post belongs to is quarantined. Quarantined subreddits
    ///     require explicit opt-in to view and are restricted from generating ad revenue.
    /// </summary>
    public bool Quarantine { get; set; }

    /// <summary>
    ///     The text color of the post's link flair label. Either <c>"dark"</c> or <c>"light"</c>,
    ///     chosen to contrast with <see cref="LinkFlairBackgroundColor" />.
    /// </summary>
    public string LinkFlairTextColor { get; set; } = string.Empty;

    /// <summary>
    ///     The ratio of upvotes to total votes as a decimal between 0.0 and 1.0.
    ///     Example: <c>0.96</c> means 96% of votes were upvotes.
    /// </summary>
    public double UpvoteRatio { get; set; }

    /// <summary>
    ///     The hex background color of the author's user flair in the subreddit.
    ///     Null when the author has no flair or no background color is set.
    /// </summary>
    public string? AuthorFlairBackgroundColor { get; set; }

    /// <summary>
    ///     The type of the subreddit. Common values: <c>"public"</c>, <c>"private"</c>,
    ///     <c>"restricted"</c>, <c>"gold_restricted"</c>, <c>"archived"</c>.
    /// </summary>
    public string SubredditType { get; set; } = string.Empty;

    /// <summary>
    ///     The number of upvotes. Reddit fuzzes this value to deter vote manipulation.
    ///     Use <see cref="Score" /> for the net score. Example: <c>3063</c>.
    /// </summary>
    public int Ups { get; set; }

    /// <summary>
    ///     The total number of awards (all types) this post has received.
    ///     Example: <c>0</c>.
    /// </summary>
    public int TotalAwardsReceived { get; set; }

    /// <summary>
    ///     Embedded media metadata for rich media link posts (e.g., embedded videos or iframes).
    ///     Contains fields like <c>content</c>, <c>width</c>, <c>height</c>.
    ///     Empty object <c>{}</c> for self posts or posts without embeddable media.
    /// </summary>
    public JsonElement MediaEmbed { get; set; }

    /// <summary>
    ///     The ID of the flair template assigned to the author in this subreddit.
    ///     Null when the author has no template-based flair.
    /// </summary>
    public string? AuthorFlairTemplateId { get; set; }

    /// <summary>
    ///     Whether the post is marked as Original Content (OC) by the author or subreddit mods.
    ///     OC posts may receive special flair/badges in the UI.
    /// </summary>
    public bool IsOriginalContent { get; set; }

    /// <summary>
    ///     An array of user-submitted reports on this post. Each report is an array of [reason, count].
    ///     Only populated for moderators; empty array <c>[]</c> for regular users.
    /// </summary>
    public JsonElement UserReports { get; set; }

    /// <summary>
    ///     Secure (HTTPS) media object for link posts that point to media resources.
    ///     Contains provider info, embed HTML, thumbnail URL, etc.
    ///     Null for self posts or posts without recognized media.
    /// </summary>
    public JsonElement? SecureMedia { get; set; }

    /// <summary>
    ///     Whether the post URL points to a Reddit-hosted media domain (i.redd.it, v.redd.it).
    ///     <c>true</c> for Reddit-hosted images/videos; <c>false</c> for self posts or external links.
    /// </summary>
    public bool IsRedditMediaDomain { get; set; }

    /// <summary>
    ///     Whether this post is a meta post about the subreddit itself (subreddit discussion).
    ///     Example: <c>false</c>.
    /// </summary>
    public bool IsMeta { get; set; }

    /// <summary>
    ///     The content category assigned to this post by the author (e.g., "drawing_and_painting").
    ///     Null for most posts; used in specific subreddits with category-based browsing.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    ///     Secure (HTTPS) version of embedded media metadata, similar to <see cref="MediaEmbed" />
    ///     but using HTTPS URLs. Empty object <c>{}</c> when not applicable.
    /// </summary>
    public JsonElement SecureMediaEmbed { get; set; }

    /// <summary>
    ///     The plain-text label of the post's link flair.
    ///     Null when no flair is assigned. Example: <c>"Remorse"</c>.
    /// </summary>
    public string? LinkFlairText { get; set; }

    /// <summary>
    ///     Whether the authenticated user has moderator permissions to take actions on this post.
    ///     Always <c>false</c> for unauthenticated requests.
    /// </summary>
    public bool CanModPost { get; set; }

    /// <summary>
    ///     The net score of the post (approximately upvotes minus downvotes). Reddit fuzzes this
    ///     value slightly. Example: <c>3063</c>.
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    ///     The username of the moderator who approved this post.
    ///     Null when the post has not been explicitly approved.
    /// </summary>
    public string? ApprovedBy { get; set; }

    /// <summary>
    ///     Whether this post was created through Reddit's advertising/promoted post UI.
    ///     <c>true</c> for promoted posts; <c>false</c> for organic posts.
    /// </summary>
    public bool IsCreatedFromAdsUi { get; set; }

    /// <summary>
    ///     Whether the post author has Reddit Premium (formerly Reddit Gold subscription).
    ///     Premium users have an ad-free experience and access to r/lounge.
    /// </summary>
    public bool AuthorPremium { get; set; }

    /// <summary>
    ///     URL of the post's thumbnail image. Special sentinel values include:
    ///     <c>"self"</c> (self post), <c>"default"</c> (no thumbnail), <c>"nsfw"</c>,
    ///     <c>"spoiler"</c>, <c>"image"</c>, or an actual URL for link posts with previews.
    ///     Empty string when no thumbnail is available.
    /// </summary>
    public string Thumbnail { get; set; } = string.Empty;

    /// <summary>
    ///     Whether and when the post was edited. <c>false</c> (boolean) if never edited;
    ///     otherwise a Unix epoch timestamp (double) of the last edit time.
    ///     Stored as <see cref="JsonElement" /> to handle the polymorphic type.
    /// </summary>
    public JsonElement Edited { get; set; }

    /// <summary>
    ///     The CSS class of the author's user flair in old Reddit's stylesheet.
    ///     Null when no CSS flair class is assigned.
    /// </summary>
    public string? AuthorFlairCssClass { get; set; }

    /// <summary>
    ///     An array of rich-text flair objects for the author's user flair.
    ///     Each element can contain text or emoji objects. Empty array when no rich-text flair is set.
    /// </summary>
    public JsonElement AuthorFlairRichtext { get; set; }

    /// <summary>
    ///     A dictionary of gilding counts keyed by gilding type (e.g., <c>"gid_1"</c> for Silver,
    ///     <c>"gid_2"</c> for Gold, <c>"gid_3"</c> for Platinum). Empty object <c>{}</c> when
    ///     the post has not been gilded.
    /// </summary>
    public JsonElement Gildings { get; set; }

    /// <summary>
    ///     An array of content category strings assigned to the post.
    ///     Null for most posts. Used by Reddit to classify content for ads and recommendations.
    /// </summary>
    public JsonElement? ContentCategories { get; set; }

    /// <summary>
    ///     Whether the post is a self (text) post as opposed to a link post.
    ///     <c>true</c> for text posts whose content is in <see cref="Selftext" />;
    ///     <c>false</c> for posts linking to an external URL.
    /// </summary>
    public bool IsSelf { get; set; }

    /// <summary>
    ///     A private note left by a moderator on this post, visible only to subreddit moderators.
    ///     Null when no mod note exists or the requester is not a moderator.
    /// </summary>
    public string? ModNote { get; set; }

    /// <summary>
    ///     Unix epoch timestamp (in seconds) of when the post was created, in the server's local time.
    ///     Use <see cref="CreatedUtc" /> for the UTC equivalent. Example: <c>1772821114.0</c>.
    /// </summary>
    public double Created { get; set; }

    /// <summary>
    ///     The type of link flair rendering. Either <c>"text"</c> for plain text flair or
    ///     <c>"richtext"</c> for flair with emojis/icons defined in <see cref="LinkFlairRichtext" />.
    /// </summary>
    public string LinkFlairType { get; set; } = string.Empty;

    /// <summary>
    ///     "Whitelist status" — an internal Reddit ad-safety score for this specific post (0-6).
    ///     Higher values indicate the content is safer for ad placement. Mirrors subreddit-level
    ///     <see cref="Pwls" /> unless overridden.
    /// </summary>
    public int Wls { get; set; }

    /// <summary>
    ///     The category/reason the post was removed, if applicable. Values include
    ///     <c>"reddit"</c> (anti-evil ops), <c>"moderator"</c>, <c>"automod_filtered"</c>, etc.
    ///     Null when the post has not been removed.
    /// </summary>
    public string? RemovedByCategory { get; set; }

    /// <summary>
    ///     The username of the moderator or admin who banned/removed this post.
    ///     Null when the post has not been banned.
    /// </summary>
    public string? BannedBy { get; set; }

    /// <summary>
    ///     The type of the author's user flair rendering. Either <c>"text"</c> for plain text
    ///     or <c>"richtext"</c> for flair with emojis/icons.
    /// </summary>
    public string AuthorFlairType { get; set; } = string.Empty;

    /// <summary>
    ///     The domain of the post's URL. For self posts this is <c>"self.{subreddit}"</c>.
    ///     For link posts it is the hostname of the linked URL (e.g., <c>"youtube.com"</c>,
    ///     <c>"i.redd.it"</c>). Example: <c>"self.confession"</c>.
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    ///     Whether live comments (real-time comment streaming) are enabled for this post.
    ///     Typically <c>false</c>; enabled for high-traffic posts like AMAs or live events.
    /// </summary>
    public bool AllowLiveComments { get; set; }

    /// <summary>
    ///     The HTML-rendered version of <see cref="Selftext" />, wrapped in Reddit's
    ///     <c>&lt;!-- SC_OFF/SC_ON --&gt;</c> markers. Contains sanitized HTML with
    ///     paragraph tags, links, etc. Null for link posts or when selftext is empty.
    /// </summary>
    public string? SelftextHtml { get; set; }

    /// <summary>
    ///     The authenticated user's vote on this post. <c>true</c> for upvote, <c>false</c> for
    ///     downvote, <c>null</c> for no vote or unauthenticated requests.
    /// </summary>
    public bool? Likes { get; set; }

    /// <summary>
    ///     The suggested default comment sort order for this post. Values include <c>"confidence"</c>,
    ///     <c>"top"</c>, <c>"new"</c>, <c>"controversial"</c>, <c>"old"</c>, <c>"qa"</c>.
    ///     Null when no sort is suggested (uses subreddit default).
    /// </summary>
    public string? SuggestedSort { get; set; }

    /// <summary>
    ///     UTC epoch timestamp of when the post author was banned from the subreddit.
    ///     Null when the author is not banned. Only visible to moderators.
    /// </summary>
    public double? BannedAtUtc { get; set; }

    /// <summary>
    ///     The number of times this post has been viewed. Reddit rarely populates this field;
    ///     it is typically <c>null</c>. When present, it reflects the raw view count.
    /// </summary>
    public int? ViewCount { get; set; }

    /// <summary>
    ///     Whether the post has been archived. Archived posts (older than ~6 months by default)
    ///     can no longer be voted on or commented on.
    /// </summary>
    public bool Archived { get; set; }

    /// <summary>
    ///     Whether the post should be excluded from "follow" recommendations. When <c>true</c>,
    ///     the post's activity will not trigger follower notifications. Typically correlates with
    ///     low engagement or new posts.
    /// </summary>
    public bool NoFollow { get; set; }

    /// <summary>
    ///     Whether the post can be crossposted to other subreddits. Subreddit settings or post
    ///     properties (e.g., NSFW, spoiler) may prevent crossposting.
    /// </summary>
    public bool IsCrosspostable { get; set; }

    /// <summary>
    ///     Whether the post is pinned to the author's profile. Distinct from <see cref="Stickied" />,
    ///     which pins to the subreddit. Users can pin up to 4 posts to their profile.
    /// </summary>
    public bool Pinned { get; set; }

    /// <summary>
    ///     Whether the post is marked as Not Safe For Work (18+ content).
    ///     <c>true</c> for adult content that requires age verification.
    /// </summary>
    [JsonPropertyName("over_18")]
    public bool Over18 { get; set; }

    /// <summary>
    ///     An array of all award objects granted to this post. Each element contains the award name,
    ///     icon URL, description, count, coin price, and other metadata. Empty array <c>[]</c> when
    ///     the post has no awards.
    /// </summary>
    public JsonElement AllAwardings { get; set; }

    /// <summary>
    ///     An array of user IDs or objects representing users who gave awards to this post.
    ///     Empty array <c>[]</c> when no awards have been given or awarder info is hidden.
    /// </summary>
    public JsonElement Awarders { get; set; }

    /// <summary>
    ///     Whether this post consists only of media content with no text body.
    ///     <c>true</c> for gallery or media-only posts; <c>false</c> for standard posts.
    /// </summary>
    public bool MediaOnly { get; set; }

    /// <summary>
    ///     Whether the authenticated user can gild (give Reddit Gold to) this post.
    ///     Depends on the user's coin balance and the post's eligibility. Always <c>false</c>
    ///     for unauthenticated requests.
    /// </summary>
    public bool CanGild { get; set; }

    /// <summary>
    ///     Whether the post is marked as containing spoilers. Spoiler posts have their
    ///     thumbnails and previews blurred until the user clicks through.
    /// </summary>
    public bool Spoiler { get; set; }

    /// <summary>
    ///     Whether the post's comment section has been locked by a moderator.
    ///     No new comments can be added to locked posts.
    /// </summary>
    public bool Locked { get; set; }

    /// <summary>
    ///     The plain-text label of the author's user flair in this subreddit.
    ///     Null when the author has no flair. Example: <c>"Verified"</c>.
    /// </summary>
    public string? AuthorFlairText { get; set; }

    /// <summary>
    ///     An array of treatment tag strings applied by Reddit for internal A/B testing and
    ///     content experiments. Empty array <c>[]</c> for most posts.
    /// </summary>
    public JsonElement TreatmentTags { get; set; }

    /// <summary>
    ///     Whether the authenticated user has previously visited this link post.
    ///     Requires Reddit Gold/Premium and the "show visited links" preference.
    ///     Always <c>false</c> for unauthenticated requests.
    /// </summary>
    public bool Visited { get; set; }

    /// <summary>
    ///     The username of the moderator or admin who removed this post.
    ///     Null when the post has not been removed. Only visible to moderators.
    /// </summary>
    public string? RemovedBy { get; set; }

    /// <summary>
    ///     The number of pending reports on this post. Only visible to moderators.
    ///     Null for non-moderators. Example: <c>3</c>.
    /// </summary>
    public int? NumReports { get; set; }

    /// <summary>
    ///     Whether and how the post is distinguished by a moderator or admin.
    ///     Values: <c>"moderator"</c> (green), <c>"admin"</c> (red), <c>null</c> (not distinguished).
    /// </summary>
    public string? Distinguished { get; set; }

    /// <summary>
    ///     The type-prefixed fullname of the subreddit. Example: <c>"t5_2qo2a"</c>.
    ///     The "t5_" prefix indicates a subreddit thing.
    /// </summary>
    public string SubredditId { get; set; } = string.Empty;

    /// <summary>
    ///     Whether the authenticated user has blocked the post author.
    ///     Always <c>false</c> for unauthenticated requests.
    /// </summary>
    public bool AuthorIsBlocked { get; set; }

    /// <summary>
    ///     The username of the moderator who provided the <see cref="ModReasonTitle" />.
    ///     Null when no mod reason has been given.
    /// </summary>
    public string? ModReasonBy { get; set; }

    /// <summary>
    ///     The reason the post was removed, as a legacy field.
    ///     Null in most responses; superseded by <see cref="RemovedByCategory" />.
    /// </summary>
    public string? RemovalReason { get; set; }

    /// <summary>
    ///     The hex background color for the post's link flair label (e.g., <c>"#ff4500"</c>).
    ///     Empty string when no background color is set. Used with <see cref="LinkFlairTextColor" />
    ///     for rendering.
    /// </summary>
    public string LinkFlairBackgroundColor { get; set; } = string.Empty;

    /// <summary>
    ///     The unique base-36 ID of this post (without the type prefix).
    ///     Example: <c>"1rmlohf"</c>. Combined with "t3_" prefix to form the fullname.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    ///     Whether search engine robots are allowed to index this post.
    ///     <c>false</c> for removed, quarantined, or otherwise restricted posts.
    /// </summary>
    public bool IsRobotIndexable { get; set; }

    /// <summary>
    ///     An array of report reason strings from various sources (users, mods, automated).
    ///     Null for non-moderators. Only populated when reports exist and the viewer is a moderator.
    /// </summary>
    public JsonElement? ReportReasons { get; set; }

    /// <summary>
    ///     The username of the post author (without the "u/" prefix).
    ///     Example: <c>"lexixon212"</c>. Shows <c>"[deleted]"</c> if the account was deleted.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    ///     The type of discussion format for this post. Typically <c>null</c>. Reddit has
    ///     experimented with values like <c>"CHAT"</c> for live chat-style discussions.
    /// </summary>
    public string? DiscussionType { get; set; }

    /// <summary>
    ///     The total number of comments on this post, including removed and deleted comments.
    ///     Example: <c>446</c>.
    /// </summary>
    public int NumComments { get; set; }

    /// <summary>
    ///     Whether the post author has opted in to receiving reply notifications for this post.
    ///     <c>true</c> by default when posting.
    /// </summary>
    public bool SendReplies { get; set; }

    /// <summary>
    ///     Whether contest mode is enabled. In contest mode, comments are sorted randomly,
    ///     scores are hidden, and reply collapsing is disabled to prevent bandwagon voting.
    /// </summary>
    public bool ContestMode { get; set; }

    /// <summary>
    ///     An array of moderator-submitted reports on this post. Each report is an array of
    ///     [reason, moderator_username]. Empty array <c>[]</c> when there are no mod reports.
    ///     Only visible to moderators.
    /// </summary>
    public JsonElement ModReports { get; set; }

    /// <summary>
    ///     Whether the post author has linked their Patreon account and has the Patreon flair enabled.
    ///     Part of Reddit's creator support features.
    /// </summary>
    public bool AuthorPatreonFlair { get; set; }

    /// <summary>
    ///     The text color of the author's user flair. Either <c>"dark"</c> or <c>"light"</c>,
    ///     chosen to contrast with <see cref="AuthorFlairBackgroundColor" />.
    ///     Null when the author has no flair.
    /// </summary>
    public string? AuthorFlairTextColor { get; set; }

    /// <summary>
    ///     The relative permalink path to this post on Reddit.
    ///     Example: <c>"/r/confession/comments/1rmlohf/car_dealer_never_cashed_7500_down_payment_check_i/"</c>.
    ///     Prepend <c>"https://www.reddit.com"</c> for the full URL.
    /// </summary>
    public string Permalink { get; set; } = string.Empty;

    /// <summary>
    ///     Whether the post is stickied (pinned) to the top of the subreddit by a moderator.
    ///     Up to 2 posts can be stickied at a time in a subreddit.
    /// </summary>
    public bool Stickied { get; set; }

    /// <summary>
    ///     The full URL of the post. For self posts this is the Reddit permalink.
    ///     For link posts this is the external URL being linked to.
    ///     Example: <c>"https://www.reddit.com/r/confession/comments/1rmlohf/..."</c>.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    ///     The total number of subscribers to the subreddit where this post was made.
    ///     Example: <c>11779221</c>.
    /// </summary>
    public long SubredditSubscribers { get; set; }

    /// <summary>
    ///     Unix epoch timestamp (in seconds, UTC) of when the post was created.
    ///     Example: <c>1772821114.0</c>. Use this for consistent time handling across time zones.
    /// </summary>
    public double CreatedUtc { get; set; }

    /// <summary>
    ///     The number of times this post has been crossposted to other subreddits.
    ///     Example: <c>0</c>.
    /// </summary>
    public int NumCrossposts { get; set; }

    /// <summary>
    ///     Media object for link posts that point to recognized media providers (YouTube, Imgur, etc.).
    ///     Contains provider name, embed HTML, thumbnail, and dimensions.
    ///     Null for self posts or unrecognized media links.
    /// </summary>
    public JsonElement? Media { get; set; }

    /// <summary>
    ///     Whether the post is a Reddit-hosted video (v.redd.it).
    ///     <c>true</c> for native Reddit video posts; <c>false</c> for all other post types.
    /// </summary>
    public bool IsVideo { get; set; }
}