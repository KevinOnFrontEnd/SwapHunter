using SwapHunter.Client.Chia;
using Tibby.Models;

namespace SwapHunter;

public interface IOfferService
{
    Task<Tuple<QuoteResponse, CreateOfferResponse, OfferResponse>> CreateOffer(double xch_amount, string assetId);
}