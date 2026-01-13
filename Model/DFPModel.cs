using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace DFPDataSetUpBannerConsumer.Lib
{
    public class DFPModel
    {
        public int ProductId { get; set; }
        public int RequestId { get; set; }
        public OrderModel order { get; set; }
        public LineItemModel line_item { get; set; }
        public CreativeModel creative { get; set; }
    }

    public class OrderModel
    {
        public string name { get; set; }
        public string advertiser { get; set; }
        public string trafficker { get; set; }
        public string secondary_trafficker { get; set; }
        public string sales_person { get; set; }
        public string secondary_salespeople { get; set; }
        public string order_url { get; set; }
        public int sync { get; set; }        
        public int order_default { get; set; }
        //public int order_proposals { get; set; }
        //public int partner_id { get; set; }
        public List<LineItemModel> line_item { get; set; }
    }

    public class LineItemModel
    {
        public int ad_type { get; set; }
        public string name { get; set; }
        public int line_item_type { get; set; }
        public string add_sizes { get; set; }
        public List<string> companion_sizes { get; set; }
        public DateTime start_time { get; set; }
        public DateTime end_time { get; set; }
        public double rate { get; set; }
        public int vat { get; set; }
        public double discount { get; set; }
        public int unit_type { get; set; }
        public List<AdUnitModel> adunit { get; set; }
        public List<PlacementModel> palcement { get; set; }
        public List<LocationModel> location { get; set; }
        public string targeting_type { get; set; }
        public string custom_targeting { get; set; }
        public string lineitem_url { get; set; }
        public long quantity { get; set; }
        public int percentage { get; set; }
        public int display_creative { get; set; }
        public int unit { get; set; }                
        public int usd { get; set; }        
        public int data_line_item_id { get; set; }
        public int item_type { get; set; }
        public int frequency { get; set; }
        public int goal { get; set; }
        public string start_deliver_time { get; set; }
        public string end_deliver_time { get; set; }
        public int create_cretive { get; set; }        
        public List<Age> age { get; set; }
        public List<Audience> audience { get; set; }
        public List<Gender> gender { get; set; }
        public List<Device> device { get; set; }
        public List<Article> article { get; set; }
        public List<Tag> tag { get; set; }
        public List<Brandsafe> brandsafe { get; set; }
        public List<CategoryId> categoryid { get; set; }
        public string category_name { get; set; }
        public string position_name { get; set; }
        public List<CreativeModel> creative { get; set; }
        public LineItemModel()
        {
            location = new List<LocationModel>();
            age = new List<Age>();
            audience = new List<Audience>();
            gender = new List<Gender>();
            device = new List<Device>();
            article = new List<Article>();
            tag = new List<Tag>();
            brandsafe = new List<Brandsafe>();
            categoryid = new List<CategoryId>();
        }
    }    

    public class AdUnitModel
    {
        public string adunit { get; set; }
        public int include { get; set; }
    }

    public class LocationModel
    {
        public int location_id { get; set; }
        public string location_name { get; set; }
        public int region { get; set; }
        public int include { get; set; }
        public LocationModel(int id, string loca, int reg, int incl)
        {
            location_id = id;
            location_name = loca;
            region = reg;
            include = incl;
        }
    }

    public class PlacementModel
    {
        public string placement { get; set; }
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

    public class CreativeModel
    {
        public string name { get; set; }
        public string click_through_url { get; set; }
        public string iframe_url { get; set; }
        public string ads_size { get; set; }
        public string input_size { get; set; }
        public string index_industrial { get; set; }
        public string index_brand { get; set; }
        public string campaign_name { get; set; }
        public int creative_type { get; set; }
        public string creative_template { get; set; }
        public string creative_url { get; set; }
        public string run_mode { get; set; }
        public string thirparty { get; set; }
        public int Id { get; set; }
        public int index { get; set; }
        public int line_item_sync { get; set; }
        public string add_sizes { get; set; }
        public List<string> third_party_tracking { get; set; }
        public int overlay_card { get; set; }
        public int data_creative_id { get; set; }
        public int safe_frame { get; set; }
    }

    public class SizeModel
    {
        public string name { get; set; }
        public string size { get; set; }
        public string ads_size { get; set; }
        public string creative_type { get; set; }
        public int is_placement { get; set; }
        public int overlay_card { get; set; }
        public int data_id { get; set; }
        public string input_size { get; set; }
        public SizeModel(int _data_id, string _name, string _size, string _ads_size, string _creative_type, int _is_placement, int _overlay_card, string _input_size)
        {
            name = _name;
            size = _size;
            ads_size = _ads_size;
            creative_type = _creative_type;
            is_placement = _is_placement;
            overlay_card = _overlay_card;
            data_id = _data_id;
            input_size = _input_size;
        }      
    }
}
