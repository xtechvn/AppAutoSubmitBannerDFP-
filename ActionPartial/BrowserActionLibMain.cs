using AppAutoSubmitBannerDFP.ViewModel;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Configuration;
using System.Threading;
using System.Collections.Generic;
using OpenQA.Selenium;
using AppAutoSubmitBannerDFP.Common;

namespace AppAutoSubmitBannerDFP.ActionPartial
{
    public class BrowserActionLibMain
    {
        private static string tele_group_id = ConfigurationManager.AppSettings["tele_group_id"];
        private static string tele_token = ConfigurationManager.AppSettings["tele_token"];
        private static string DOMAIN_WEBSITE_ORDER_DEFAULT = @"https://admanager.google.com/27973503#delivery/order/order_overview/order_id=2961633598&tab=line_items";
        private static string DOMAIN_WEBSITE_CRAWLER = ConfigurationManager.AppSettings["DOMAIN_WEBSITE_CRAWLER"];
        private ChromeDriver browers;
        private BannerEntities banner;
        private List<ErrorModel> lstError;
        WebDriverWait wait = null;
        private static string LogPath = AppDomain.CurrentDomain.BaseDirectory;

        public BrowserActionLibMain(ChromeDriver _browers, BannerEntities _banner, List<ErrorModel> _lstError)
        {
            browers = _browers;
            banner = _banner;
            lstError = _lstError;
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

        public void addUrl()
        {

            if (banner.order.order_default == 1)
            {
                browers.Url = DOMAIN_WEBSITE_ORDER_DEFAULT;
            }
            else
            {
                browers.Url = DOMAIN_WEBSITE_CRAWLER;
            }
        }

        public string addUrlLineItem(string url, int index = 0)
        {
            string url_line_item = string.Empty;
            try
            {
                if (url != "")
                {
                    browers.Url = url;
                }
                else
                {
                    string orderid_from_url = Common.Common.getOrderId(browers.Url);
                    if (orderid_from_url != "")
                    {
                        url_line_item = string.Format(@"https://admanager.google.com/27973503#delivery/line_item/create/order_id={0}", orderid_from_url);
                        browers.Url = url_line_item;
                    }
                    else
                    {
                        string x_path_button_new_line_item = "(//*[contains(local-name(),'toolbelt-material-menu')]//*[contains(@class,'trigger-button')])[1]";
                        browers.FindElement(By.XPath(x_path_button_new_line_item)).Click();
                    }
                }
                Thread.Sleep(3000);
                url_line_item = browers.Url;
                return url_line_item;
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    return addUrlLineItem(url, 1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "addUrlLineItem", url_line_item, 2));
                    Ultities.Telegram.pushNotify("addUrlLineItem " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addUrlLineItem [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
            return string.Empty;
        }

        public void addAdType(int ad_type, int index = 0)
        {
            try
            {
                string x_path_option_ad_type_standard = "(//*[contains(local-name(),'material-radio')]//*[contains(@role,'radio')])[1]";
                string x_path_option_ad_type_master = "(//*[contains(local-name(),'material-radio')]//*[contains(@role,'radio')])[2]";
                string x_path_select_display_ad = "//*[contains(local-name(),'material-button')][contains(@debugid,'display-select')]";
                string x_path_select_video_or_audio = "//*[contains(local-name(),'material-button')][contains(@debugid,'video-select')]";

                WebDriverWait wait = new WebDriverWait(browers, TimeSpan.FromSeconds(20));
                // Nếu vị trí là Supper mashead PC trang trong thì fix ad_type = 1
                
               // ad_type = banner.order.line_item[0].category_name.IndexOf("PC: MH") >= 0 ? 1 : ad_type;

                switch (ad_type)
                {
                    case 0:
                    case (Int16)AdsType.standard:
                        wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(x_path_option_ad_type_standard)));
                        var move_item_standard = browers.FindElement(By.XPath(x_path_option_ad_type_standard));
                        Common.Common.MoveToXpath(move_item_standard, browers);
                        Thread.Sleep(300);
                        browers.FindElement(By.XPath(x_path_option_ad_type_standard)).Click();
                        browers.FindElement(By.XPath(x_path_select_display_ad)).Click();
                        break;
                    case (Int16)AdsType.master_companion:
                        wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(x_path_option_ad_type_master)));
                        var move_item_master = browers.FindElement(By.XPath(x_path_option_ad_type_master));
                        Common.Common.MoveToXpath(move_item_master, browers);
                        Thread.Sleep(300);
                        browers.FindElement(By.XPath(x_path_option_ad_type_master)).Click();
                        browers.FindElement(By.XPath(x_path_select_display_ad)).Click();
                        break;
                    case (Int16)AdsType.video_or_audio:
                        wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(x_path_select_video_or_audio)));
                        var move_item_video = browers.FindElement(By.XPath(x_path_select_video_or_audio));
                        Common.Common.MoveToXpath(move_item_video, browers);
                        Thread.Sleep(300);
                        browers.FindElement(By.XPath(x_path_select_video_or_audio)).Click();
                        break;
                }

                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addAdType(ad_type, 1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "addAdType", ad_type.ToString(), 2));
                    Ultities.Telegram.pushNotify("addAdType " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addAdType [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addTabCreative(int index = 0)
        {
        string line_item_id_from_url = string.Empty;
            try
            {
                line_item_id_from_url = Common.Common.getLineItemId(browers.Url);
                if (line_item_id_from_url != "")
                {
                    string url_line_item = string.Format(@"https://admanager.google.com/27973503#delivery/line_item/detail/line_item_id={0}&line_item=true&li_tab=creatives", line_item_id_from_url);
                    browers.Url = url_line_item;
                }
                else
                {
                    string x_path_tab_button = "(//*[contains(@class,'tab-button')])[2]";
                    browers.FindElement(By.XPath(x_path_tab_button)).Click();
                }
                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addTabCreative(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "addTabCreative", line_item_id_from_url, 3));
                    Ultities.Telegram.pushNotify("addTabCreative " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addTabCreative [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public string addNewCreative(int item_type, int ad_type, string url, int index = 0, string ads_size = "", string add_size = "")
        {
            try
            {
                string x_path_new_creative = "";
                //if (url != "")
                //{
                //    if (ad_type != (Int32)AdsType.master_companion)
                //    {
                //        browers.Url = url;
                //    }
                //}
                //else
                //{
                //string link_create_creative = @"https://admanager.google.com/27973503#creatives/creative/create/line_item_id={0}";
                //if (item_type == (Int32)line_item_type.DFP_CPM_TOI_UU)
                //{
                //    string line_item_id_from_url = Common.Common.getLineItemId(browers.Url);
                //    if (line_item_id_from_url != "")
                //    {
                //        browers.Url = string.Format(link_create_creative, line_item_id_from_url);
                //    }
                //}
                //else
                //{
                //Xác định tạo news creative theo size
                //div[contains(@class, 'warning-container')]//span[contains(@class, 'size')]

                //wait.Until(ExpectedConditions.ElementExists(By.XPath("//div[contains(@class, 'warning-container')]//span[contains(@class, 'size')]")));

                #region check element
                IReadOnlyCollection<IWebElement> obj_elements = null;
                int maxRetries = 5;
                int retryCount = 0;
                int waitTime = 1000; // Thời gian chờ giữa các lần thử lại (ms)
                string x_path_size = "//div[contains(@class, 'warning-container')]//span[contains(@class, 'size')] | //div[contains(@class, 'placeholder-text')]//div[contains(@class, 'bold-text')]";
                obj_elements = browers.FindElements(By.XPath(x_path_size));
                while (obj_elements.Count == 0 && retryCount < maxRetries)
                {
                    try
                    {
                        obj_elements = browers.FindElements(By.XPath(x_path_size));
                        if (obj_elements.Count == 0)
                        {
                            Thread.Sleep(2000);
                        }
                        retryCount += 1;
                    }
                    catch (NoSuchElementException)
                    {
                        // Không tìm thấy phần tử, chờ và thử lại
                        System.Threading.Thread.Sleep(waitTime);
                        retryCount++;
                    }
                }
                #endregion


                int d = 0;
                bool is_click_new_create = false;
                //for (int i = 0; i < obj_elements.Count; i++)
                foreach (var item in obj_elements)
                {
                    if ((item.Text.Trim() == ads_size) || (item.Text.Trim().ToLower().IndexOf(add_size.ToLower()) >= 0))
                    {
                        x_path_new_creative = "(//*[contains(@debugid,'create-creative')])[" + (d + 1) + "]";
                        browers.FindElement(By.XPath(x_path_new_creative)).Click();
                        is_click_new_create = true;
                        System.Threading.Thread.Sleep(500);

                        break;
                    }
                    d += 1; // Kiểm tra có click chuyển không
                }
                if (!is_click_new_create)
                {
                    x_path_new_creative = "//*[contains(@debugid,'create-creative')]";
                    if (Common.Common.checkXpathExist(x_path_new_creative, browers))
                    {
                        browers.FindElement(By.XPath(x_path_new_creative)).Click();
                        is_click_new_create = true;
                        d += 1;
                    }
                }
                //}
                Thread.Sleep(2000);
                string temp_url = (!is_click_new_create ? "" : browers.Url);// nếu d =0 nghĩa là không có link creative nào nữa
                return temp_url;
            }

            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    return addNewCreative(item_type, ad_type, url, 1, ads_size, add_size);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "addNewCreative", item_type.ToString() + "|" + ad_type.ToString() + "|" + url, 3));
                    Ultities.Telegram.pushNotify("addNewCreative " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addNewCreative [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
            return string.Empty;
        }
        // Quy định điều hướng tạo Creative
        public string get_creative_template(string position, string url, string creative_template)
        {
            var dimensions = GetDimensionsFromUrl(url);
            if (dimensions != null)
            {
                if (dimensions["width"] == "1" && dimensions["height"] == "1")
                {
                    switch (position.ToLower())
                    {
                        case "sticky":
                        case "bottom sticky":
                            return "Standard banner";
                        case "popup":
                            return "Popup";
                        case "in image":
                            return "Popup";
                        default:
                            break;
                    }
                }
            }
            return creative_template;
        }
        // Hàm để lấy width và height từ URL
        public Dictionary<string, string> GetDimensionsFromUrl(string url)
        {
            try
            {
                // Phân tách phần query từ URL
                Uri uri = new Uri(url);
                string[] queryParts = uri.Fragment.Split(new char[] { '&', '=' }, StringSplitOptions.RemoveEmptyEntries);

                // Duyệt các tham số trong query để tìm width và height
                Dictionary<string, string> queryParams = new Dictionary<string, string>();
                for (int i = 0; i < queryParts.Length; i += 2)
                {
                    if (i + 1 < queryParts.Length)
                    {
                        queryParams[queryParts[i]] = queryParts[i + 1];
                    }
                }

                // Kiểm tra và trả về width, height nếu có
                if (queryParams.ContainsKey("width") && queryParams.ContainsKey("height"))
                {
                    return new Dictionary<string, string>
                {
                    { "width", queryParams["width"] },
                    { "height", queryParams["height"] }
                };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi phân tích URL: {ex.Message}");
            }
            return null;
        }

        public void addStandardCreative(int item_type, int ad_type, int creative_type, string creative_template, string add_sizes, bool first, int index = 0, string position_name = "")
        {
            try
            {
                if (ad_type == (Int32)AdsType.master_companion && first == false) return;
                string x_path_title_move = "//*[contains(local-name(),'creator-card')]//*[contains(@class,'title')]";
                string x_path_template_move = "//*[contains(local-name(),'creator-card')][{0}]";
                string x_path_custom_creative_template = "//*[contains(local-name(),'creator-card')][{0}]//*[contains(@aria-label,'Select a template')]";
                string x_path_first_custom_creative_template = "(//*[contains(@class,'suggestion-list')]//*[contains(@class,'list-group')]//*[contains(local-name(),'material-select-dropdown-item')])[1]";
                string x_path_custom_creative_continue = "//*[contains(@class,'continue-button')][contains(@aria-disabled,'false')]";

                string x_path_native_format = "//*[contains(local-name(),'creator-card')][{0}]//*[contains(@aria-label,'Select a template')]";
                string x_path_first_nvative_format = "(//*[contains(@class,'suggestion-list')]//*[contains(@class,'list-group')]//*[contains(local-name(),'material-select-dropdown-item')])[1]";
                string x_path_nvative_format_continue = "//*[contains(@class,'continue-button')][contains(@aria-disabled,'false')]";

                string x_path_code_select = "//*[contains(@class,'select-button')][contains(@aria-label,'Select Third party creative')][contains(@aria-disabled,'false')]";
                string x_path_custom_creative_tmp = "//div[contains(@class, 'title') and contains(text(), 'Custom creative template')]";

                int creator_card_index = 7;

                // TÍnh lại creative_template
                // cuonglv8: 18-12-2024
                creative_template = get_creative_template(position_name, browers.Url, creative_template);


                // Sử dụng WebDriverWait để chờ đợi phần tử xuất hiện khi set Creative
                if (Common.Common.checkXpathExist(x_path_custom_creative_tmp, browers))
                {
                    WebDriverWait wait = new WebDriverWait(browers, TimeSpan.FromSeconds(10));
                    IWebElement element = wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(x_path_custom_creative_tmp))
                    );
                }

                if (Common.Common.checkXpathExist(x_path_custom_creative_template, browers))
                {
                    WebDriverWait wait = new WebDriverWait(browers, TimeSpan.FromSeconds(10));
                    IWebElement element = wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(x_path_custom_creative_template))
                    );
                }

                if (Common.Common.checkXpathExist(x_path_title_move, browers))
                {
                    WebDriverWait wait = new WebDriverWait(browers, TimeSpan.FromSeconds(10));
                    IWebElement element = wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(x_path_title_move))
                    );
                }


                if (creative_template == "Code Tracking")
                {
                    Thread.Sleep(500);
                    var elem_code = browers.FindElementsByXPath(x_path_title_move);
                    if (elem_code.Count > 0)
                    {
                        for (int i = 0; i <= elem_code.Count - 1; i++)
                        {
                            string value = elem_code[i].Text;
                            if (value.ToLower() == "third party")
                            {
                                creator_card_index = i + 1;
                                break;
                            }
                        }
                    }
                    // check xpath
                    if (Common.Common.checkXpathExist(string.Format(x_path_custom_creative_template, creator_card_index), browers))
                    {
                        WebDriverWait wait = new WebDriverWait(browers, TimeSpan.FromSeconds(10));
                        IWebElement element = wait.Until(
                            SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(string.Format(x_path_custom_creative_template, creator_card_index)))
                        );
                    }
                    else
                    {
                        return;
                    }

                    var elem_move_code = browers.FindElement(By.XPath(string.Format(x_path_template_move, creator_card_index)));
                    Common.Common.MoveToXpath(elem_move_code, browers);

                    Thread.Sleep(300);
                    browers.FindElement(By.XPath(x_path_code_select)).Click();

                    Thread.Sleep(1000);
                }
                else if (creative_type == (Int16)standard_creative.SITE_CO_DINH || creative_template.ToLower() == "iframer tracking")
                {
                    Thread.Sleep(500);
                    // check xpath
                    if (!(Common.Common.checkXpathExist(x_path_title_move, browers)))
                    {
                        return;
                    }

                    var elem_custom = browers.FindElementsByXPath(x_path_title_move);
                    if (elem_custom.Count > 0)
                    {
                        for (int i = 0; i <= elem_custom.Count - 1; i++)
                        {
                            string value = elem_custom[i].Text;
                            if (value.ToLower() == "custom creative template")
                            {
                                creator_card_index = i + 1;
                                break;
                            }
                        }
                    }


                    // check xpath
                    if (!(Common.Common.checkXpathExist(string.Format(x_path_template_move, creator_card_index), browers)))
                    {
                        return;
                    }

                    var elem_move_cd = browers.FindElement(By.XPath(string.Format(x_path_template_move, creator_card_index)));
                    Common.Common.MoveToXpath(elem_move_cd, browers);

                    Thread.Sleep(300);



                    var elem_creative_template = browers.FindElement(By.XPath(string.Format(x_path_custom_creative_template, creator_card_index)));

                    elem_creative_template.Clear();

                    Thread.Sleep(300);
                    elem_creative_template.SendKeys(creative_template);

                    Thread.Sleep(1500);
                    // Sử dụng WebDriverWait và MoveToXpath để đảm bảo element có thể click được
                    WebDriverWait waitClick = new WebDriverWait(browers, TimeSpan.FromSeconds(10));
                    var elem_first_template = waitClick.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath(x_path_first_custom_creative_template))
                    );
                    Common.Common.MoveToXpath(elem_first_template, browers);
                    Thread.Sleep(300);
                    elem_first_template.Click();

                    Thread.Sleep(500);
                    // Tương tự cho continue button
                    var elem_continue = waitClick.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath(x_path_custom_creative_continue))
                    );
                    Common.Common.MoveToXpath(elem_continue, browers);
                    Thread.Sleep(300);
                    elem_continue.Click();

                    Thread.Sleep(1000);
                }
                else if (item_type == (Int32)line_item_type.DFP_CPM_TOI_UU && creative_type == (Int16)standard_creative.SITE_KHONG_CO_DINH)
                {
                    Thread.Sleep(500);
                    var elem_custom = browers.FindElementsByXPath(x_path_title_move);
                    if (elem_custom.Count > 0)
                    {
                        for (int i = 0; i <= elem_custom.Count - 1; i++)
                        {
                            string value = elem_custom[i].Text;
                            if (value.ToLower() == "native format")
                            {
                                creator_card_index = i + 1;
                                break;
                            }
                        }
                    }
                    var elem_move = browers.FindElement(By.XPath(string.Format(x_path_template_move, creator_card_index)));
                    Common.Common.MoveToXpath(elem_move, browers);

                    Thread.Sleep(300);
                    var elem_native_template = browers.FindElement(By.XPath(string.Format(x_path_native_format, creator_card_index)));

                    elem_native_template.Clear();

                    Thread.Sleep(300);
                    elem_native_template.SendKeys(add_sizes);

                    Thread.Sleep(1000);
                    // Sử dụng WebDriverWait và MoveToXpath để đảm bảo element có thể click được
                    WebDriverWait waitNative = new WebDriverWait(browers, TimeSpan.FromSeconds(10));
                    var elem_first_native = waitNative.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath(x_path_first_nvative_format))
                    );
                    Common.Common.MoveToXpath(elem_first_native, browers);
                    Thread.Sleep(300);
                    elem_first_native.Click();

                    Thread.Sleep(500);
                    // Tương tự cho continue button
                    var elem_native_continue = waitNative.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath(x_path_nvative_format_continue))
                    );
                    Common.Common.MoveToXpath(elem_native_continue, browers);
                    Thread.Sleep(300);
                    elem_native_continue.Click();

                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addStandardCreative(item_type, ad_type, creative_type, creative_template, add_sizes, first, 1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "addStandardCreative", item_type.ToString() + "|" + ad_type.ToString() + "|" + creative_type.ToString() + "|" + creative_template + "|" + add_sizes + "|" + first.ToString(), 3));
                    //Ultities.Telegram.pushNotify("addStandardCreative " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addStandardCreative [{0}] = {1}", index.ToString(), ex.ToString()));
                    ErrorWriter.WriteLog(LogPath, "addStandardCreative - FINAL ERROR", string.Format("Failed after retry. Details: {0}, Error: addStandardCreative",  ex.ToString()));
                }
            }
        }

        public void addNewCreativeSync(int ad_type, bool first, string creative_template, int index = 0)
        {
            try
            {
                int creator_card_index = 8;

                string x_path_button_close = "//*[contains(local-name(),'material-button')][contains(@aria-label,'Close')]";
                string x_path_title_move = "//*[contains(local-name(),'creator-card')]//*[contains(@class,'title')]";
                string x_path_template_move = "//*[contains(local-name(),'creator-card')][{0}]";
                string x_path_add_new_creative = "//*[contains(@debug-id,'dropdown-menu-size')]/dropdown-button";
                string x_path_choice_item_creative = "//*[contains(@class,'visible')]//*[contains(@class,'item-group-list')]//*[contains(local-name(),'material-select-item')][1]";
                string x_path_custom_creative_template = "//*[contains(local-name(),'creator-card')][{0}]//*[contains(@aria-label,'Select a template')]";
                string x_path_first_custom_creative_template = "(//*[contains(@class,'suggestion-list')]//*[contains(@class,'list-group')]//*[contains(local-name(),'material-select-dropdown-item')])[1]";
                string x_path_custom_creative_continue = "//*[contains(@class,'continue-button')][contains(@aria-disabled,'false')]";
                string x_path_custom_creative_template_2 = "//material-select-dropdown-item[.//span[text()='{template_name}']]";
                if (Common.Common.checkXpathExist(x_path_button_close, browers))
                {
                    browers.FindElement(By.XPath(x_path_button_close)).Click();
                    Thread.Sleep(500);
                }

                browers.FindElement(By.XPath(x_path_add_new_creative)).Click();
                Thread.Sleep(1000);
                browers.FindElement(By.XPath(x_path_choice_item_creative)).Click();

                Thread.Sleep(2000);

                var elem_custom = browers.FindElementsByXPath(x_path_title_move);
                if (elem_custom.Count > 0)
                {
                    for (int i = 0; i <= elem_custom.Count - 1; i++)
                    {
                        string value = elem_custom[i].Text;
                        if (value.ToLower() == "custom creative template")
                        {
                            creator_card_index = i + 1;
                            break;
                        }
                    }
                }
                var elem_move_cd = browers.FindElement(By.XPath(string.Format(x_path_template_move, creator_card_index)));
                Common.Common.MoveToXpath(elem_move_cd, browers);

                browers.FindElement(By.XPath(string.Format(x_path_custom_creative_template, creator_card_index))).SendKeys(creative_template);

                Thread.Sleep(1500);
                browers.FindElement(By.XPath(x_path_custom_creative_template_2.Replace("{template_name}", creative_template))).Click();
                Thread.Sleep(1500);
                browers.FindElement(By.XPath(x_path_custom_creative_continue)).Click();
                Thread.Sleep(2000);

            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addNewCreativeSync(ad_type, first, creative_template, 1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "addNewCreativeSync", ad_type.ToString() + "|" + creative_template + "|" + first.ToString(), 3));
                    Ultities.Telegram.pushNotify("addNewCreativeSync " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addNewCreativeSync [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }
    }
}
