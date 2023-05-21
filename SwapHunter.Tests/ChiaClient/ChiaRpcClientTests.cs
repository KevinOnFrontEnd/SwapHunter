using Xunit;

namespace SwapHunter.Tests;

public class ChiaRpcClientTests  : SwapHunterTestBase
{
    public ChiaRpcClientTests(SwapHunterFixture fixture) : base(fixture)
    {
    }
    
    [Fact(Skip="Skipping until ChiaWalletClient.CreateOffer is bottomed out")]
    public async Task creating_chia_offer_returns_success()
    {
        // arrange
        var assetId = "d82dd03f8a9ad2f84353cd953c4de6b21dbaaf7de3ba3f4ddd9abe31ecba80ad"; //hard coded dbx assetid
        
        // act
        var result = await ChiaWalletClient.CreateOffer(assetId, 1500,0.1, 1000);
        
        // assert
        Assert.True(result.Success);
        Assert.Empty(result.Error);
    }
}