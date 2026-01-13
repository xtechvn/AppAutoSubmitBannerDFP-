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
    public class BrowserActionLibOrder
    {
        private static string tele_group_id = ConfigurationManager.AppSettings["tele_group_id"];
        private static string tele_token = ConfigurationManager.AppSettings["tele_token"];
        private ChromeDriver browers;
        private OrderViewModel order;
        private int product_id;
        private int request_id;
        private static string is_production = ConfigurationManager.AppSettings["is_production"].ToString();
        private List<ErrorModel> lstError;
        WebDriverWait wait = null;
        public BrowserActionLibOrder(ChromeDriver _browers, OrderViewModel _order, int _product_id, int _request_id, List<ErrorModel> _lstError)
        {
            browers = _browers;
            order = _order;
            product_id = _product_id;
            request_id = _request_id;
            lstError = _lstError;
            wait = new WebDriverWait(_browers, TimeSpan.FromSeconds(10));
        }

        string x_path_order_name = "//*[contains(local-name(),'material-input')] [contains(@ngcontrol,'name')]//input";
        string x_path_order_advertiser = "//*[contains(local-name(),'company-picker')] [contains(@ngcontrol,'advertiserId')]//input";
        string x_path_first_advertiser = "((//*[contains(@class,'suggestion-list')]//*[contains(@class,'list-group')])[1]//*)[1]";
        string x_path_order_sales_person = "//*[contains(local-name(),'user-picker')] [contains(@ngcontrol,'primarySalesperson')]//input";
        string x_path_order_secondary_salespeople = "//*[contains(local-name(),'user-picker')] [contains(@ngcontrol,'secondarySalespeople')]//input";
        string x_path_order_secondary_trafficker = "//*[contains(local-name(),'user-picker')] [contains(@ngcontrol,'secondaryTraffickerIds')]//input";
        string x_path_button_save = "//*[contains(local-name(),'material-button')] [contains(@debugid,'saveButton')]";
        string x_path_first_sales_person = "//*[contains(@pane-id,'TRAFFICKING-')][contains(@style,'visibility')]//*[contains(@class,'list-group')]/material-select-dropdown-item[1]";
        string x_path_secondary_sales_person = "//*[contains(@pane-id,'TRAFFICKING-')][contains(@style,'visibility')]//*[contains(local-name(),'material-select-item')][1]";
        string x_path_secondary_trafficker_person = "(//*[contains(@id,'acx-overlay-container-TRAFFICKING')]//*[contains(@pane-id,'TRAFFICKING-')][contains(@class,'visible')]//*[contains(local-name(),'material-checkbox')])[1]";
        private static string DOMAIN_WEBSITE_ORDER_DEFAULT = @"https://admanager.google.com/27973503#delivery/order/order_overview/order_id=2961633598&tab=line_items";
        private static string DOMAIN_WEBSITE_CRAWLER = ConfigurationManager.AppSettings["DOMAIN_WEBSITE_CRAWLER"];

        public bool WaitOrderReady(int index = 0)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(browers, TimeSpan.FromSeconds(20));
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(x_path_order_name)));
                return true;
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    if (order.order_default == 1)
                    {
                        browers.Url = DOMAIN_WEBSITE_ORDER_DEFAULT;
                    }
                    else
                    {
                        browers.Url = DOMAIN_WEBSITE_CRAWLER;
                    }
                    return WaitOrderReady(1);
                }
                else
                {
                    Ultities.Telegram.pushNotify("WaitOrderReady " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("WaitOrderReady [{0}] = {1}", index.ToString(), ex.ToString()));
                }
                return false;
            }
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
                string order_name = order.name.Substring(0, Math.Min(240, order.name.Length));
                Thread.Sleep(1000);
                browers.FindElement(By.XPath(x_path_order_name)).Clear();
                browers.FindElement(By.XPath(x_path_order_name)).SendKeys(order_name + (is_production == "1" ? "" : "_DEV_TEST_BOT_" + DateTime.Now.ToString()));
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
                    lstError.Add(new ErrorModel("Created Order", "addName", order.name, 1));
                    Ultities.Telegram.pushNotify("addName " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addName [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addAdvertiser(int index = 0)
        {
            try
            {
                browers.FindElement(By.XPath(x_path_order_advertiser)).Click();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(x_path_first_advertiser)));
                browers.FindElement(By.XPath(x_path_order_advertiser)).Clear();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(x_path_first_advertiser)));
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
                    lstError.Add(new ErrorModel("Created Order", "addAdvertiser", order.advertiser, 1));
                    Ultities.Telegram.pushNotify("addAdvertiser " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addAdvertiser [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addTrafficker(int index = 0)
        {
            try
            {
                string x_path = "//*[contains(local-name(),'user-picker')] [contains(@ngcontrol,'primaryTrafficker')]//input";
                string x_path_select = "//*[contains(@pane-id,'TRAFFICKING-')][contains(@style,'visibility')]//*[contains(@class,'list-group')]/material-select-dropdown-item[1]";
                browers.FindElement(By.XPath(x_path)).Click();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(x_path_select)));
                browers.FindElement(By.XPath(x_path)).Clear();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(x_path_select)));
                browers.FindElement(By.XPath(x_path)).SendKeys(order.trafficker);
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(x_path_select)));
                browers.FindElement(By.XPath(x_path_select)).Click();
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
                    lstError.Add(new ErrorModel("Created Order", "addTrafficker", order.trafficker, 1));
                    Ultities.Telegram.pushNotify("addTrafficker " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addTrafficker [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addOrderSalesPerson(int index = 0)
        {
            try
            {
                string x_path_select = "//*[contains(@pane-id,'TRAFFICKING-')][contains(@style,'visibility')]//*[contains(@class,'list-group')]/material-select-dropdown-item[1]";
                var elem = browers.FindElement(By.XPath(x_path_order_sales_person));
                Common.Common.MoveToXpath(elem, browers);
                browers.FindElement(By.XPath(x_path_order_sales_person)).Click();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(x_path_select)));
                browers.FindElement(By.XPath(x_path_order_sales_person)).Clear();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(x_path_select)));

                //browers.FindElement(By.XPath(x_path_order_sales_person)).SendKeys(order.sales_person);

                string sales_person = order.sales_person + "," + order.secondary_salespeople;
                var obj_lst_saleperson = sales_person.Split(',');
                foreach (var sale in obj_lst_saleperson)
                {
                    // add tên
                    browers.FindElement(By.XPath(x_path_order_sales_person)).Clear();
                    browers.FindElement(By.XPath(x_path_order_sales_person)).SendKeys(sale);

                    Thread.Sleep(1000);
                    // kiem tra co tim thay saleperson khong
                    if (Common.Common.checkXpathExist(x_path_first_sales_person, browers))
                    {
                        wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(x_path_first_sales_person)));
                        browers.FindElement(By.XPath(x_path_first_sales_person)).Click();
                        break;
                    }
                    else
                    {
                        // khong tim thay salesPerson se lay tu secondSalePerson add len.
                        //browers.FindElement(By.XPath(x_path_order_sales_person)).Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addOrderSalesPerson(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Order", "addOrderSalesPerson", order.sales_person, 1));
                    Ultities.Telegram.pushNotify("addOrderSalesPerson " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addOrderSalesPerson [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }
        string txt_sale_person = "//input[@aria-label='Salesperson']";
        public void addSecondarySalesPeople(int index = 0)
        {
            try
            {
                if (!string.IsNullOrEmpty(order.secondary_salespeople))
                {
                    string[] arrSecondary = order.secondary_salespeople.Split(';');
                    foreach (string sales in arrSecondary)
                    {
                        try
                        {
                            string sales_person = (Common.Common.checkXpathExist(txt_sale_person, browers) ? browers.FindElement(By.XPath(txt_sale_person)).GetAttribute("value") : "").ToLower();
                            if (sales_person.IndexOf(sales.ToLower()) == -1)
                            {
                                browers.FindElement(By.XPath(x_path_order_secondary_salespeople)).Clear();
                                browers.FindElement(By.XPath(x_path_order_secondary_salespeople)).SendKeys(sales);
                                
                                // Đợi một chút để dropdown hiển thị
                                Thread.Sleep(1000);
                                
                                // Kiểm tra có element suggestion hiển thị không
                                if (!Common.Common.checkXpathExist(x_path_secondary_sales_person, browers))
                                {
                                    Console.WriteLine(string.Format("addSecondarySalesPeople: Không tìm thấy suggestion cho '{0}'", sales));
                                    continue;
                                }
                                
                                wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(x_path_secondary_sales_person)));
                                browers.FindElement(By.XPath(x_path_secondary_sales_person)).Click();
                            }
                        }
                        catch (Exception exItem)
                        {
                            // Log lỗi cho từng item nhưng vẫn tiếp tục với item tiếp theo
                            Console.WriteLine(string.Format("addSecondarySalesPeople: Lỗi khi thêm '{0}': {1}", sales, exItem.Message));
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addSecondarySalesPeople(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Order", "addSecondarySalesPeople", order.secondary_salespeople, 1));
                    Ultities.Telegram.pushNotify("addSecondarySalesPeople " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addSecondarySalesPeople [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addSecondarytrafficker(int index = 0)
        {
            try
            {
                int same_trafficker = 0;
                if (!string.IsNullOrEmpty(order.secondary_trafficker))
                {
                    browers.FindElement(By.XPath(x_path_order_secondary_trafficker)).Clear();
                    string[] arrSecondary = order.secondary_trafficker.Split(';');
                    foreach (string sales in arrSecondary)
                    {
                        same_trafficker = sales.Trim() == order.trafficker.Trim() ? 1 : 0;
                        if (same_trafficker == 1) continue;
                        browers.FindElement(By.XPath(x_path_order_secondary_trafficker)).Clear();
                        browers.FindElement(By.XPath(x_path_order_secondary_trafficker)).SendKeys(sales.Replace(" ", ""));
                        
                        // Kiểm tra có dom suggestion show ra không                        
                        if (!(Common.Common.checkXpathExist(x_path_secondary_trafficker_person, browers)))
                        {
                            continue;
                        }

                        wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(x_path_secondary_trafficker_person)));
                        var elem = browers.FindElement(By.XPath(x_path_secondary_trafficker_person));
                        var elem_value = elem.GetAttribute("aria-checked");
                        if (elem_value == "false")
                        {
                            if (same_trafficker == 0)
                            {
                                elem.Click();
                            }
                            else
                            {
                                browers.FindElement(By.XPath(x_path_order_secondary_trafficker)).Clear();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addSecondarytrafficker(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Order", "addSecondarytrafficker", order.secondary_trafficker, 1));
                    Ultities.Telegram.pushNotify("addSecondarytrafficker " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addSecondarytrafficker [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void saveOrder(int index = 0)
        {
            string link_order = "";
            try
            {
                browers.FindElement(By.XPath(x_path_button_save)).Click();
                Thread.Sleep(2000);
                link_order = browers.Url;
                if (link_order.Contains("order_id"))
                {
                    Console.WriteLine("CREATE ORDER SUCCESS: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": " + order.name);
                }
                else
                {
                    Thread.Sleep(1000);
                    if (!link_order.Contains("order_id"))
                    {
                        index = index + 1;
                        if (index == 1)
                        {
                            saveOrder(1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    saveOrder(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Order", "saveOrder", link_order, 1));
                    string url_error = Common.Common.TakeScreen(browers, request_id, product_id);
                    Ultities.Telegram.pushNotify("saveOrder " + ex.ToString() + " #$# " + HttpUtility.UrlEncode(url_error), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("saveOrder [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public bool verifyCreatedOrder(int index = 0)
        {
            return true;
        }

        public int saveDatabase()
        {
            try
            {
                if (!Common.Common.checkXpathExist(x_path_button_save, browers))
                {
                    string link_order = browers.Url;
                    int parent_id = -1;
                    int type = (Int16)dfp_setup_type.order;
                    int iResult = Repository.updateStatusFilter(parent_id, link_order, type, product_id, request_id, "", -1, -1);
                    Ultities.Telegram.pushNotify("CREATE ORDER SUCCESS: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": " + order.name + " link order: " + HttpUtility.UrlEncode(link_order), tele_group_id, tele_token);
                    return iResult;
                }
                else
                {
                    Console.WriteLine("[ORDER] Chua luu duoc link vao database");
                    Ultities.Telegram.pushNotify("KHÔNG TẠO ĐƯỢC ORDER " + order.name, tele_group_id, tele_token);
                    return -1;
                }
            }
            catch (Exception ex)
            {
                lstError.Add(new ErrorModel("Created Order", "saveOrder", "Có lỗi khi save dữ liệu vào database", 1));
                Console.WriteLine("BrowserActionLibOrder - saveOrder = " + ex.ToString());
                Ultities.Telegram.pushNotify("saveDatabase " + ex.ToString(), tele_group_id, tele_token);
                return -1;
            }
        }
    }
}
