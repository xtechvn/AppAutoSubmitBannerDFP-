using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppAutoSubmitBannerDFP.ViewModel
{
    public class LineItemViewModel
    {
        public int ad_type { get; set; } //1: Standard || 2: Master/companion || 3: Video or audio   https://admanager.google.com/27973503#delivery/line_item/create/order_id=3071858295
        public string name { get; set; }// Phải đặt theo rule name(Hợp đồng - Product ID - Request ID - Tên tắt Khách hàng - Campaign name - Mã tắt vị trí - Nhóm/chuyên mục - nền tảng - vùng miền - thời gian chiến dịch)
        public int line_item_type { get; set; } // 1: cpd, 2: cpm, 3: cpm toi uu , 4: cpc   line item type : ex: Sponsorship(4)         
        public long quantity { get; set; }
        public string add_sizes { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public double rate { get; set; } // price
        public double vat { get; set; }
        public double discount { get; set; }

        public int unit_type { get; set; } // percentage or vnd
      //public List<InventoryViewModel> inventory   { get; set; } // thông tin submit Banner. Bao gồm site, mục, device, vị trí
        public List<AdUnitViewModel> adunit { get; set; }
        public string custom_targeting { get; set; } // 2,4,3. Vị trí targeting
        public int percentage { get; set; }
        public int display_creative { get; set; }        
        public List<string> companion_sizes { get; set; }
        public int unit { get; set; }                
        public int usd { get; set; }
        public string lineitem_url { get; set; }
        public int data_line_item_id { get; set; }
        public int item_type { get; set; }
        public int frequency { get; set; }
        public string start_deliver_time { get; set; }
        public string end_deliver_time { get; set; }
        public int goal { get; set; }
        public string category_name { get; set; }
        public string position_name { get; set; }
        public List<Age> age { get; set; }
        public List<Audience> audience { get; set; }
        public List<Gender> gender { get; set; }
        public List<Device> device { get; set; }
        public List<Article> article { get; set; }
        public List<Tag> tag { get; set; }
        public List<Brandsafe> brandsafe { get; set; }
        public List<CategoryId> categoryid { get; set; }
        public List<CreativeViewModel> creative { get; set; }        
        public List<PalcementViewModel> palcement { get; set; }
        public List<LocationViewModel> location { get; set; }
    }

    public class Age
    {
        public string age { get; set; }
        public int include { get; set; }
        public Age(string item, int type)
        {
            age = item;
            include = type;
        }
    }

    public class Audience
    {
        public string audience { get; set; }

        public Audience(string item)
        {
            audience = item;
        }
    }

    public class Gender
    {
        public string gender { get; set; }
        public int include { get; set; }

        public Gender(string item, int type)
        {
            gender = item;
            include = type;
        }
    }

    public class Device
    {
        public string device { get; set; }

        public Device(string item)
        {
            device = item;
        }
    }

    public class Article
    {
        public string article { get; set; }
        public Article(string item)
        {
            article = item;
        }
    }

    public class Tag
    {
        public string tag { get; set; }
        public Tag(string item)
        {
            tag = item;
        }
    }

    public class Brandsafe
    {
        public string brandsafe { get; set; }
        public Brandsafe(string item)
        {
            brandsafe = item;
        }
    }

    public class CategoryId
    {
        public string categoryid { get; set; }
        public CategoryId(string item)
        {
            categoryid = item;
        }
    }
}
