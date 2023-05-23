using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using SwapHunter.Client;
using System.Text.Json.Nodes;
using SwapHunter.Client.Chia;
using SwapHunter.Client.TibetSwap;

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
            
            //TODO: Determine if this token is worth getting
            
            //TBC
            //intetionally can't get to this block of code until
            //logic has been decided on which tokens/token amount has been decided.
            if (false)
            {
              var quote = await _tibetClient.GetQuote(pair.pair_id, 1000);
              if (quote != null)
              {
                var requestingTokenAmount = 100.0;
                var xchAmount = ChiaHelper.ConvertToMojos(0.01);
                var fee = ChiaHelper.ConvertToMojos(0.0001); //higher fee = faster transaction
                var offer = await _chiaRpcClient.CreateOffer(pair.Asset_id, requestingTokenAmount, xchAmount, fee,false);
                if (offer.Success)
                {
                  //TODO:
                  //Post Content of offer file to (https://api.v2.tibetswap.io/offer/{quoteid})
                }
              }
            }
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
    
    private List<TokenResponse> GetNewPairs(Dictionary<string, TokenResponse> knownPairs, Dictionary<string, TokenResponse> apiPairs)
    {
      var newPairs = new List<TokenResponse>();
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
    private async Task<List<TokenResponse>> GetLatestTokenPairGistFromGitHub()
    {
      var client = new HttpClient();
      client.BaseAddress = new Uri("https://gist.githubusercontent.com/");
      var response = await client.GetAsync(_config["LatestTokenPairGist"]);
      response.EnsureSuccessStatusCode();
      string responseBody = await response.Content.ReadAsStringAsync();
      var obj = JsonObject.Parse(responseBody);
      var pairs = JArray.Parse(obj.ToString());
      return pairs.ToObject<List<TokenResponse>>();
    }
  }
}
