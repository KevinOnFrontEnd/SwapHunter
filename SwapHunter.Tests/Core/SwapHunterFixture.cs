using System.Collections.Specialized;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SwapHunter.Client;
using SwapHunter.Client.Chia;
using SwapHunter.Client.TibetSwap;
using SwapHunter.Worker;

namespace SwapHunter.Tests.Core;

public class SwapHunterFixture
{
    public IHost TestHost { get; }

    public SwapHunterFixture()
    {
        // TestHost = CreateHostBuilder().Build();
        // Task.Run(() => TestHost.RunAsync());

    }

    public IHostBuilder? CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder().ConfigureWebHostDefaults(b =>
        {
            b.ConfigureAppConfiguration((hostContext, configurationBuilder) =>
            {
                configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
                configurationBuilder.AddJsonFile("testingappsettings.json", optional: false);
                configurationBuilder.AddEnvironmentVariables(prefix: "PREFIX_");
                configurationBuilder.AddUserSecrets<SwapHunterFixture>(optional: true);
            });
        }).ConfigureWebHost(builder =>
        {
            //builder.UseConfiguration(config);
            builder.ConfigureServices((hostContext, services) =>
            {
                var chiaRpcOptions = hostContext.Configuration.GetSection("ChiaRpc").Get<ChiaRpcOptions>();
                var tibetSwapOptions = hostContext.Configuration.GetSection("TibetSwap").Get<TibetSwapOptions>();
                var handler = new SocketsHttpHandler();
                handler.SslOptions.ClientCertificates =
                    GetCerts(chiaRpcOptions.Wallet_cert_path, chiaRpcOptions.Wallet_key_path);
                handler.SslOptions.RemoteCertificateValidationCallback += ValidateServerCertificate;

                services.AddHttpClient<IChiaRpcClient, ChiaRpcClient>(c =>
                {
                    c.BaseAddress = new System.Uri(chiaRpcOptions.WalletRpcEndpoint);
                }).ConfigurePrimaryHttpMessageHandler(() => { return handler; });

                services.AddHttpClient<ITibetClient, TibetClient>(c =>
                {
                    c.BaseAddress = new System.Uri(tibetSwapOptions.ApiEndpoint);
                });
                
                services.Configure<TibetSwapOptions>(hostContext.Configuration.GetSection("TibetSwap"));
                services.Configure<ChiaRpcOptions>(hostContext.Configuration.GetSection("ChiaRpc"));
            });
        });
    }

    private static bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        // uncomment these checks to change remote cert validaiton requirements

        // require remote ca to be trusted on this machine
        //if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) == SslPolicyErrors.RemoteCertificateChainErrors) 
        //    return false;

        // require server name to be validated in the cert
        //if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) == SslPolicyErrors.RemoteCertificateNameMismatch)
        //    return false;

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
}