using System.Diagnostics;
using SwapHunter.Client.Chia;
using SwapHunter.Client.TibetSwap;

namespace SwapHunter;

public class OfferService : IOfferService
{
    private ITibetClient _tibetClient { get; set; }
    private IChiaRpcClient _chiaRpcClient { get; set; }

    public OfferService(ITibetClient tibetClient, IChiaRpcClient chiaClient)
    {
        _tibetClient = tibetClient;
        _chiaRpcClient = chiaClient;
    }


    /// <summary>
    /// This function wraps up the api calls to do an swap on tibetswap.
    ///
    /// This method will use the xch_amount in chia and convert to  mojos, request a quote from
    /// Tibetwap, calculate the amount of assetId you will get in return and create an offer in xch. the generated
    /// offer is then posted to tibetswap.
    /// </summary>
    /// <param name="xch_amount">The amount of chia to offer</param>
    /// <param name="assetId"></param>
    /// <returns></returns>
    public async Task<Tuple<QuoteResponse, CreateOfferResponse, OfferResponse>> CreateOffer(double xch_amount,
        string assetId)
    {
        //convert requesting amount to mojos
        var requesting_xch_in_mojos = ChiaHelper.ConvertToMojos(xch_amount);

        // fetch token from tibetswap to get assetid
        var token = await _tibetClient.GetToken(assetId);
        Debug.Assert(token != null);

        // Generate a quote
        var quote = await _tibetClient.GetQuote(token.pair_id, requesting_xch_in_mojos);
        Debug.Assert(quote != null);

        // calculate token amount for lowest chia amount
        var tokenOutput = TibetHelper.GetInputPrice(requesting_xch_in_mojos, quote.input_reserve, quote.output_reserve);
        var lowestxchAmount = TibetHelper.getOutputPrice(quote.amount_out, quote.input_reserve, quote.output_reserve);

        //add dev fee of 0.003 to offer
        var offering_amount = Math.Floor(lowestxchAmount * 1.003); //input + dev fee
        var donationFee = lowestxchAmount * 0.003; //donationAmount

        // create chia offer
        var chia_offer = await _chiaRpcClient.CreateOffer(assetId, tokenOutput, offering_amount, quote.fee, false);
        Debug.Assert(chia_offer.Success == true);

        // send generated quote to tibetswap for exchange
        var swap = await _tibetClient.PostOffer(token.pair_id, chia_offer.Offer, donationFee);
        Debug.Assert(swap.Success == true);
        
        return Tuple.Create(quote, chia_offer, swap);
    }
}