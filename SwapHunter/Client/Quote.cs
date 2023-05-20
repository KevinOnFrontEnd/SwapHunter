using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwapHunter.Client
{
  public class Quote
  {
    public string amount_in { get; set; }
    public string amount_out { get; set; }
    public string price_warning { get; set; }
    public string fee { get; set; }
    public string asset_id { get; set; }
    public string input_reserve { get; set; }
    public string output_reserve { get; set; }
  }
}
