using AppAutoSubmitBannerDFP.ViewModel;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppAutoSubmitBannerDFP.Interfaces
{
    public interface ICreativeService
    {
        bool submitForm(ChromeDriver browers, CreativeViewModel banner,int product_id, int request_id, int line_item_id, int line_item_type, List<ErrorModel> lstError);
        bool submitFormCPMCreativeMasterCompanion(ChromeDriver browers, CreativeViewModel banner, int product_id, int request_id, int line_item_id, int line_item_type, List<ErrorModel> lstError);
        //bool editForm(ChromeDriver browers, CreativeViewModel banner, int product_id, int request_id, int line_item_id, int line_item_type);
    }
}
