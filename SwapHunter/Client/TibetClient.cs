using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SwapHunter.Client
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

    public async Task<TokenPair> GetPair(string pair)
    {
      var response = await _client.GetAsync($"{_options.Value.TokenPairEndpoint}/{pair}");
      response.EnsureSuccessStatusCode();
      string responseBody = await response.Content.ReadAsStringAsync();
      var item = JsonConvert.DeserializeObject<TokenPair>(responseBody);
      return item;
    }

    public async Task<Quote> GetQuote(string pair, string amount_in, bool xch_is_input = true, bool estimate_fee = true)
    {
      var response = await _client.GetAsync($"{_options.Value.QuoteEndpoint}/{pair}?amount_in={amount_in}&xch_is_input={xch_is_input}&estimate_fee={estimate_fee}");
      response.EnsureSuccessStatusCode();
      string responseBody = await response.Content.ReadAsStringAsync();
      var quote = JsonConvert.DeserializeObject<Quote>(responseBody);
      return quote;
    }

    public async Task<List<Token>> GetTokenPairs()
    {
      var response = await _client.GetAsync(_options.Value.TokensEndpoint);
      response.EnsureSuccessStatusCode();
      string responseBody = await response.Content.ReadAsStringAsync();
      List<Token> pairs = JsonConvert.DeserializeObject<List<Token>>(responseBody);
      return pairs;
    }
  }
}
