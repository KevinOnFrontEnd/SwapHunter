using System.Collections.Specialized;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SwapHunter.Client;
using SwapHunter.Client.Chia;
using SwapHunter.Worker;
using Tibby.Extensions;
using Tibby.Models;

namespace SwapHunter.Tests.Core;

public class SwapHunterFixture : IDisposable
{
    public IHost TestHost { get; }

    public SwapHunterFixture()
    {
        try
        {
            TestHost = CreateHostBuilder().Build();
            TestHost.Start();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public IHostBuilder? CreateHostBuilder()
    {
        return new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.ConfigureAppConfiguration((hostContext, configurationBuilder) =>
                {
                    configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
                    configurationBuilder.AddJsonFile("testingappsettings.json", optional: false);
                    configurationBuilder.AddEnvironmentVariables(prefix: "PREFIX_");
                    configurationBuilder.AddUserSecrets<SwapHunterFixture>(optional: true);
                });

                webHost.ConfigureServices((hostContext, services) =>
                {
                    var chiaRpcOptions = hostContext.Configuration.GetSection("ChiaRpc").Get<ChiaRpcOptions>();
                    var handler = new SocketsHttpHandler();
                    handler.SslOptions.ClientCertificates =
                        GetCerts(chiaRpcOptions.Wallet_cert_path, chiaRpcOptions.Wallet_key_path);
                    handler.SslOptions.RemoteCertificateValidationCallback += ValidateServerCertificate;

                    services.AddLogging(loggingBuilder => {
                        loggingBuilder.AddFile("app.log", append:true);
                    });
                    
                    services.AddHttpClient<IChiaRpcClient, ChiaRpcClient>(c =>
                    {
                        c.BaseAddress = new System.Uri(chiaRpcOptions.WalletRpcEndpoint);
                    }).ConfigurePrimaryHttpMessageHandler(() => { return handler; });

                    services.Configure<TibetSwapOptions>(hostContext.Configuration.GetSection("TibetSwap"));
                    services.AddTibbyClient();

                    services.AddSingleton<IOfferService, OfferService>();
                    services.Configure<ChiaRpcOptions>(hostContext.Configuration.GetSection("ChiaRpc"));
                });
                webHost.UseTestServer();
                webHost.Configure(app =>
                {
                    app.Run(async ctx =>
                    {
                        await ctx.Response.WriteAsync("Hello World!");
                    });
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

    public void Dispose()
    {
        TestHost.Dispose();
    }
}