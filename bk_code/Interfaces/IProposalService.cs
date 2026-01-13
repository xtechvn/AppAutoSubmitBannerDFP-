using AppAutoSubmitBannerDFP.ViewModel;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppAutoSubmitBannerDFP.Interfaces
{
    public interface IProposalService
    {
        bool createProposal(ChromeDriver browers, OrderViewModel order, int product_id, int request_id, out int iresult);
    }
}
