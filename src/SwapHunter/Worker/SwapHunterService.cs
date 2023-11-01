using System.ComponentModel.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using SwapHunter.Client;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using SwapHunter.Client.Chia;
using Tibby;
using Tibby.Models;

namespace SwapHunter.Worker
{
    /// <summary>
    /// SwapHunter Service that runs as a background service that deals with
    /// Fetching tokens from tibetswaps api and facilitates creating the offer
    /// for a swap.
    /// </summary>
    public class SwapHunterService : BackgroundService
    {
        private ITibbyClient _tibetClient { get; set; }
        private IOptions<TibetSwapOptions> _tibetOptions { get; set; }
        private IConfiguration _config;
        private IChiaRpcClient _chiaRpcClient { get; set; }
        private IOfferService _offerService { get; set; }

        private ILogger<SwapHunterService> _logger;

        public SwapHunterService(ITibbyClient tibetclient, IOptions<TibetSwapOptions> tibetOptions,
            IConfiguration config, IChiaRpcClient chiaRpcClient, IOfferService offerService,
            ILogger<SwapHunterService> logger)
        {
            _tibetClient = tibetclient;
            _tibetOptions = tibetOptions;
            _config = config;
            _chiaRpcClient = chiaRpcClient;
            _offerService = offerService;
            _logger = logger;
        }

        private async Task Snipe()
        {
            var assetId = AnsiConsole.Ask<string>("Enter Asset Id:");
            var swapAmount = AnsiConsole.Ask<double>("Enter an amount to swap:");
            var swapAmountInMojos = ChiaHelper.ConvertToMojos(swapAmount);
            Console.WriteLine($"Waiting to Snipe AssetId: {assetId}");

            while (true)
            {
                var (tokenPairs, _) = await _tibetClient.GetTokenPairs();
                var apiPairs = tokenPairs.ToDictionary(x => x.pair_id);
                var token = tokenPairs.FirstOrDefault(x => x.Asset_id.ToLower().Equals(assetId.ToLower()));

                if (token != null)
                {
                    var (quote, _) = await _tibetClient.GetQuote(token.pair_id, swapAmountInMojos);
                    var tokenOutput = TibbyHelper.GetInputPrice(swapAmountInMojos, quote.input_reserve,
                        quote.output_reserve);
                    AnsiConsole.MarkupLine($"Sniping [green]~{tokenOutput / 1000}[/] of {token.short_name} for {swapAmount} xch");
                    var (_, chiaOfferRpcResponse, tibetOfferResponse) =
                        await _offerService.CreateOffer(swapAmount, token.Asset_id);
                    
                    AnsiConsole.MarkupLine(chiaOfferRpcResponse.Success
                        ? $"Chia Offer Created: [green]{chiaOfferRpcResponse.Success}[/]"
                        : $"Chia Offer Created: [red]{chiaOfferRpcResponse.Success}[/]");
                    AnsiConsole.MarkupLine(chiaOfferRpcResponse.Success
                        ? $"Offer Posted to TibetSwap Successfully: [green]{tibetOfferResponse.Success}[/]"
                        : $"Offer Posted to TibetSwap Successfully: [red]{tibetOfferResponse.Success}[/]");

                    if (chiaOfferRpcResponse.Success == true && tibetOfferResponse.Success == true)
                    {
                        AnsiConsole.MarkupLine($"[green]Congratulations - you have sniped {tokenOutput / 1000} of assetid: {assetId} - returning to main menu![/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[red]Problem occurred when sniping assetid: {assetId} - Please swap manually![/]");
                    }

                    return;
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now} - AssetId not found in {apiPairs.Count} tokens - delaying 10 minutes!");
                }
                
                //delay trying again for 10 minutes
                await Task.Delay(600000);
            }
        }

        private async Task Interactive()
        {
            var (tokenPairs, _) = await _tibetClient.GetTokenPairs();
            var apiPairs = tokenPairs.ToDictionary(x => x.pair_id);
            var swapChoices = apiPairs.Select(x => x.Value.pair_id).ToList();
            swapChoices.Add("Exit");
            var rootCommand = AnsiConsole.Prompt(
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
            {
                _logger.LogInformation("Exiting");
                Environment.Exit(1);
            }

            if (!string.IsNullOrEmpty(rootCommand))
            {
                var pair = apiPairs.First(x => x.Value.pair_id == rootCommand);
                var swapAmount =
                    AnsiConsole.Ask<double>(
                        $"What is the amount of $XCH would you like to swap for [green]{pair.Value.short_name}[/]?");
                var swapAmountInMojos = ChiaHelper.ConvertToMojos(swapAmount);
                var (quote, _) = await _tibetClient.GetQuote(pair.Value.pair_id, swapAmountInMojos);
                var tokenOutput = TibbyHelper.GetInputPrice(swapAmountInMojos, quote.input_reserve,
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
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var swapModes = new List<string>() { "Interactive", "Snipe" };
                swapModes.Add("Exit");

                // logic that handles accepting user input on what tokens to swap & how much XCH to exchange for it.
                string rootCommand = "";
                while (rootCommand != "Exit")
                {
                    //choose if sniping or using interactive
                    rootCommand = rootCommand = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Select Swapping [green]MODE[/]?")
                            .MoreChoicesText("[grey](Move up and down to reveal more select mode)[/]")
                            .AddChoices(swapModes)
                            .UseConverter((i) =>
                            {
                                return i switch
                                {
                                    "Exit" => "Exit",
                                    _ => $"{i}"
                                };
                            })
                    );

                    if (rootCommand == "Interactive")
                    {
                        await Interactive();
                    }
                    else if (rootCommand == "Snipe")
                    {
                        await Snipe();
                    }
                    else if (rootCommand == "Exit")
                    {
                        _logger.LogInformation("Exiting");
                        Environment.Exit(1);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}