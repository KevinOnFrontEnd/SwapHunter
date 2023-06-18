using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SwapHunter.Client;
using SwapHunter.Worker;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Logging;
using Serilog;
using SwapHunter.Client.Chia;
using Tibby;
using Tibby.Extensions;
using Tibby.Models;

namespace SwapHunter
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).ConfigureAppConfiguration((hostContext, configurationBuilder) =>
                {
                    configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
                    configurationBuilder.AddJsonFile("appsettings.json", optional: false);
                    configurationBuilder.AddEnvironmentVariables(prefix: "PREFIX_");
                    configurationBuilder.AddUserSecrets<Program>(optional: true);
                })
                .ConfigureServices(services =>
                {
                    services.AddTibbyClient();
                    services.AddTransient<SwapHunterService>();
   
                }).UseSerilog((ctx, lc) => lc
                    .WriteTo.File("app.log")
                    .ReadFrom.Configuration(ctx.Configuration))
                .Build();
            
            var cancelSource = new CancellationTokenSource();
            var service = host.Services.GetRequiredService<SwapHunterService>();
            Task.Run(() =>  service.StartAsync(cancelSource.Token));
            await host.WaitForShutdownAsync();
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return !((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) ==
                     SslPolicyErrors.RemoteCertificateNotAvailable);
        }

        public static X509Certificate2Collection GetCerts(string certPath, string keyPath)
        {
            if (!File.Exists(certPath))
            {
                throw new FileNotFoundException($"crt file {certPath} not found");
            }

            if (!File.Exists(keyPath))
            {
                throw new FileNotFoundException($"key file {keyPath} not found");
            }

            using X509Certificate2 cert = new(certPath);
            using StreamReader streamReader = new(keyPath);

            var base64 = new StringBuilder(streamReader.ReadToEnd())
                .Replace("-----BEGIN RSA PRIVATE KEY-----", string.Empty)
                .Replace("-----END RSA PRIVATE KEY-----", string.Empty)
                .Replace(Environment.NewLine, string.Empty)
                .ToString();

            using var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(base64), out _);

            using var certWithKey = cert.CopyWithPrivateKey(rsa);
            var ephemeralCert = new X509Certificate2(certWithKey.Export(X509ContentType.Pkcs12));

            return new(ephemeralCert);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var chiaRpcOptions = hostContext.Configuration.GetSection("ChiaRpc").Get<ChiaRpcOptions>();
                    var handler = new SocketsHttpHandler();
                    handler.SslOptions.ClientCertificates =
                        GetCerts(chiaRpcOptions.Wallet_cert_path, chiaRpcOptions.Wallet_key_path);
                    handler.SslOptions.RemoteCertificateValidationCallback += ValidateServerCertificate;

                    services.AddSingleton<IOfferService, OfferService>();
                    services.AddHttpClient<IChiaRpcClient, ChiaRpcClient>(c =>
                    {
                        c.BaseAddress = new System.Uri(chiaRpcOptions.WalletRpcEndpoint);
                    }).ConfigurePrimaryHttpMessageHandler(() => { return handler; });
                    

                    services.AddSingleton<IOfferService, OfferService>();
                    services.AddHostedService<SwapHunterService>();
                    services.Configure<TibetSwapOptions>(hostContext.Configuration.GetSection("TibetSwap")); 
                    services.Configure<ChiaRpcOptions>(hostContext.Configuration.GetSection("ChiaRpc"));
                });
    }
}