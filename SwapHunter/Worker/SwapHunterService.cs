using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using SwapHunter.Client;
using System.Text.Json.Nodes;

namespace SwapHunter.Worker
{
  public class SwapHunterService : BackgroundService
  {
    private ITibetClient _tibetClient { get; set; }
    private IOptions<TibetSwapOptions> _tibetOptions { get; set; }
    private IConfiguration _config;
    private IChiaRpcClient _chiaRpcClient { get; set; }

    public SwapHunterService(ITibetClient tibetclient, IOptions<TibetSwapOptions> tibetOptions, IConfiguration config, IChiaRpcClient chiaRpcClient)
    {
      _tibetClient = tibetclient;
      _tibetOptions = tibetOptions;
      _config = config;
      _chiaRpcClient = chiaRpcClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      try
      {
        //known token pairs
        var knownTokens = await GetLatestTokenPairGistFromGitHub();
        var knownTokenshs = knownTokens.ToDictionary(x => x.pair_id);

        while (true)
        {
          //fetch token pairs listed on the api
          var tokenPairs = await _tibetClient.GetTokenPairs();
          var apiPairs = tokenPairs.ToDictionary(x=>x.pair_id);

          var newPairs = GetNewPairs(knownTokenshs, apiPairs);
          foreach(var pair in newPairs)
          {
            Console.Beep();
            Console.WriteLine($"New Pairs Detected");
            Console.WriteLine($"{pair.short_name} - {pair.name}");
            
            //get quote
            
            var offer = await _chiaRpcClient.CreateOffer(pair.Asset_id, 100, 1, 100);
            if (offer.Success)
            {
              //either print to console/save file or post to tibetswap api
            }
          }

          //TODO:
          //DETERMINE IF Token is worth buying (WhiteList of names? Supply?
          //Post Content of offer file to (https://api.v2.tibetswap.io/offer/{quoteid})
          
          //wait 10 mins arbitrary amount time before trying again. This is configurable.
          await Task.Delay(_tibetOptions.Value.TokenRefreshDelay);
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
      }
    }


    private List<Token> GetNewPairs(Dictionary<string, Token> knownPairs, Dictionary<string, Token> apiPairs)
    {
      var newPairs = new List<Token>();
      foreach (var item in apiPairs)
      {
        if (!knownPairs.ContainsKey(item.Key))
          newPairs.Add(item.Value);
      }
      return newPairs;
    }

    /// <summary>
    /// This method fetches the latest gist from github. It is used as the known list of tokens - 
    /// When there are subsequent tokens found when fetching from tibetswap - then they are new.
    /// </summary>
    /// <returns></returns>
    private async Task<List<Token>> GetLatestTokenPairGistFromGitHub()
    {
      var client = new HttpClient();
      client.BaseAddress = new Uri("https://gist.githubusercontent.com/");
      var response = await client.GetAsync(_config["LatestTokenPairGist"]);
      response.EnsureSuccessStatusCode();
      string responseBody = await response.Content.ReadAsStringAsync();
      var obj = JsonObject.Parse(responseBody);
      var pairs = JArray.Parse(obj.ToString());
      return pairs.ToObject<List<Token>>();
    }
  }
}
