using AppAutoSubmitBannerDFP.ActionPartial;
using AppAutoSubmitBannerDFP.Interfaces;
using AppAutoSubmitBannerDFP.ViewModel;
using OpenQA.Selenium.Chrome;
using System;
using System.Configuration;
using System.Threading;

namespace AppAutoSubmitBannerDFP.Engines.LineItem
{
    public class LineItemProposalService: ILineItemProposalService
    {

        public bool submitForm(ChromeDriver browers, LineItemViewModel banner, int product_id, int request_id, int order_id, out int line_item_id)
        {
            try
            {
                line_item_id = -1;
                return true;
            }
            catch (Exception ex)
            {
                line_item_id = -1;
                return false;
                throw;
            }
        }



    }
}
