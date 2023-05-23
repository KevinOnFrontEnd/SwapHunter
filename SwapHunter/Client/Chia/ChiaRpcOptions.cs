using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwapHunter.Client
{
  public class ChiaRpcOptions
  {
    public string WalletRpcEndpoint { get; set; }

    public string Wallet_key_path { get; set; }
    public string Wallet_cert_path { get; set; }
  }
}
