using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppAutoSubmitBannerDFP.ViewModel
{
 public  class AdUnitViewModel
    {
        public string adunit { get; set; }
        public int include { get; set; } // 1: include dấu xanh | 0: exclude :dấu đỏ
    }
}
