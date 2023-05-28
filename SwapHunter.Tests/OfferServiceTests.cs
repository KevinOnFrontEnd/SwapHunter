using Microsoft.Extensions.Hosting;
using SwapHunter.Client.TibetSwap;
using SwapHunter.Tests.Core;
using Xunit;

namespace SwapHunter.Tests;

public class OfferServiceTests : SwapHunterTestBase
{
    public OfferServiceTests(SwapHunterFixture baseTest):base(baseTest)
    {
    }

    [Fact]
    public async Task create_offer_and_post_to_tibetswap_succeeds()
    {
        // arrange
        var assetId = "d82dd03f8a9ad2f84353cd953c4de6b21dbaaf7de3ba3f4ddd9abe31ecba80ad"; //dbx
        
        // act
        var (quote, chia_offer, swap) = await OfferService.CreateOffer(0.001, assetId);
        
        // assert
        Assert.NotNull(quote);
        Assert.NotNull(chia_offer);
        Assert.True(chia_offer.Success);
        Assert.NotNull(swap);
        Assert.Equal("{\"status\": \"SUCCESS\", \"success\": true}",swap.Message);
        Assert.True(swap.Success);
    }
    
    // [Fact]
    // public async Task create_and_post_offer_succeeds()
    // {
    //     // arrange
    //     var assetId = "d82dd03f8a9ad2f84353cd953c4de6b21dbaaf7de3ba3f4ddd9abe31ecba80ad"; //dbx
    //     var pairId = "3f2b9ddb0d90b81f43d86870d31b505df5dd377f4215928a436bc51d9d613294"; //dbx-txch pair
    //     var requesting_xch = 0.001;
    //     var requesting_xch_in_mojos = ChiaHelper.ConvertToMojos(requesting_xch);
    //
    //     // act
    //     var quote = await TibetClient.GetQuote(pairId, requesting_xch_in_mojos);
    //     var tokenOutput = TibetHelper.GetInputPrice(requesting_xch_in_mojos, quote.input_reserve, quote.output_reserve);
    //     var lowestxchAmount = TibetHelper.getOutputPrice(quote.amount_out, quote.input_reserve, quote.output_reserve);
    //     
    //     var offering_amount = Math.Floor(lowestxchAmount * 1.003); //input + dev fee
    //     var donationFee = lowestxchAmount * 0.003; //donationAmount
    //     var chia_offer = await ChiaWalletClient.CreateOffer(assetId, tokenOutput, offering_amount, quote.fee, false);
    //     var swap = await TibetClient.PostOffer(pairId, chia_offer.Offer, donationFee);
    //
    //     // assert
    //     Assert.NotNull(quote);
    //     Assert.NotNull(chia_offer);
    //     Assert.True(chia_offer.Success);
    //     Assert.NotNull(swap);
    //     Assert.Equal("{\"status\": \"SUCCESS\", \"success\": true}",swap.Message);
    //     Assert.True(swap.Success);
    // }
}