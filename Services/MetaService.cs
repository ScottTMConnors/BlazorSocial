using BlazorSocial.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Timers;

namespace BlazorSocial.Services {
    public class MetaService {
        // TODO
        // CREATE SERVICE THAT CONTINUALLY UPDATES VOTE COUNT

        // CREATE SERVICE THAT DESTROYS OLD VOTES

        private readonly int TimerInterval = 1; //Timer interval in min

        private readonly IServiceProvider _serviceProvider;

        private string sqlCommand = @"
            WITH RecentViews AS (
                    SELECT
                        V.PostId,
                        V.UserId,
                        ROW_NUMBER() OVER (PARTITION BY V.PostId, V.UserId ORDER BY V.PostId DESC) as rn
                    FROM
                        dbo.Views V
                ),
                AggregatedViews AS (
                    SELECT
                        PostId,
                        COUNT(*) as ViewCount
                    FROM
                        RecentViews
                    WHERE
                        rn = 1
                    GROUP BY
                        PostId
                ),
                RecentVotes AS (
                    SELECT
                        Vo.PostId,
                        Vo.IsUpvote,
                        ROW_NUMBER() OVER (PARTITION BY Vo.PostId, Vo.UserId ORDER BY Vo.VoteDate DESC) as rn
                    FROM
                        dbo.Votes Vo
                    WHERE
                        Vo.IsActive = 1
                ),
                AggregatedVotes AS (
                    SELECT
                        PostId,
                        SUM(CASE WHEN IsUpvote = 1 THEN 1 ELSE 0 END) as Upvotes,
                        SUM(CASE WHEN IsUpvote = 0 THEN 1 ELSE 0 END) as Downvotes,
                        COUNT(*) as TotalVotes,
                        SUM(CASE WHEN IsUpvote = 1 THEN 1 ELSE -1 END) as NetVotes
                    FROM
                        RecentVotes
                    WHERE
                        rn = 1
                    GROUP BY
                        PostId
                ),
                CombinedData AS (
                    SELECT
                        P.Id as PostId,
                        COALESCE(AV.ViewCount, 0) as ViewCount,
                        COALESCE(AVo.Upvotes, 0) as Upvotes,
                        COALESCE(AVo.Downvotes, 0) as Downvotes,
                        COALESCE(AVo.TotalVotes, 0) as TotalVotes,
                        COALESCE(AVo.NetVotes, 0) as NetVotes
                    FROM
                        dbo.Posts P
                    LEFT JOIN
                        AggregatedViews AV ON P.Id = AV.PostId
                    LEFT JOIN
                        AggregatedVotes AVo ON P.Id = AVo.PostId
                )
                MERGE INTO dbo.PostMetadatas AS PM
                USING CombinedData AS CD
                ON PM.PostId = CD.PostId
                WHEN MATCHED THEN
                    UPDATE SET
                        PM.ViewCount = CD.ViewCount,
                        PM.Upvotes = CD.Upvotes,
                        PM.Downvotes = CD.Downvotes,
                        PM.TotalVotes = CD.TotalVotes,
		                PM.NetVotes = CD.NetVotes
                WHEN NOT MATCHED THEN
                    INSERT (PostId, ViewCount, Upvotes, Downvotes, TotalVotes)
                    VALUES (CD.PostId, CD.ViewCount, CD.Upvotes, CD.Downvotes, CD.TotalVotes);
                ";
        public MetaService(IServiceProvider serviceProvider) {

            Debug.WriteLine($"Metadata service started!");

            _serviceProvider = serviceProvider;

            // Create a timer
            System.Timers.Timer timer = new System.Timers.Timer((TimerInterval * 60000));

            timer.Elapsed += UpdatePostMeta;
            timer.AutoReset = true;
            timer.Enabled = true;

        }

        private void UpdatePostMeta(object? sender, ElapsedEventArgs e) {
             
            using (var scope = _serviceProvider.CreateScope()) {
                var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ContentDbContext>>();
                using (var context = dbContextFactory.CreateDbContext()) {
                    // Update metadata structures
                    context.Database.ExecuteSqlRaw(sqlCommand);
                }
            }
        }
    }
}
