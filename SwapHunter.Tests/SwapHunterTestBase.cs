using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SwapHunter.Client;
using Xunit;

namespace SwapHunter.Tests;

[Collection("Integration")]
public abstract class SwapHunterTestBase : IAsyncLifetime, IDisposable
{
    public SwapHunterTestBase(SwapHunterFixture fixture)
    {
        Fixture = fixture;
    }
    
    public SwapHunterFixture Fixture { get; private set; }

    public ITibetClient TibetClient => Fixture.Services.GetService<ITibetClient>();
    
    public IChiaRpcClient ChiaWalletClient => Fixture.Services.GetService<IChiaRpcClient>();
     
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }


}