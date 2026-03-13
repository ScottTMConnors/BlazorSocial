using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("blazorsocial-sqldata")
    .AddDatabase("ContentDatabase");

builder.AddProject<BlazorSocial>("blazorsocial")
    .WithReference(sql)
    .WaitFor(sql);

builder.Build().Run();