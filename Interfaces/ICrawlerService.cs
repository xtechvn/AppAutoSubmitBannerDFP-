using AppAutoSubmitBannerDFP.ViewModel;
using AppCrawlTaxNo.ViewModel;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppAutoSubmitBannerDFP.Interfaces
{
    public interface ICrawlerService
    {
      
        bool crawlerPage(ChromeDriver browers, string url, BannerEntities banner);
        
        
    }
}
