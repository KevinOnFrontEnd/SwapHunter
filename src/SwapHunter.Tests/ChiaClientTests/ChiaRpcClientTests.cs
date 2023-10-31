using SwapHunter.Tests.Core;
using Xunit;

namespace SwapHunter.Tests.ChiaClientTests;

public class ChiaRpcClientTests  : SwapHunterTestBase
{
    public ChiaRpcClientTests(SwapHunterFixture baseTest):base(baseTest)
    {
    }
    
    [Fact]
    public async Task creating_chia_offer_returns_success()
    {
        // arrange
        var assetId = "d82dd03f8a9ad2f84353cd953c4de6b21dbaaf7de3ba3f4ddd9abe31ecba80ad"; //hard coded dbx assetid
        var offering_xch_in_mojos = ChiaHelper.ConvertToMojos(0.001);
        
        // act
        var result = await ChiaWalletClient.CreateOffer(assetId, 1500, Convert.ToInt64(offering_xch_in_mojos), 1000, true);
        
        // assert
        //Assert.True(result.Success);
        Assert.Null(result.Error);
    }
    
    [Fact]
    public async Task create_offer_with_offering_more_chia_than_in_wallet_returns_error()
    {
        // arrange
        var offering_xch_in_mojos = ChiaHelper.ConvertToMojos(1000);
        var assetId = "d82dd03f8a9ad2f84353cd953c4de6b21dbaaf7de3ba3f4ddd9abe31ecba80ad"; //hard coded dbx assetid
        
        // act
        var result = await ChiaWalletClient.CreateOffer(assetId, 150000, Convert.ToInt64(offering_xch_in_mojos), 1000, true);
        
        // assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
    }
    
    [Fact]
    public async Task create_offer_for_dbx()
    {
        // arrange
        var offering_xch_in_mojos = ChiaHelper.ConvertToMojos(0.0010021469);
        var assetId = "d82dd03f8a9ad2f84353cd953c4de6b21dbaaf7de3ba3f4ddd9abe31ecba80ad"; //hard coded dbx assetid
        var minFee = ChiaHelper.ConvertToMojos(0.000122973515);
        
        // act
        var result = await ChiaWalletClient.CreateOffer(assetId, 617, Convert.ToInt64(offering_xch_in_mojos), Convert.ToInt64(minFee), true);
        
        // assert
        Assert.True(result.Success);
        Assert.Null(result.Error);
    }
}