using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SwapHunter.Client.Chia;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public async Task<CreateOfferResponse> CreateOffer(string assetid, string requesting_amount, string xch_amount, string fee)
    {
      var handler = new HttpClientHandler()
      {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
      };
      var client = new HttpClient(handler);
      client.BaseAddress = new Uri(_options.Value.WalletRpcEndpoint);
      var obj =  new {
        offer=new Dictionary<string,string>() {
          { "1", $"{xch_amount }" },
          { assetid,requesting_amount }
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
  }
}
