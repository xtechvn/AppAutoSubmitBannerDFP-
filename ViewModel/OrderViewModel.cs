using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppAutoSubmitBannerDFP.ViewModel
{
    public class OrderViewModel
    {
        public string name { get; set; } //Phải đặt theo rule name (Hợp đồng - Product ID - Request ID - Tên tắt Khách hàng - Campaign name - Mã tắt vị trí - Nhóm/chuyên mục - nền tảng - vùng miền - thời gian chiến dịch)
        public string advertiser { get; set; } //lấy tên khách hàng từ order thiết kế
        public string sales_person { get; set; } //Người tạo order thiết kế (chỉ lấy user ID)
        public string secondary_salespeople { get; set; } //salecare của order thiết kế, khác người tạo.
        public string secondary_trafficker { get; set; }
        public string order_url { get; set; }
        public string trafficker { get; set; }
        public int sync { get; set; } // 1: Banner đồng bộ | 0: không đồng bộ
        public int order_default { get; set; } //1: Banner Default | 0: Normal
        public List<LineItemViewModel> line_item { get; set; }
    }
}
