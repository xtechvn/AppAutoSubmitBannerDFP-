using AppAutoSubmitBannerDFP.Common;
using AppAutoSubmitBannerDFP.Model;
using AppAutoSubmitBannerDFP.ViewModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace AppAutoSubmitBannerDFP.ActionPartial
{
    public class BrowserActionLibProposal
    {
        private static string tele_group_id = ConfigurationManager.AppSettings["tele_group_id"];
        private static string tele_token = ConfigurationManager.AppSettings["tele_token"];
        private ChromeDriver browers;
        private OrderViewModel order;
        private int product_id;
        private int request_id;
        private static string is_production = ConfigurationManager.AppSettings["is_production"].ToString();
        WebDriverWait wait = null;

        public BrowserActionLibProposal(ChromeDriver _browers, OrderViewModel _order, int _product_id, int _request_id)
        {
            browers = _browers;
            order = _order;
            product_id = _product_id;
            request_id = _request_id;
            wait = new WebDriverWait(_browers, TimeSpan.FromSeconds(10));
        }

        public void CloseNote(int index = 0)
        {
            try
            {
                string x_path_popup_note = "//*[contains(local-name(),'release-notes')]//*[contains(@class,'release-notes-popup-container')][contains(@class,'hidden')]";
                string x_path_close_note = "//*[contains(local-name(),'release-notes')]//*[contains(local-name(),'material-button')][contains(@class,'close')]";
                if (!Common.Common.checkXpathExist(x_path_popup_note, browers))
                {
                    if (Common.Common.checkXpathExist(x_path_close_note, browers))
                    {
                        browers.FindElement(By.XPath(x_path_close_note)).Click();
                    }
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    CloseNote(1);
                }
                else
                {
                    Ultities.Telegram.pushNotify("CloseNote " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("CloseNote [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addName(int index = 0)
        {
            try
            {
                string x_path_order_name = "//*[contains(@debugid,'proposalName')]//*[contains(local-name(),'input')]";                
                browers.FindElement(By.XPath(x_path_order_name)).Clear();
                browers.FindElement(By.XPath(x_path_order_name)).SendKeys(order.name + (is_production == "1" ? "" : "_BOT_" + DateTime.Now.ToString()));
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addName(1);
                }
                else
                {
                    Ultities.Telegram.pushNotify("addName " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addName [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addBuyer(int index = 0)
        {
            try
            {

            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addBuyer(1);
                }
                else
                {
                    //Ultities.Telegram.pushNotify("addBuyer " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addBuyer [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addAdvertiser(int index = 0)
        {
            try
            {
                string x_path_order_advertiser  = "//*[contains(@ngcontrol,'advertiser')]//*[contains(local-name(),'input')] [contains(@aria-label,'Type to search')]";
                string x_path_first_advertiser = "((//*[contains(@class,'suggestion-list')]//*[contains(@class,'list-group')])[1]//*)[1]";

                browers.FindElement(By.XPath(x_path_order_advertiser)).Clear();
                Thread.Sleep(500);                

                browers.FindElement(By.XPath(x_path_order_advertiser)).SendKeys(order.advertiser);     
                
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(x_path_first_advertiser)));
                browers.FindElement(By.XPath(x_path_first_advertiser)).Click();
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addAdvertiser(1);
                }
                else
                {
                    Ultities.Telegram.pushNotify("addAdvertiser " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addAdvertiser [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addSalesContact(int index = 0)
        {
            try
            {
                string x_path_proposal_sales_person = "//*[contains(local-name(),'user-picker')] [contains(@ngcontrol,'sellerContactIds')]//*[contains(local-name(),'input')] [contains(@aria-label,'Type to search')]";                
                string x_path_proposal_sales_person_select = "(//*[contains(@class,'acx-overlay-container-parent')]//*[contains(@pane-id,'TRAFFICKING-')][contains(@style,'visibility')]//*[contains(@class,'list-group')])[1]/material-select-dropdown-item[1]";
                browers.FindElement(By.XPath(x_path_proposal_sales_person)).Clear();
                browers.FindElement(By.XPath(x_path_proposal_sales_person)).SendKeys(order.sales_person);
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(x_path_proposal_sales_person_select)));
                browers.FindElement(By.XPath(x_path_proposal_sales_person_select)).Click();
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addSalesContact(1);
                }
                else
                {
                    Ultities.Telegram.pushNotify("addSalesContact " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addSalesContact [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addSalesPerson(int index = 0)
        {
            try
            {
                string x_path_proposal_sales_person = "//*[contains(local-name(),'user-picker')] [contains(@ngcontrol,'primarySalesperson')]//*[contains(local-name(),'input')] [contains(@aria-label,'Type to search')]";
                string x_path_proposal_sales_person_select = "(//*[contains(@class,'acx-overlay-container-parent')]//*[contains(@pane-id,'TRAFFICKING-')][contains(@style,'visibility')]//*[contains(@class,'list-group')])[1]/material-select-dropdown-item[1]";
                browers.FindElement(By.XPath(x_path_proposal_sales_person)).Clear();
                browers.FindElement(By.XPath(x_path_proposal_sales_person)).SendKeys(order.sales_person);
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(x_path_proposal_sales_person_select)));
                browers.FindElement(By.XPath(x_path_proposal_sales_person_select)).Click();
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addSalesPerson(1);
                }
                else
                {
                    Ultities.Telegram.pushNotify("addSalesPerson " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addSalesPerson [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addTrafficker(int index = 0)
        {
            try
            {
                string x_path_proposal_trafficker = "//*[contains(local-name(),'user-picker')] [contains(@ngcontrol,'primaryTraffickerId')]//*[contains(local-name(),'input')] [contains(@aria-label,'Type to search')]";
                string x_path_proposal_trafficker_person = "(//*[contains(@class,'acx-overlay-container-parent')]//*[contains(@pane-id,'TRAFFICKING-')][contains(@style,'visibility')]//*[contains(@class,'list-group')])[1]/material-select-dropdown-item[1]";
                
                browers.FindElement(By.XPath(x_path_proposal_trafficker)).Clear();
                browers.FindElement(By.XPath(x_path_proposal_trafficker)).SendKeys(order.sales_person);
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(x_path_proposal_trafficker_person)));
                browers.FindElement(By.XPath(x_path_proposal_trafficker_person)).Click();

            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addTrafficker(1);
                }
                else
                {
                    Ultities.Telegram.pushNotify("addTrafficker " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addTrafficker [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addCurrency(int index = 0)
        {
            try
            {
                string x_path_proposal_currency = "//*[contains(local-name(),'currency-picker')]//*[contains(local-name(),'dropdown-button')]";
                string x_path_proposal_currency_select = "//*[contains(@id,'acx-overlay-container-TRAFFICKING')]//*[contains(@pane-id,'TRAFFICKING-')][contains(@class,'visible')]//material-select-dropdown-item[2]";

                browers.FindElement(By.XPath(x_path_proposal_currency)).Click();                
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(x_path_proposal_currency_select)));
                browers.FindElement(By.XPath(x_path_proposal_currency_select)).Click();

            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addCurrency(1);
                }
                else
                {
                    Ultities.Telegram.pushNotify("addCurrency " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addCurrency [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addDeliveryPausing(int index = 0)
        {
            try
            {
                string x_path_proposal_delivery = "//*[contains(local-name(),'currency-picker')]//*[contains(local-name(),'dropdown-button')]";
                string x_path_proposal_delivery_status = "//*[contains(@debugid,'pausingConsentToggle')]//*[contains(local-name(),'material-toggle')]//*[contains(@class,'material-toggle')]";

                var elem = browers.FindElement(By.XPath(x_path_proposal_delivery_status));
                var status = elem.GetAttribute("aria-checked");
                if (status == "true")
                {
                    browers.FindElement(By.XPath(x_path_proposal_delivery)).Click();
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addDeliveryPausing(1);
                }
                else
                {
                    Ultities.Telegram.pushNotify("addDeliveryPausing " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addDeliveryPausing [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void saveProposal(int index = 0)
        {
            try
            {
                string x_path_button_save = "//*[contains(local-name(),'material-button')] [contains(@debugid,'save-button')]";

                browers.FindElement(By.XPath(x_path_button_save)).Click();
                Thread.Sleep(1200);
                string link_order = browers.Url;
                if (link_order.Contains("proposal_id"))
                {
                    Console.WriteLine("CREATE PROPOSAL SUCCESS: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": " + order.name);
                }
                else
                {
                    Thread.Sleep(1000);
                    if (!link_order.Contains("proposal_id"))
                    {
                        index = index + 1;
                        if (index == 1)
                        {
                            saveProposal(1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    saveProposal(1);
                }
                else
                {
                    string url_error = Common.Common.TakeScreen(browers, request_id, product_id);
                    Ultities.Telegram.pushNotify("saveProposal " + ex.ToString() + " #$# " + HttpUtility.UrlEncode(url_error), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("saveProposal [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public int saveDatabase(int index = 0)
        {
            try
            {
                string x_path_button_save = "//*[contains(local-name(),'material-button')] [contains(@debugid,'save-button')]";

                if (!Common.Common.checkXpathExist(x_path_button_save, browers))
                {
                    string link_proposal = browers.Url;
                    int parent_id = -1;
                    int type = (Int16)dfp_setup_type.order;
                    int iResult = Repository.updateStatusFilter(parent_id, link_proposal, type, product_id, request_id, "", -1, -1);
                    Ultities.Telegram.pushNotify("CREATE PROPOSAL SUCCESS: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": " + order.name + " link proposal: " + HttpUtility.UrlEncode(link_proposal), tele_group_id, tele_token);
                    return iResult;
                }
                else
                {
                    Console.WriteLine("[ORDER] Chua luu duoc link proposal vao database");
                    Ultities.Telegram.pushNotify("KHÔNG TẠO ĐƯỢC PROPOSAL " + order.name, tele_group_id, tele_token);
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("BrowserActionLibProposal - saveDatabase = " + ex.ToString());
                Ultities.Telegram.pushNotify("BrowserActionLibProposal - saveDatabase " + ex.ToString(), tele_group_id, tele_token);
                return -1;
            }
        }
    }
}
