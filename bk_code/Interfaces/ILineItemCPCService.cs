using AppAutoSubmitBannerDFP.ViewModel;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppAutoSubmitBannerDFP.Interfaces
{
    public interface ILineItemCPCService
    {
        bool submitForm(ChromeDriver browers, LineItemViewModel banner, int product_id, int request_id, int order_id, List<ErrorModel> lstError, out int line_item_id);
        bool EditForm(ChromeDriver browers, LineItemViewModel banner, int product_id, int request_id, int order_id, out int line_item_id);
    }
}
