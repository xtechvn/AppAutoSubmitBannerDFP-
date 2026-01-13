using AppAutoSubmitBannerDFP.ActionPartial;
using AppAutoSubmitBannerDFP.Common;
using AppAutoSubmitBannerDFP.Interfaces;
using AppAutoSubmitBannerDFP.Model;
using AppAutoSubmitBannerDFP.ViewModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Configuration;
using System.Threading;

namespace AppAutoSubmitBannerDFP.Engines.Proposal
{
    public class ProposalService: IProposalService
    {
        private static string tele_group_id = ConfigurationManager.AppSettings["tele_group_id"];
        private static string tele_token = ConfigurationManager.AppSettings["tele_token"];

        public bool createProposal(ChromeDriver browers, OrderViewModel order, int product_id, int request_id, out int iresult)
        {

            var BrowserLib = new BrowserActionLibProposal(browers, order, product_id, request_id);

            Console.WriteLine("========== START CREATE PROPOSAL " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: " + request_id + "==========");
            Ultities.Telegram.pushNotify("========== START CREATE PROPOSAL " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: " + request_id + "==========", tele_group_id, tele_token);

            BrowserLib.CloseNote();
            Thread.Sleep(100);

            BrowserLib.addName();
            Thread.Sleep(100);

            BrowserLib.addBuyer();
            Thread.Sleep(100);

            BrowserLib.addAdvertiser();
            Thread.Sleep(100);

            BrowserLib.addSalesContact();
            Thread.Sleep(100);

            BrowserLib.addSalesPerson();
            Thread.Sleep(100);

            BrowserLib.addTrafficker();
            Thread.Sleep(100);

            BrowserLib.addCurrency();
            Thread.Sleep(100);

            BrowserLib.addDeliveryPausing();
            Thread.Sleep(100);

            BrowserLib.saveProposal();
            Thread.Sleep(100);

            int iProposalId = BrowserLib.saveDatabase();
            Thread.Sleep(100);

            iresult = iProposalId;

            Console.WriteLine("==========END CREATE PROPOSAL " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + product_id + "***REQUEST_ID: " + request_id + "==========");

            return true;
        }
    }
}
