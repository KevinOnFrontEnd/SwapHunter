using SwapHunter.Client.Chia;
using SwapHunter.Client.TibetSwap;

namespace SwapHunter;

public interface IOfferService
{
    Task<Tuple<QuoteResponse, CreateOfferResponse, OfferResponse>> CreateOffer(double xch_amount, string assetId);
}