using AppAutoSubmitBannerDFP.ActionPartial;
using AppAutoSubmitBannerDFP.Interfaces;
using AppAutoSubmitBannerDFP.ViewModel;
using OpenQA.Selenium.Chrome;
using System;
using System.Configuration;
using System.Threading;
using System.Collections.Generic;

namespace AppAutoSubmitBannerDFP.Engines.Creative
{
    public class CreativeService : ICreativeService
    {
        private static string tele_group_id = ConfigurationManager.AppSettings["tele_group_id"];
        private static string tele_token = ConfigurationManager.AppSettings["tele_token"];

        public bool submitForm(ChromeDriver browers, CreativeViewModel creative, int product_id, int request_id, int line_item_id, int line_item_type, List<ErrorModel> lstError)
        {
            try
            {            

                var BrowserLib = new BrowserActionLibCreative(browers, creative, product_id, request_id, line_item_id, line_item_type, lstError);
                Console.WriteLine("==========START CREATE CREATIVE " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: " + request_id + "==========");
                
               // BrowserLib.WaitOrderReady();

                Thread.Sleep(5000);
                BrowserLib.CloseNote();

                Thread.Sleep(100);
                BrowserLib.creativeName();
                Thread.Sleep(100);
                
                BrowserLib.tagertAdSize();
                Thread.Sleep(500);

                BrowserLib.addCodeScript();
                Thread.Sleep(500);

                BrowserLib.addSafeFrame();
                Thread.Sleep(500);

                BrowserLib.clickThroughUrl();
                Thread.Sleep(100);

                BrowserLib.selectInputsize();
                Thread.Sleep(500);

                BrowserLib.selectRunMode();
                Thread.Sleep(500);

                // Tự check sticky size nếu có
                BrowserLib.selectStickySize(creative.ads_size);
                Thread.Sleep(500);

                // Chọn banner Size nếu có
                //BrowserLib.selectBannerSize(creative.ads_size);
                //Thread.Sleep(500);



                BrowserLib.upload_file();
                Thread.Sleep(500); 

                BrowserLib.iframeUrl();
                Thread.Sleep(100);

                BrowserLib.campaignName();
                Thread.Sleep(100);

                BrowserLib.indexIndustrial();
                Thread.Sleep(100);

                BrowserLib.indexBrand();
                Thread.Sleep(100);

                BrowserLib.selectVideo();
                Thread.Sleep(100);

                BrowserLib.thirdPartyImpression();
                Thread.Sleep(100);

                BrowserLib.selectOverlayCard();            
                Thread.Sleep(100);

                BrowserLib.saveCreative();
                Thread.Sleep(1500);

                BrowserLib.saveDatabase();
                Console.WriteLine("==========END CREATE CREATIVE " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: " + request_id + "==========");
                Ultities.Telegram.pushNotify("==========END CREATE CREATIVE " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: " + request_id + "==========", tele_group_id, tele_token);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("submitForm invalid !!! " + ex.ToString());
                Ultities.Telegram.pushNotify("submitForm invalid !!! ", tele_group_id, tele_token);
                return false;
            }
        }

        public bool submitFormCPMCreativeMasterCompanion(ChromeDriver browers, CreativeViewModel creative, int product_id, int request_id, int line_item_id, int line_item_type, List<ErrorModel> lstError)
        {
            try
            {
                var BrowserLib = new BrowserActionLibCreative(browers, creative, product_id, request_id, line_item_id, line_item_type, lstError);

                Console.WriteLine("==========START CREATE CREATIVE " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: " + request_id + "==========");

                BrowserLib.creativeNameMC();

                Thread.Sleep(500);
                BrowserLib.clickThroughUrlMC();

                Thread.Sleep(500);
                BrowserLib.iframeUrlMC();

                Thread.Sleep(500);
                BrowserLib.indexIndustrialMC();

                Thread.Sleep(500);
                BrowserLib.indexBrandMC();

                Thread.Sleep(500);
                BrowserLib.campaignNameMC();

                Thread.Sleep(500);
                BrowserLib.thirdPartyImpressionMC();

                Thread.Sleep(500);
                BrowserLib.saveCreativeMC();

                Thread.Sleep(2000);

                BrowserLib.saveAndPreview();
                Thread.Sleep(1500);

                BrowserLib.saveDatabase();

                Console.WriteLine("==========END CREATE CREATIVE - MC " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: " + request_id + "==========");
                Ultities.Telegram.pushNotify("==========END CREATE CREATIVE - MC " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: " + request_id + "==========", tele_group_id, tele_token);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("submitFormCPMCreativeMasterCompanion invalid !!! " + ex.ToString());
                Ultities.Telegram.pushNotify("submitFormCPMCreativeMasterCompanion invalid= " + ex.ToString(), tele_group_id, tele_token);
                return false;
            }
        }

        /*public bool editForm(ChromeDriver browers, CreativeViewModel creative, int product_id, int request_id, int line_item_id, int line_item_type)
        {
            try
            {
                var BrowserLib = new BrowserActionLibCreative(browers, creative, product_id, request_id, line_item_id, line_item_type);
                Console.WriteLine("==========START editForm CREATIVE " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: " + request_id + "==========");
                Ultities.Telegram.pushNotify("==========START editForm CREATIVE " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: "  + request_id + "==========", tele_group_id, tele_token);

                BrowserLib.creativeName();
                Thread.Sleep(100);          

                BrowserLib.clickThroughUrl();
                Thread.Sleep(100);              

                BrowserLib.iframeUrl();
                Thread.Sleep(100);

                BrowserLib.campaignName();
                Thread.Sleep(100);

                BrowserLib.indexIndustrial();
                Thread.Sleep(100);

                BrowserLib.indexBrand();
                Thread.Sleep(100);

                Thread.Sleep(100);
                BrowserLib.thirdPartyImpression();
            
                BrowserLib.saveCreative();
                Thread.Sleep(1500);

                BrowserLib.saveDatabase();

                Console.WriteLine("==========END editForm CREATIVE " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: " + request_id + "==========");
                Ultities.Telegram.pushNotify("==========END editForm CREATIVE " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: " + request_id + "==========", tele_group_id, tele_token);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("editForm CREATIVE invalid !!! " + ex.ToString());
                Ultities.Telegram.pushNotify("editForm CREATIVE invalid = " + ex.ToString(), tele_group_id, tele_token);
                return false;
            }
        }*/
    }
}