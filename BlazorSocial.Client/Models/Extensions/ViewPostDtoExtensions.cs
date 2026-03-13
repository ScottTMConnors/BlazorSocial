namespace BlazorSocial.Client.Models.Extensions;

public static class ViewPostDtoExtensions
{
    extension(ViewPostDto Post)
    {
        public void VotePost(bool isUpvote)
        {
            if (isUpvote)
            {
                if (Post.CurrentUserVote == 1)
                {
                    // Remove upvote
                    Post.Score -= 1;
                    Post.CurrentUserVote = 0;
                }
                else
                {
                    if (Post.CurrentUserVote == -1)
                    {
                        // Remove downvote and add upvote
                        Post.Score += 2;
                    }
                    else
                    {
                        // Add upvote
                        Post.Score += 1;
                    }

                    Post.CurrentUserVote = 1;
                }
            }
            else
            {
                if (Post.CurrentUserVote == -1)
                {
                    // Remove downvote
                    Post.Score += 1;
                    Post.CurrentUserVote = 0;
                }
                else
                {
                    if (Post.CurrentUserVote == 1)
                    {
                        // Remove upvote and add downvote
                        Post.Score -= 2;
                    }
                    else
                    {
                        // Add downvote
                        Post.Score -= 1;
                    }

                    Post.CurrentUserVote = -1;
                }
            }
        }
    }
}