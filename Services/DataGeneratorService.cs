using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using BlazorSocial.Data;
using BlazorSocial.Data.Entities;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using NameGenerator.Generators;

namespace BlazorSocial.Services;

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
        PostList = GeneratePosts(numberOfPosts, numberOfInteractions);

        // Generate User/Post views and votes


        Debug.WriteLine("Inserting into db");
        await dbContext.BulkInsertAsync(UserList);
        await dbContext.BulkInsertAsync(PostList);
        await dbContext.BulkInsertAsync(_postMetaList.ToList());
        await dbContext.BulkInsertAsync(_viewList.ToList());
        await dbContext.BulkInsertAsync(_voteList.ToList());
        await dbContext.BulkInsertAsync(_commentList.ToList());
        await dbContext.UpdatePostMetadataAsync();
        // DbContext.Posts.AddRange(GeneratedPosts);
        // await DbContext.SaveChangesAsync();
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
            var generatedUser = new SocialUser
            {
                Id = userId,
                UserName = username,
                NormalizedUserName = username.ToUpper(),
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
            //for (int i = 0; i < 100000; i++) {
            // Generate a random title and content
            var random = new Random();

            var randomTitletask =
                GenerateRandomContent(_maxTitle); // Assuming title length between 10 and 20 characters
            var randomContenttask = GenerateRandomContent(_maxContent); // Content length between 5 and 100 characters
            var startDate = new DateTime(2020, 1, 1);
            var userId = UserList[random.Next(UserList.Count)].Id;
            var postId = PostId.New();

            var range = (_endDate - startDate).Days;
            var randomDate = startDate.AddDays(random.Next(range)).AddHours(random.Next(DateTime.Now.Hour))
                .AddMinutes(random.Next(DateTime.Now.Minute)).AddSeconds(random.Next(DateTime.Now.Second));


            var newPost = new Post(string.Empty, string.Empty, userId, randomDate);

            var viewvoteTask = GenerateViewsandVotes(newPost, numberOfInteractions);


            await Task.WhenAll(randomTitletask, randomContenttask, viewvoteTask);

            //Build Title
            var randomTitle = randomTitletask.Result;
            newPost.Title = randomTitle;

            //Content
            var randomContent = randomContenttask.Result;

            newPost.Content = randomContent;


            GeneratedPosts.Add(newPost);

            //Create the metadata object

            var newMetadate = new PostMetadata(postId);

            _postMetaList.Add(newMetadate);
        });

        return GeneratedPosts.ToList();
    }

    private Task GenerateViewsandVotes(Post post, int numberOfInteractions)
    {
        ICollection<UserId> UserIdsInteracted = new List<UserId>();
        // Get a random number of users to interact with this post
        var random = new Random();
        var userCount = UserList.Count();
        var numSeed = numberOfInteractions;
        if (numSeed > userCount)
        {
            numSeed = userCount;
        }

        var numofInteractions = random.Next(1, numSeed);
        // For ever number of users, pick a user that is not the post user and hasnt interacted with this post yet (Will probably be resource intensive)

        UserIdsInteracted.Add(post.AuthorId!);
        var listCount = 1; //Start at 1 to account for the post author
        for (var i = 0; i < numofInteractions; i++)
        {
            // Pick a random user in the userlist
            var userId = UserList
                .Where(u => UserIdsInteracted.All(u2 => u2 != u.Id))
                .ElementAt(random.Next(userCount - listCount))
                .Id;
            UserIdsInteracted.Add(userId);
            listCount++;
            // Create a view object using the userId and the postId

            // Create a random view date

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

            // Randomly determine if a vote will be made

            if (random.Next() > int.MaxValue / 2)
            {
                // Create a vote objecte
                var upVote = false;
                randomDate = startDate.AddDays(random.Next(range)).AddHours(random.Next(DateTime.Now.Hour))
                    .AddMinutes(random.Next(DateTime.Now.Minute)).AddSeconds(random.Next(DateTime.Now.Second));

                //Randomly determine if will be an upvote of downvote
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
                    IsActive = true //Redundant because this field initializes to true
                };

                _voteList.Add(GeneratedVote);
            }


            // TODO, add comments
            // Randomly determine if a comment will be made

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
            // Generate a pseudo-sentence
            var sentenceLength = random.Next(3, 10); // Random length of the sentence
            for (var i = 0; i < sentenceLength; i++)
            {
                post.Append(words[random.Next(words.Length)] + " ");
            }

            // Apply random formatting
            // switch (random.Next(3)) {
            //     case 0: post.Insert(currentLength, "<b>"); post.Append("</b> "); break;
            //     case 1: post.Insert(currentLength, "<i>"); post.Append("</i> "); break;
            //     case 2: post.Append("<br />"); break;
            // }

            currentLength = post.Length;
        }

        // Ensure the post is not longer than maxChars
        return Task.FromResult(post.ToString()[..Math.Min(post.Length, maxChars)]);
    }
}