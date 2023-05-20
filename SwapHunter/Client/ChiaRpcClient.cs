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
    private HttpClient _client;
    
    public ChiaRpcClient(IOptions<ChiaRpcOptions> options, HttpClient httpClient) 
    {
      _options = options;
      _client = httpClient;
    }

    public async Task<CreateOfferResponse> CreateOffer(string assetid, double requesting_amount_in_mojos, double xch_amount_in_mojos, double fee)
    {
      var obj =  new {
        offer=new Dictionary<string,double>() {
          { "1", -requesting_amount_in_mojos },
          { assetid, xch_amount_in_mojos }
        },
        Fee=fee
      };

      var targetJObject = JObject.FromObject(obj);
      var content = new StringContent(targetJObject.ToString(), Encoding.UTF8, "application/json");
      var response = await _client.PostAsync($"/create_offer_for_ids", content);
      response.EnsureSuccessStatusCode();
      string responseBody = await response.Content.ReadAsStringAsync();
      var offer = JsonConvert.DeserializeObject<CreateOfferResponse>(responseBody);
      return offer;
    }
  }
}
