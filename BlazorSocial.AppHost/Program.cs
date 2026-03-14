using Aspire.Hosting.ApplicationModel;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .AddDatabase("ContentDatabase");

builder.AddProject<BlazorSocial>("blazorsocial")
    .WithReference(sql)
    .WaitFor(sql)
    .WithUrls(context => context.Urls.Add(new ResourceUrlAnnotation
    {
        Url = $"{context.Urls[0].Url}/swagger",
        DisplayText = "Swagger",
        DisplayLocation = UrlDisplayLocation.SummaryAndDetails,
    }));

builder.Build().Run();