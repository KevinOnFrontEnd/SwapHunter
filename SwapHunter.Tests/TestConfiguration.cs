using Microsoft.Extensions.Configuration;

namespace SwapHunter.Tests;

public class TestConfiguration
{
    public IConfiguration Configuration => new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("testingappsettings.json")
        .AddUserSecrets<SwapHunterFixture>(optional: true)
        .AddEnvironmentVariables()
        .Build();
}