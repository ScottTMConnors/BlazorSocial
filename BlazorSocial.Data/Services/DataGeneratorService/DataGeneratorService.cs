using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BlazorSocial.Data.Entities;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using NameGenerator.Generators;

namespace BlazorSocial.Data.Services;

public class DataGeneratorService(IDbContextFactory<ContentDbContext> DbContextFactory)
{
    private readonly ConcurrentBag<Comment> _commentList = [];

    private readonly DateTime _endDate = DateTime.Now;
    private readonly int _maxComment = 999;
    private readonly int _maxContent = 3999;

    private readonly int _maxTitle = 100;

    private readonly ConcurrentBag<PostMetadata> _postMetaList = [];

    private readonly ConcurrentBag<View> _viewList = [];

    private readonly ConcurrentBag<Vote> _voteList = [];

    private readonly bool UseReddit = true;
    private List<Post> PostList { get; set; } = [];

    private List<SocialUser> UserList { get; set; } = [];
    public int PostCount { get; private set; }

    public async Task GenerateData(int numberOfPosts, int numberOfUsers, int numberOfInteractions)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        PostCount = dbContext.Posts.Count();

        // Generate random users
        UserList = GenerateUsers(numberOfUsers);

        //Generate random posts
        if (UseReddit)
        {
            PostList = await GeneratePostsFromReddit(numberOfInteractions);
        }
        else
        {
            PostList = GeneratePosts(numberOfPosts, numberOfInteractions);
        }

        // Generate User/Post views and votes


        Debug.WriteLine("Inserting into db");
        await dbContext.BulkInsertAsync(UserList);
        await dbContext.BulkInsertAsync(PostList);
        await dbContext.BulkInsertAsync(_postMetaList.ToList());
        await dbContext.BulkInsertAsync(_viewList.ToList());
        await dbContext.BulkInsertAsync(_voteList.ToList());
        await dbContext.BulkInsertAsync(_commentList.ToList());
        await dbContext.UpdatePostMetadataAsync();
        PostCount = dbContext.Posts.Count();
        UserList.Clear();
        PostList.Clear();
        _postMetaList.Clear();
        _viewList.Clear();
        _voteList.Clear();
        _commentList.Clear();
        GC.Collect();
    }

    private static List<SocialUser> GenerateUsers(int numberOfUsers)
    {
        var generatedUsers = new ConcurrentBag<SocialUser>();
        Parallel.For(0, numberOfUsers, i =>
        {
            var random = new Random();
            var generator = new RealNameGenerator
            {
                SpaceCharacter = ""
            };
            var userNumber = $"{random.Next(99):00}";
            var username = generator.Generate();
            username = string.Concat(username, userNumber);
            var userId = UserId.New();
            // Assemble the content user object
            var generatedUser = new SocialUser(userId)
            {
                UserName = username,
                DisplayName = username
            };
            generatedUsers.Add(generatedUser);
        });

        return generatedUsers.ToList();
    }

    private List<Post> GeneratePosts(int numberOfPosts, int numberOfInteractions)
    {
        Debug.WriteLine($"Generating {numberOfPosts} posts");

        var GeneratedPosts = new ConcurrentBag<Post>();

        Parallel.For(0, numberOfPosts, async i =>
        {
            var random = new Random();

            var randomTitletask = GenerateRandomContent(_maxTitle);
            var randomContenttask = GenerateRandomContent(_maxContent);
            var startDate = new DateTime(2020, 1, 1);
            var userId = UserList[random.Next(UserList.Count)].Id;
            var postId = PostId.New();

            var range = (_endDate - startDate).Days;
            var randomDate = startDate.AddDays(random.Next(range)).AddHours(random.Next(DateTime.Now.Hour))
                .AddMinutes(random.Next(DateTime.Now.Minute)).AddSeconds(random.Next(DateTime.Now.Second));


            var newPost = new Post(string.Empty, string.Empty, userId, randomDate, PostType.Text);

            var viewvoteTask = GenerateViewsandVotes(newPost, numberOfInteractions);


            await Task.WhenAll(randomTitletask, randomContenttask, viewvoteTask);

            //Build Title
            var randomTitle = randomTitletask.Result;
            newPost.Title = randomTitle;

            //Content
            var randomContent = randomContenttask.Result;

            newPost.Content = randomContent;


            GeneratedPosts.Add(newPost);

            var newMetadate = new PostMetadata(postId);

            _postMetaList.Add(newMetadate);
        });

        return GeneratedPosts.ToList();
    }

    private async Task<List<Post>> GeneratePostsFromReddit(int numberOfInteractions)
    {
        var subreddit = "confession";
        var url = $"https://www.reddit.com/r/{Uri.EscapeDataString(subreddit)}/hot.json?limit=100";

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("BlazorSocial", "1.0"));

        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var listing = JsonSerializer.Deserialize<RedditData>(json, options)
                      ?? throw new InvalidOperationException("Failed to deserialize Reddit listing.");

        var generatedPosts = new List<Post>();
        var random = new Random();

        foreach (var child in listing.Data.Children)
        {
            var data = child.Data;

            var title = data.Title;
            if (title.Length > _maxTitle)
            {
                title = title[.._maxTitle];
            }

            var content = data.Selftext;
            if (content.Length > _maxContent)
            {
                content = content[.._maxContent];
            }

            var postDate = DateTimeOffset.FromUnixTimeSeconds((long)data.CreatedUtc).LocalDateTime;

            var postType = data.IsVideo ? PostType.Video
                : data.IsSelf ? PostType.Text
                : PostType.Link;

            var userId = UserList[random.Next(UserList.Count)].Id;

            var newPost = new Post(title, content, userId, postDate, postType);

            _ = GenerateViewsandVotes(newPost, numberOfInteractions);

            generatedPosts.Add(newPost);

            var newMetadata = new PostMetadata(newPost.Id);
            _postMetaList.Add(newMetadata);
        }

        return generatedPosts;
    }

    private Task GenerateViewsandVotes(Post post, int numberOfInteractions)
    {
        ICollection<UserId> UserIdsInteracted = new List<UserId>();
        var random = new Random();
        var userCount = UserList.Count();
        var numSeed = numberOfInteractions;
        if (numSeed > userCount)
        {
            numSeed = userCount;
        }

        var numofInteractions = random.Next(1, numSeed);

        UserIdsInteracted.Add(post.AuthorId!);
        var listCount = 1;
        for (var i = 0; i < numofInteractions; i++)
        {
            var userId = UserList
                .Where(u => UserIdsInteracted.All(u2 => u2 != u.Id))
                .ElementAt(random.Next(userCount - listCount))
                .Id;
            UserIdsInteracted.Add(userId);
            listCount++;

            var startDate = post.PostDate;
            var range = (_endDate - startDate).Days;
            var randomDate = startDate.AddDays(random.Next(range)).AddHours(random.Next(DateTime.Now.Hour))
                .AddMinutes(random.Next(DateTime.Now.Minute)).AddSeconds(random.Next(DateTime.Now.Second));


            var GeneratedView = new View
            {
                PostId = post.Id,
                UserId = userId,
                ViewDateTime = randomDate
            };
            _viewList.Add(GeneratedView);

            if (random.Next() > int.MaxValue / 2)
            {
                var upVote = false;
                randomDate = startDate.AddDays(random.Next(range)).AddHours(random.Next(DateTime.Now.Hour))
                    .AddMinutes(random.Next(DateTime.Now.Minute)).AddSeconds(random.Next(DateTime.Now.Second));

                if (random.Next() > int.MaxValue / 2)
                {
                    upVote = true;
                }

                var GeneratedVote = new Vote
                {
                    PostId = post.Id,
                    UserId = userId,
                    IsUpvote = upVote,
                    VoteDate = randomDate,
                    IsActive = true
                };

                _voteList.Add(GeneratedVote);
            }

            if (random.Next() > int.MaxValue / 2)
            {
                randomDate = startDate.AddDays(random.Next(range)).AddHours(random.Next(DateTime.Now.Hour))
                    .AddMinutes(random.Next(DateTime.Now.Minute)).AddSeconds(random.Next(DateTime.Now.Second));

                var commentContent = GenerateRandomContent(_maxComment).Result;

                var commentId = CommentId.New();
                var GeneratedComment = new Comment
                {
                    Id = commentId,
                    PostId = post.Id,
                    AuthorID = userId,
                    PostDate = randomDate,
                    Content = commentContent
                };
                _commentList.Add(GeneratedComment);
            }
        }

        return Task.CompletedTask;
    }

    private Task<string> GenerateRandomContent(int maxChars)
    {
        var random = new Random();
        string[] words =
        [
            "lorem", "ipsum", "dolor", "sit", "amet", "consectetur", "adipiscing", "elit", "sed", "do", "eiusmod",
            "tempor", "incididunt"
        ];
        var post = new StringBuilder();
        var currentLength = 0;

        while (currentLength <= maxChars)
        {
            var sentenceLength = random.Next(3, 10);
            for (var i = 0; i < sentenceLength; i++)
            {
                post.Append(words[random.Next(words.Length)] + " ");
            }

            currentLength = post.Length;
        }

        return Task.FromResult(post.ToString()[..Math.Min(post.Length, maxChars)]);
    }
}
