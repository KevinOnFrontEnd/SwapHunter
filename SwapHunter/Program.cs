using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SwapHunter.Client;
using SwapHunter.Worker;

namespace SwapHunter
{
  internal class Program
  {
    static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
              services.AddSingleton<ITibetClient, TibetClient>();
              services.AddSingleton<IChiaRpcClient, ChiaRpcClient>();
              services.AddHostedService<SwapHunterService>();
              services.Configure<TibetSwapOptions>(hostContext.Configuration.GetSection("TibetSwap"));
              services.Configure<ChiaRpcOptions>(hostContext.Configuration.GetSection("ChiaRpc"));
            });
  }
}