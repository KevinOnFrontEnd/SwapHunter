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

    public TibetClient(IOptions<TibetSwapOptions> options) 
    { 
      _options = options;
    }

    public async Task<List<TokensPair>> GetTokenPairs()
    {
      var client = new HttpClient();
      client.BaseAddress = new Uri(_options.Value.ApiEndpoint);
      var response = await client.GetAsync(_options.Value.TokensEndpoint);
      response.EnsureSuccessStatusCode();
      string responseBody = await response.Content.ReadAsStringAsync();
      List<TokensPair> pairs = JsonConvert.DeserializeObject<List<TokensPair>>(responseBody);
      return pairs;
    }
  }
}
