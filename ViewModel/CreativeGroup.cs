using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppAutoSubmitBannerDFP.ViewModel
{
   public class CreativeGroup
    {
        public string AdsSize { get; set; } // Thay string bằng kiểu của c.ads_size
        public List<object> Creative { get; set; } // Thay object bằng kiểu của c
    }
}
