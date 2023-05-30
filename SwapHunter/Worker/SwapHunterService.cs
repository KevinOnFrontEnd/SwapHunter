using System.ComponentModel.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using SwapHunter.Client;
using System.Text.Json.Nodes;
using Spectre.Console;
using SwapHunter.Client.Chia;
using SwapHunter.Client.TibetSwap;

namespace SwapHunter.Worker
{
    /// <summary>
    /// SwapHunter Service that runs as a background service that deals with
    /// Fetching tokens from tibetswaps api and facilitates creating the offer
    /// for a swap.
    /// </summary>
    public class SwapHunterService : BackgroundService
    {
        private ITibetClient _tibetClient { get; set; }
        private IOptions<TibetSwapOptions> _tibetOptions { get; set; }
        private IConfiguration _config;
        private IChiaRpcClient _chiaRpcClient { get; set; }
        private IOfferService _offerService { get; set; }

        public SwapHunterService(ITibetClient tibetclient, IOptions<TibetSwapOptions> tibetOptions,
            IConfiguration config, IChiaRpcClient chiaRpcClient, IOfferService offerService)
        {
            _tibetClient = tibetclient;
            _tibetOptions = tibetOptions;
            _config = config;
            _chiaRpcClient = chiaRpcClient;
            _offerService = offerService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                //known token pairs
                var knownTokens = await GetLatestTokenPairGistFromGitHub();
                var knownTokenshs = knownTokens.ToDictionary(x => x.pair_id);
                var tokenPairs = await _tibetClient.GetTokenPairs();
                var apiPairs = tokenPairs.ToDictionary(x => x.pair_id);
                var swapChoices = apiPairs.Select(x => x.Value.pair_id).ToList();
                swapChoices.Add("Exit");

                // logic that handles accepting user input on what tokens to swap & how much XCH to exchange for it.
                string rootCommand = "";
                while (rootCommand != "Exit")
                {
                    rootCommand = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("What Tokens would you like to [green]SWAP[/]?")
                            .MoreChoicesText("[grey](Move up and down to reveal more Tokens Pairs)[/]")
                            .AddChoices(swapChoices)
                            .UseConverter((i) =>
                            {
                                var verifiedWarning = "";
                                apiPairs.TryGetValue(i, out TokenResponse pair);
                                if (pair != null)
                                {
                                    verifiedWarning = pair.verified == true
                                        ? $"[green]{pair.verified}[/]"
                                        : $"[red]{pair.verified}[/]";
                                }
                                return i switch
                                {
                                    "Exit" => "Exit",
                                    _ => $"{pair.short_name} - {pair.Asset_id} - verified: {verifiedWarning}"
                                };
                            })
                    );

                    if (rootCommand == "Exit")
                        Environment.Exit(1);

                    if (!string.IsNullOrEmpty(rootCommand))
                    {
                        var pair = apiPairs.First(x => x.Value.pair_id == rootCommand);
                        var swapAmount =
                            AnsiConsole.Ask<double>(
                                $"What is the amount of $XCH would you like to swap for [green]{pair.Value.short_name}[/]?");
                        var swapAmountInMojos = ChiaHelper.ConvertToMojos(swapAmount);
                        var quote = await _tibetClient.GetQuote(pair.Value.pair_id, swapAmountInMojos);
                        var tokenOutput = TibetHelper.GetInputPrice(swapAmountInMojos, quote.input_reserve,
                            quote.output_reserve);
                        var confirmSwap =
                            AnsiConsole.Confirm(
                                $"You would receive ~{tokenOutput / 1000} {pair.Value.short_name} - confirm swap?",
                                false);
                        if (confirmSwap)
                        {
                            var (_, chiaOfferRpcResponse, tibetOfferResponse) =
                                await _offerService.CreateOffer(swapAmount, pair.Value.Asset_id);
                            AnsiConsole.MarkupLine(chiaOfferRpcResponse.Success
                                ? $"Chia Offer Created: [green]{chiaOfferRpcResponse.Success}[/]"
                                : $"Chia Offer Created: [red]{chiaOfferRpcResponse.Success}[/]");
                            AnsiConsole.MarkupLine(chiaOfferRpcResponse.Success
                                ? $"Offer Posted to TibetSwap Successfully: [green]{tibetOfferResponse.Success}[/]"
                                : $"Offer Posted to TibetSwap Successfully: [red]{tibetOfferResponse.Success}[/]");
                            Console.WriteLine($"Press any key to return to main menu");
                            Console.ReadKey();
                        }
                    }
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