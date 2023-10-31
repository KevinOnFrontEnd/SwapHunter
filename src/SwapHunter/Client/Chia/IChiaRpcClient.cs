using SwapHunter.Client.Chia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwapHunter.Client.Chia
{
  public interface IChiaRpcClient
  {
    Task<CreateOfferResponse> CreateOffer(string assetid, Int64 requesting_amount, Int64 mojo_amount_offering, Int64 fee, bool validate_only);
  }
}
