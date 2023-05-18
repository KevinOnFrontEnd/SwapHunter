using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SwapHunter.Client;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Nodes;

namespace SwapHunter.Worker
{
  public class SwapHunterService : BackgroundService
  {
    private ITibetClient _tibetClient { get; set; }
    private IOptions<TibetSwapOptions> _tibetOptions { get; set; }
    private IConfiguration _config;

    public SwapHunterService(ITibetClient tibetclient, IOptions<TibetSwapOptions> tibetOptions, IConfiguration config)
    {
      _tibetClient = tibetclient;
      _tibetOptions = tibetOptions;
      _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      try
      {
        //known token pairs
        var knownTokens = await GetLatestTokenPairGistFromGitHub();

        while (true)
        {
          //fetch token pairs listed on the api
          var tokenPairs = await _tibetClient.GetTokenPairs();

          //union knownTokens from github gist & tibetswap api tokens and take
          //differences.
          var newTokenPairs = tokenPairs.Where(x=> !tokenPairs.Any(y=>y.pair_id == x.pair_id)).ToList();
          foreach(var pair in newTokenPairs)
          {
            Console.Beep();
            Console.WriteLine($"New Pairs Detected");
            Console.WriteLine($"{pair.short_name} - {pair.name}");
          }

          //TODO:
          //DETERMINE IF Token is worth buying (WhiteList of names? Supply?
          //Get From TibetSwapEndpoint for price (https://api.v2.tibetswap.io/pair/{pair}
          //Post an amount to TibetSwap (https://api.v2.tibetswap.io/quote/8a47627f50869b310229455a7ed984c8384380ab810bd8ff4df3a6aded469c7d?amount_in=1000000000000&xch_is_input=true&estimate_fee=true)
          //Generate Offer File (locally using chia wallet make_offer - location varies between OSes)
          //Post Content of offer file to (https://api.v2.tibetswap.io/offer/{quoteid})

          foreach (var pair in tokenPairs)
          {
            Console.WriteLine($"{pair.short_name} - {pair.name}");
          }

          //wait 10 mins arbitrary amount time before trying again. This is configurable.
          await Task.Delay(_tibetOptions.Value.TokenRefreshDelay);
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
      }
    }

    /// <summary>
    /// This method fetches the latest gist from github. It is used as the known list of tokens - 
    /// When there are subsequent tokens found when fetching from tibetswap - then they are new.
    /// </summary>
    /// <returns></returns>
    private async Task<List<TokensPair>> GetLatestTokenPairGistFromGitHub()
    {
      var client = new HttpClient();
      client.BaseAddress = new Uri("https://gist.githubusercontent.com/");
      var response = await client.GetAsync(_config["LatestTokenPairGist"]);
      response.EnsureSuccessStatusCode();
      string responseBody = await response.Content.ReadAsStringAsync();
      var obj = JsonObject.Parse(responseBody);
      var pairs = JArray.Parse(obj.ToString());
      return pairs.ToObject<List<TokensPair>>();
    }
  }
}
