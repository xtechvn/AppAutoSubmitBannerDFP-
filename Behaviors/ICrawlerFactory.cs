using AppAutoSubmitBannerDFP.ViewModel;
using AppCrawlTaxNo.ViewModel;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppAutoSubmitBannerDFP.Behaviors
{
    public interface ICrawlerFactory
    {
        //void DoSomeRealWork(string url, ChromeDriver browers, BannerEntities banner);

        void MainProcess(string url, ChromeDriver browers, BannerEntities banner);
    }
}
