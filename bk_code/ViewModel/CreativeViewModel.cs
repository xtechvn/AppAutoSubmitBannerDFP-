using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppAutoSubmitBannerDFP.ViewModel
{
    // STEP 3
   public class CreativeViewModel
    {
        public string name { get; set; }
        public string click_through_url { get; set; }
        public string iframe_url { get; set; }
        public string input_size { get; set; }
        public string ads_size { get; set; }
        public int line_item_sync { get; set; } // 1: đồng bộ | 0: Không phải
        public string index_industrial { get; set; }
        public string index_brand { get; set; }
        public string campaign_name { get; set; }
        public int creative_type { get; set; } // dùng để detect site cố định hay không. 1: site cố định thì phải đi qua trang: https://admanager.google.com/27973503#creatives/creative/create/view=standard | 0: không phải
        public string creative_template { get; set; } // text search -- Custom creative template        
        public string add_sizes { get; set; }
        public List<string> third_party_tracking { get; set; }
        public string creative_url { get; set; }
        public int overlay_card { get; set; }
        public int data_creative_id { get; set; }
        public int safe_frame { get; set; }
        public string run_mode { get; set; }
    }
}
