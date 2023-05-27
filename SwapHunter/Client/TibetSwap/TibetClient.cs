using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SwapHunter.Client.TibetSwap
{
  public class TibetClient : ITibetClient
  {
    private IOptions<TibetSwapOptions> _options;
    private HttpClient _client { get; set; }

    public TibetClient(IOptions<TibetSwapOptions> options, HttpClient httpClient) 
    { 
      _options = options;
      _client = httpClient;
    }

    public async Task<TokenPairResponse> GetPair(string pair)
    {
      var response = await _client.GetAsync($"{_options.Value.TokenPairEndpoint}/{pair}");
      response.EnsureSuccessStatusCode();
      string responseBody = await response.Content.ReadAsStringAsync();
      var item = JsonConvert.DeserializeObject<TokenPairResponse>(responseBody);
      return item;
    }

    public async Task<QuoteResponse> GetQuote(string pair, double amount_in, bool xch_is_input = true, bool estimate_fee = true)
    {
      var response = await _client.GetAsync($"{_options.Value.QuoteEndpoint}/{pair}?amount_in={amount_in}&xch_is_input={xch_is_input}&estimate_fee={estimate_fee}");
      response.EnsureSuccessStatusCode();
      string responseBody = await response.Content.ReadAsStringAsync();
      var quote = JsonConvert.DeserializeObject<QuoteResponse>(responseBody);
      return quote;
    }

    public async Task<OfferResponse> PostOffer(string pairId, string offer, double donationAmount, string action = "SWAP",
      string[] donationAddresses = null, string[] donationWeights = null)
    {
      var postedOffer = new
      {
        offer = offer,
        action = "SWAP",
        total_donation_amount = (int) Math.Floor(donationAmount),
        donation_addresses= new []{"txch1hm6sk2ktgx3u527kp803ex2lten3xzl2tpjvrnc0affvx5upd6mqnn6lxh"},
        donation_weights = new []{1},
      };
      
      var json = JsonConvert.SerializeObject(postedOffer);
      var offerContent = JsonContent.Create(json); // use MediaTypeNames.Application.Json in Core 3.0+ and Standard 2.1+

      
      var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
      content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
      var response = await _client.PostAsync($"{_options.Value.OfferEndpoint}/{pairId}", content);
      response.EnsureSuccessStatusCode();
      string responseBody = await response.Content.ReadAsStringAsync();
      var swap = JsonConvert.DeserializeObject<OfferResponse>(responseBody);
      return swap;
    }

    public async Task<RouterResponse> GetRouter()
    {
      var response = await _client.GetAsync($"{_options.Value.RouterEndpoint}");
      response.EnsureSuccessStatusCode();
      string responseBody = await response.Content.ReadAsStringAsync();
      var router = JsonConvert.DeserializeObject<RouterResponse>(responseBody);
      return router;
    }

    public async Task<TokenResponse> GetToken(string assetId)
    {
      var response = await _client.GetAsync($"{_options.Value.TokenEndpoint}/{assetId}");
      response.EnsureSuccessStatusCode();
      string responseBody = await response.Content.ReadAsStringAsync();
      var token = JsonConvert.DeserializeObject<TokenResponse>(responseBody);
      return token;
    }

    public async Task<List<TokenResponse>> GetTokenPairs()
    {
      var response = await _client.GetAsync(_options.Value.TokensEndpoint);
      response.EnsureSuccessStatusCode();
      string responseBody = await response.Content.ReadAsStringAsync();
      List<TokenResponse> pairs = JsonConvert.DeserializeObject<List<TokenResponse>>(responseBody);
      return pairs;
    }
  }
}
