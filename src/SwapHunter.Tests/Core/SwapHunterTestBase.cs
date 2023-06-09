using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SwapHunter.Client;
using SwapHunter.Client.Chia;
using SwapHunter.Worker;
using Tibby;
using Xunit;

namespace SwapHunter.Tests.Core;

[Collection("Integration")]
public class SwapHunterTestBase : IClassFixture<SwapHunterFixture>
{
    public SwapHunterFixture Fixture { get; }
    public ITibbyClient TibetClient => Fixture.TestHost.Services.GetService<ITibbyClient>();
    
    public IChiaRpcClient ChiaWalletClient => Fixture.TestHost.Services.GetService<IChiaRpcClient>();
    
    public IOfferService OfferService => Fixture.TestHost.Services.GetService<IOfferService>();

    
    public SwapHunterTestBase(SwapHunterFixture fixture)
    {
        Fixture = fixture;
    }
}