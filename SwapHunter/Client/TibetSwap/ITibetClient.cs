using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwapHunter.Client.TibetSwap
{
  public interface ITibetClient
  {
    Task<List<TokenResponse>> GetTokenPairs();
    Task<TokenPairResponse> GetPair(string pair);
    Task<QuoteResponse> GetQuote(string pair, double amount_in, bool xch_is_input = true, bool estimate_fee = true);
    Task<OfferResponse> PostOffer(string pairId, string offer, double donationAmount, string action="SWAP", string[] donationAddresses=null, string[] donationWeights=null);
    Task<RouterResponse> GetRouter();
    Task<TokenResponse> GetToken(string assetId);
  }
}
