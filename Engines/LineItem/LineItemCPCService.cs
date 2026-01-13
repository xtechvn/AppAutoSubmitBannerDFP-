using AppAutoSubmitBannerDFP.ActionPartial;
using AppAutoSubmitBannerDFP.Interfaces;
using AppAutoSubmitBannerDFP.ViewModel;
using OpenQA.Selenium.Chrome;
using System;
using System.Configuration;
using System.Threading;
using System.Collections.Generic;


namespace AppAutoSubmitBannerDFP.Engines.LineItem
{
    public class LineItemCPCService : ILineItemCPCService
    {
        private static string tele_group_id = ConfigurationManager.AppSettings["tele_group_id"];
        private static string tele_token = ConfigurationManager.AppSettings["tele_token"];

        public bool submitForm(ChromeDriver browers, LineItemViewModel banner, int product_id, int request_id, int order_id,List<ErrorModel> lstError, out int line_item_id)
        {
            try
            {
                var BrowserLib = new BrowserActionLibLineItem(browers, banner, product_id, request_id, order_id, lstError);
                Console.WriteLine("==========START CREATE LINEITEM " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: " + request_id + "==========");
                Ultities.Telegram.pushNotify("==========START CREATE LINEITEM " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: " + request_id + "==========", tele_group_id, tele_token);

                BrowserLib.WaitOrderReady();

                Thread.Sleep(500);
                BrowserLib.CloseNote();

                Thread.Sleep(500);                
                BrowserLib.addTargetingName();

                Thread.Sleep(500);
                BrowserLib.addLineItem();

                Thread.Sleep(500);
                BrowserLib.addLineItemPriority(); // cpm = HIGH
                
                Thread.Sleep(500);                
                BrowserLib.creativeSize();

                Thread.Sleep(500);                                
                BrowserLib.companionSizes();                
                
                Thread.Sleep(500);
                BrowserLib.addExpectedCreatives();
                
                Thread.Sleep(500);
                BrowserLib.addStartTime();

                Thread.Sleep(500);
                BrowserLib.addStartHour();

                Thread.Sleep(500);
                BrowserLib.addEndTime();                

                Thread.Sleep(500);
                BrowserLib.addEndHour();

                Thread.Sleep(500);
                BrowserLib.addGoal();

                Thread.Sleep(500);
                BrowserLib.goalType();

                Thread.Sleep(500);
                BrowserLib.addQuantity();

                Thread.Sleep(500);
                BrowserLib.addUnitType();

                Thread.Sleep(500);
                BrowserLib.addRate();

                Thread.Sleep(500);
                BrowserLib.addCurrency();

                Thread.Sleep(500);
                BrowserLib.addDiscount();                

                Thread.Sleep(500);
                BrowserLib.addUnit();                

                Thread.Sleep(500);
                BrowserLib.addDisplayCompanions(); //Adjust delivery | CPM: At least one

                Thread.Sleep(500);
                BrowserLib.addDeliveryTime();                

                Thread.Sleep(500);
                BrowserLib.addFrequency();
                
                Thread.Sleep(500);
                BrowserLib.addTargetingInventory();

                Thread.Sleep(500);
                BrowserLib.addPlacements();

                Thread.Sleep(500);
                BrowserLib.checkSizeAdUnitsAndPlacements();

                Thread.Sleep(500);
                int slot = BrowserLib.addSlot();                

                //Thread.Sleep(500);
                BrowserLib.addAge();

                //Thread.Sleep(500);
                BrowserLib.addGender();

                //Thread.Sleep(500);
                BrowserLib.addAudience();

                //Thread.Sleep(500);
                BrowserLib.addArticle();

                //Thread.Sleep(500);
                BrowserLib.addTag();

                //Thread.Sleep(500);
                BrowserLib.addBrandsafe();

                //Thread.Sleep(500);
                BrowserLib.addCategoryId();                

                //Thread.Sleep(500);
                BrowserLib.addGeography();

                //Thread.Sleep(500);
                BrowserLib.addDevice();             

                Thread.Sleep(500);
                BrowserLib.saveLineItem();

                Thread.Sleep(1500);
                line_item_id = BrowserLib.saveDatabase(slot);               

                Console.WriteLine("==========END CREATE LINEITEM " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: " + request_id + "==========");
                Ultities.Telegram.pushNotify("==========END CREATE LINEITEM " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: " + request_id + " ========== ", tele_group_id, tele_token);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("LineItemCPCService- submitForm:  " + ex.ToString());
                Ultities.Telegram.pushNotify("LineItemCPCService- submitForm:  " + ex.ToString(), tele_group_id, tele_token);
                line_item_id = -1;
                return false;

            }
        }

        public bool EditForm(ChromeDriver browers, LineItemViewModel banner, int product_id, int request_id, int order_id, out int line_item_id)
        {
            try
            {
                var BrowserLib = new BrowserActionLibLineItem(browers, banner, product_id, request_id, order_id, null);
                Console.WriteLine("==========START EDIT LINEITEM " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: " + request_id + "==========");
                Ultities.Telegram.pushNotify("==========START EDIT LINEITEM " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: " + request_id + "==========", tele_group_id, tele_token);

                Thread.Sleep(500);
                BrowserLib.addStartTime();

                Thread.Sleep(500);
                BrowserLib.addEndTime();

                Thread.Sleep(500);
                BrowserLib.addStartHour();

                Thread.Sleep(500);
                BrowserLib.addEndHour();

                Thread.Sleep(500);
                BrowserLib.goalType();

                Thread.Sleep(500);
                BrowserLib.addRate();

                Thread.Sleep(500);
                BrowserLib.addDiscount();                      

                Thread.Sleep(500);
                BrowserLib.addUnitType();               

                Thread.Sleep(500);
                //int slot = BrowserLib.checkInventory();
                int slot = 0;

                Thread.Sleep(500);
                BrowserLib.saveLineItem();

                Thread.Sleep(1500);
                line_item_id = BrowserLib.saveDatabase(slot);

                Console.WriteLine("==========END EDIT LINEITEM " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: " + request_id + "==========");
                Ultities.Telegram.pushNotify("==========END EDIT LINEITEM " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: " + request_id + "==========", tele_group_id, tele_token);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("LineItemCPCService- submitForm:  " + ex.ToString());
                Ultities.Telegram.pushNotify("LineItemCPCService- submitForm:  " + ex.ToString(), tele_group_id, tele_token);
                line_item_id = -1;
                return false;

            }
        }

    }
}
