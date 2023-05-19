using SwapHunter.Client.Chia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwapHunter.Client
{
  public interface IChiaRpcClient
  {
    Task<CreateOfferResponse> CreateOffer(string assetid, int requesting_amount, int xch_amount, int fee);
  }
}
