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
    Task<CreateOfferResponse> CreateOffer(string assetid, double requesting_amount, double mojo_amount_offering, double fee, bool validate_only);
  }
}
