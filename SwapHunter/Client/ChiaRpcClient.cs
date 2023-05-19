using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SwapHunter.Client.Chia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SwapHunter.Client
{
  public class ChiaRpcClient : IChiaRpcClient
  {
    IOptions<ChiaRpcOptions> _options { get; set; }
    
    public ChiaRpcClient(IOptions<ChiaRpcOptions> options) 
    {
      _options = options;
    }

    public async Task<CreateOfferResponse> CreateOffer(string assetid, int requesting_amount, int xch_amount, int fee)
    {
      var handler = new SocketsHttpHandler();
      handler.SslOptions.ClientCertificates = GetCerts(_options.Value.Wallet_cert_path, _options.Value.Wallet_key_path);
      handler.SslOptions.RemoteCertificateValidationCallback += ValidateServerCertificate;

      
      var client = new HttpClient(handler);
      client.BaseAddress = new Uri(_options.Value.WalletRpcEndpoint);
      var obj =  new {
        offer=new Dictionary<string,int>() {
          { "1", xch_amount },
          { assetid, requesting_amount }
        },
        Fee=fee
      };

      var targetJObject = JObject.FromObject(obj);
      var content = new StringContent(targetJObject.ToString(), Encoding.UTF8, "application/json");
      var response = await client.PostAsync($"/create_offer_for_ids", content);
      response.EnsureSuccessStatusCode();
      string responseBody = await response.Content.ReadAsStringAsync();
      var offer = JsonConvert.DeserializeObject<CreateOfferResponse>(responseBody);
      return offer;
    }
    private static bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    {
      // uncomment these checks to change remote cert validaiton requirements

      // require remote ca to be trusted on this machine
      //if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) == SslPolicyErrors.RemoteCertificateChainErrors) 
      //    return false;

      // require server name to be validated in the cert
      //if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) == SslPolicyErrors.RemoteCertificateNameMismatch)
      //    return false;

      return !((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) == SslPolicyErrors.RemoteCertificateNotAvailable);
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
}
