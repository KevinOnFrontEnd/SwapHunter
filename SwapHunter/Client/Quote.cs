using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwapHunter.Client
{
  public class Quote
  {
    string amount_in { get; set; }
    string amount_out { get; set; }
    string price_warning { get; set; }
    string fee { get; set; }
    string asset_id { get; set; }
    string input_reserve { get; set; }
    string output_reserve { get; set; }
  }
}
