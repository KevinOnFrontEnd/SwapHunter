using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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

          //Do something meaningful when new token is detected
          //Looking into either Generating an offer file

          foreach (var pair in tokenPairs)
          {
            Console.WriteLine($"{pair.short_name} - {pair.name}");
          }

          //wait arbitrary amount time so that we don't hammer tibetswaps api =)
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
