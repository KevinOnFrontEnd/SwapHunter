﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwapHunter.Client.Chia
{
  public class CreateOfferResponse
  {
    public string Offer { get; set; }
    public bool Success { get; set; }
    public string Error { get; set; }
    
    public double Spendable { set; get; }
    public string offer_id { get; set; }

  }
}
