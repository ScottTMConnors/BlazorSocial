using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace BlazorSocial.Tests.Infrastructure;

public class TestWebAppFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _keepAliveConnection =
        new("DataSource=BlazorSocialTest;Mode=Memory;Cache=Shared");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _keepAliveConnection.Open();
        builder.UseEnvironment("Testing");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _keepAliveConnection.Close();
            _keepAliveConnection.Dispose();
        }
        base.Dispose(disposing);
    }
}
