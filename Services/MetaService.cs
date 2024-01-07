using BlazorSocial.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Timers;

namespace BlazorSocial.Services {
    public class MetaService {
        //TODO
        //CREATE SERVICE THAT CONTINUALLY UPDATES VOTE COUNT

        // CREATE SERVICE THAT DESTROYS OLD VOTES

        private readonly int TimerInterval = 1; //Timer interval in min

        private readonly IServiceProvider _serviceProvider;

        private string sqlCommand = @"EXEC UpdateMetadata;";
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
