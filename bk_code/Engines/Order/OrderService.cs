using AppAutoSubmitBannerDFP.ActionPartial;
using AppAutoSubmitBannerDFP.Interfaces;
using AppAutoSubmitBannerDFP.ViewModel;
using System.Collections.Generic;
using OpenQA.Selenium.Chrome;
using System;
using System.Configuration;
using System.Threading;

namespace AppAutoSubmitBannerDFP.Engines.Order
{
    public class OrderService : IOderService
    {
        private static string tele_group_id = ConfigurationManager.AppSettings["tele_group_id"];
        private static string tele_token = ConfigurationManager.AppSettings["tele_token"];
        public bool createOrder(ChromeDriver browers, OrderViewModel order, int product_id, int request_id, List<ErrorModel> lstError, out int iResult)
        {
            bool bResult = false;
            try
            {
                var BrowserLib = new BrowserActionLibOrder(browers, order, product_id, request_id, lstError);

                Console.WriteLine("========== START CREATE ORDER " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: " + request_id + "==========");
                Ultities.Telegram.pushNotify("========== START CREATE ORDER " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: " + request_id + "==========", tele_group_id, tele_token);
                if (order.order_default == 0)
                {
                    BrowserLib.WaitOrderReady();
                    Thread.Sleep(100);
                    BrowserLib.CloseNote();
                    Thread.Sleep(100);
                    BrowserLib.addName();
                    Thread.Sleep(100);
                    BrowserLib.addAdvertiser();
                    Thread.Sleep(500);
                    BrowserLib.addTrafficker();                    
                    Thread.Sleep(500);
                    BrowserLib.addOrderSalesPerson();                    
                    Thread.Sleep(500);
                    BrowserLib.addSecondarySalesPeople();
                    Thread.Sleep(500);
                    BrowserLib.addSecondarytrafficker();
                    Thread.Sleep(500);
                    BrowserLib.saveOrder();                                        
                    Thread.Sleep(1500);
                }
                iResult = BrowserLib.saveDatabase();
                bResult = true;
                Console.WriteLine("==========END CREATE ORDER " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: " + request_id + "==========");
                Ultities.Telegram.pushNotify("==========END CREATE ORDER " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: " + request_id + "==========", tele_group_id, tele_token);
            }
            catch (Exception ex)
            {
                Console.WriteLine("createOrder: " + ex.ToString());
                Ultities.Telegram.pushNotify("createOrder: " + ex.ToString(), tele_group_id, tele_token);
                iResult = -1;
                bResult = false;
            }

            return bResult;
        }
    }
}
