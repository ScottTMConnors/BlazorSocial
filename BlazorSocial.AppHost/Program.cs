using Aspire.Hosting.ApplicationModel;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql");
var authDb = sql.AddDatabase("AuthDatabase");
var contentDb = sql.AddDatabase("ContentDatabase");

var redis = builder.AddRedis("cache");

var jwtKey = builder.AddParameter("JwtSigningKey", secret: true);

var auth = builder.AddProject<BlazorSocial_Auth>("auth")
    .WithReference(authDb)
    .WaitFor(authDb)
    .WithEnvironment("Jwt__SigningKey", jwtKey);

var api = builder.AddProject<BlazorSocial_Api>("api")
    .WithReference(contentDb)
    .WithReference(redis)
    .WaitFor(contentDb)
    .WaitFor(redis)
    .WithReference(auth)
    .WithEnvironment("Jwt__SigningKey", jwtKey);

builder.AddProject<BlazorSocial_WebServer>("blazorsocial")
    .WithReference(auth)
    .WithReference(api)
    .WaitFor(auth)
    .WaitFor(api)
    .WithEnvironment("Jwt__SigningKey", jwtKey)
    .WithUrls(context => context.Urls.Add(new ResourceUrlAnnotation
    {
        Url = $"{context.Urls[0].Url}/swagger",
        DisplayText = "Swagger",
        DisplayLocation = UrlDisplayLocation.SummaryAndDetails,
    }));

builder.Build().Run();
