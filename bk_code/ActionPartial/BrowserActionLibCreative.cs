using AppAutoSubmitBannerDFP.Common;
using AppAutoSubmitBannerDFP.Model;
using AppAutoSubmitBannerDFP.ViewModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace AppAutoSubmitBannerDFP.ActionPartial
{
    public class BrowserActionLibCreative
    {
        private static string tele_group_id = ConfigurationManager.AppSettings["tele_group_id"];
        private static string tele_token = ConfigurationManager.AppSettings["tele_token"];

        private static string is_production = ConfigurationManager.AppSettings["is_production"].ToString();
        private ChromeDriver browers;
        private CreativeViewModel creative;
        private int product_id;
        private int request_id;
        private int line_item_id;
        private string add_size;
        private int creative_line_item_type;
        private List<ErrorModel> lstError;
        WebDriverWait wait = null;
        public BrowserActionLibCreative(ChromeDriver _browers, CreativeViewModel _creative, int _product_id, int _request_id, int _line_item_id, int _line_item_type, List<ErrorModel> _lstError)
        {
            browers = _browers;
            creative = _creative;
            product_id = _product_id;
            line_item_id = _line_item_id;
            request_id = _request_id;
            add_size = _creative.add_sizes;
            creative_line_item_type = _line_item_type;
            lstError = _lstError;
            wait = new WebDriverWait(_browers, TimeSpan.FromSeconds(10));
        }

        #region CREATIVE

        string x_path_creative_name = "//*[contains(@debugid,'creative-name')]//*[contains(local-name(),'input')]";
        string x_path_through_url_0 = "//*[contains(@ngcontrol,'destinationUrl')]//*[contains(local-name(),'textarea')]|//input[contains(@aria-label, 'Click-through URL')] | //click-through-url-input//textarea[contains(@class, 'input-area')]";// creative_type = 0
        string x_path_through_url_1 = "//span[text()='Click-through URL']/ancestor::label/following::textarea[1] | *//div[2]/div/url-variable/*//div[1]/div[1]/label/input | //input[contains(@aria-label, 'Click-through URL')] | //click-through-url-input//textarea[contains(@class, 'input-area')]"; // creative_type = 1

        string x_path_through_url_2 = "//div[contains(@class, 'creator-card') and .//div[contains(text(), 'Custom creative template')]]//drx-form-field//input[@type='text' and contains(@aria-label, 'Select a template')]";

        string x_path_tagert_adsize = "//*[contains(@ngcontrol,'size')]//input[contains(@aria-label,'Target ad unit size')]";
        string x_path_select_adsize = "//*[contains(@container-name,'CREATIVES')]//*[contains(@pane-id,'CREATIVES-')][contains(@class,'visible')]//*[contains(local-name(),'material-list')]//*[contains(local-name(),'material-select-dropdown-item')][1]";

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

        public bool WaitOrderReady(int index = 0)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(browers, TimeSpan.FromSeconds(20));
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(x_path_through_url_2)));
                return true;
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
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

        public void creativeName(int index = 0)
        {
            try
            {
                if (!string.IsNullOrEmpty(creative.name))
                {
                    Thread.Sleep(500);
                    var elem = browers.FindElement(By.XPath(x_path_creative_name));
                    Common.Common.MoveToXpath(elem, browers);

                    elem.Clear();
                    elem.SendKeys(creative.name + (is_production == "1" ? "" : "_DEV_TEST_BOT_" + DateTime.Now.ToString()));
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    creativeName(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "creativeName", creative.name, 3));
                    Ultities.Telegram.pushNotify("creativeName " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("creativeName [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void tagertAdSize(int index = 0)
        {
            try
            {
                if (creative.add_sizes != "" && creative_line_item_type == (Int16)line_item_type.DFP_CPM_TOI_UU && creative.creative_type == 1)
                {
                    // fill Name
                    Thread.Sleep(200);
                    if (Common.Common.checkXpathExist(x_path_tagert_adsize, browers))
                    {
                        wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(x_path_tagert_adsize)));
                        browers.FindElement(By.XPath(x_path_tagert_adsize)).Clear();
                        Thread.Sleep(2000);
                        browers.FindElement(By.XPath(x_path_tagert_adsize)).SendKeys(creative.add_sizes);
                        wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(x_path_select_adsize)));
                        browers.FindElement(By.XPath(x_path_select_adsize)).Click();
                        Thread.Sleep(200);
                    }

                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    tagertAdSize(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "tagertAdSize", creative.add_sizes, 3));
                    Ultities.Telegram.pushNotify("tagertAdSize " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("tagertAdSize [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void clickThroughUrl(int index = 0)
        {
            if (creative.creative_template == "Code Tracking") return;

            try
            {
                switch (creative.creative_type)
                {
                    case (Int16)standard_creative.SITE_CO_DINH:
                        //x_path_through_url_1 = x_path_through_url_0 + "|" + x_path_through_url_1;
                        browers.FindElement(By.XPath(x_path_through_url_1)).Clear();
                        browers.FindElement(By.XPath(x_path_through_url_1)).SendKeys(creative.click_through_url);

                        break;
                    case (Int16)standard_creative.SITE_KHONG_CO_DINH:
                        // fill click_through_url
                        browers.FindElement(By.XPath(x_path_through_url_0)).Clear();
                        browers.FindElement(By.XPath(x_path_through_url_0)).SendKeys(creative.click_through_url);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    clickThroughUrl(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "clickThroughUrl", creative.creative_type.ToString(), 3));
                    Ultities.Telegram.pushNotify("clickThroughUrl " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("clickThroughUrl [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void iframeUrl(int index = 0)
        {
            if (creative.creative_template == "Code Tracking") return;

            try
            {
                //if (creative.creative_template == "Iframer Tracking")
                //{
                string x_path_label_iframe_url = "//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//*[contains(@class,'label-container')]//span|//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//*[contains(@class,'input-container')]//span";
                var elem = browers.FindElements(By.XPath(x_path_label_iframe_url));
                string elem_text = string.Empty;
                if (elem.Count > 0)
                {
                    foreach (var obj in elem)
                    {
                        if (obj.Text == "iFrame URL" || obj.Text == "URL of the HTML5 ad" || obj.Text == "iframe_url" || obj.Text == "iFrameURL" || obj.Text == "HTML5 URL")
                        {
                            obj.FindElement(By.XPath("../../../..//input")).Clear();
                            obj.FindElement(By.XPath("../../../..//input")).SendKeys(creative.iframe_url);
                            return;
                        }
                    }
                }
                //}
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    iframeUrl(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "clickThroughUrl", creative.iframe_url, 3));
                    Ultities.Telegram.pushNotify("iframeUrl " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("iframeUrl [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        private string download_file()
        {
            string file_extension = "";
            string file_download = "";
            string file_name = "";
            string sDay = string.Format("{0:dd}", DateTime.Now);
            string sMonth = string.Format("{0:MM}", DateTime.Now);
            string sYear = DateTime.Now.Year.ToString();
            string file_path = string.Format("D:\\{0}\\{1}\\{2}", sYear, sMonth, sDay);
            if (creative.iframe_url != "" && creative.creative_template == "Image Tracking")
            {
                if (creative.iframe_url.ToLower().IndexOf(".") >= 0)
                {
                    string[] file_split_name = creative.iframe_url.Split('/');
                    file_name = file_split_name[file_split_name.Length - 1];
                    string[] file_split = file_name.Split('.');
                    file_extension = file_split[file_split.Length - 1];
                    for (int k = 0; k <= file_split.Length - 2; k++)
                    {
                        file_name = file_split[k] + "_";
                    }
                    file_name = file_name + Common.Common.ToUnixTimeMilliSeconds(DateTime.Now) + "." + file_extension;
                    WebClient client = new WebClient();
                    Stream stream = client.OpenRead(creative.iframe_url);
                    Bitmap bitmap;
                    bitmap = new Bitmap(stream);
                    if (bitmap != null)
                    {
                        //Tạo thư mục nếu chưa có
                        if (!Directory.Exists(file_path))
                        {
                            Directory.CreateDirectory(file_path);
                        }
                        file_download = file_path + "\\" + file_name;
                        bitmap.Save(file_download);
                    }
                    stream.Flush();
                    stream.Close();
                    client.Dispose();
                }
            }
            return file_download;
        }

        public void upload_file(int index = 0)
        {
            try
            {
                if (creative.creative_template == "Image Tracking")
                {
                    string file_upload = download_file();
                    if (file_upload != "")
                    {
                        string x_path_exists_cancel = "//*[contains(@id,'acx-overlay-container-CREATIVES')]//*[contains(@pane-id,'CREATIVES-')][contains(@class,'visible')]//footer//*[contains(local-name(),'material-button')][2]";
                        if (creative.add_sizes != ADD_SIZE.SIZE_INPAGE_IMAGES)
                        {
                            //string x_path_upload_file_button = "//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//*[contains(local-name(),'material-file-upload')]//*[contains(local-name(),'material-button')][contains(@aria-label,'Browse files to upload')]";
                            string x_path_input_upload = "//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//*[contains(local-name(),'material-file-upload')]//*[contains(local-name(),'input')][contains(@type,'file')]";
                            //browers.FindElement(By.XPath(x_path_upload_file_button)).Click();
                            if (Common.Common.checkXpathExist(x_path_input_upload, browers))
                            {
                                var fileInput = browers.FindElement(By.XPath(x_path_input_upload));
                                fileInput.SendKeys(file_upload);
                            }
                        }
                        else
                        {
                            string x_path_label_upload_inpage = "//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//*[contains(@class,'label-container')]//span|//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//*[contains(@class,'input-container')]//span";
                            var elem = browers.FindElements(By.XPath(x_path_label_upload_inpage));
                            string elem_text = string.Empty;
                            if (elem.Count > 0)
                            {
                                foreach (var obj in elem)
                                {
                                    if (obj.Text == "image_portrait")
                                    {
                                        Common.Common.MoveToXpath(obj, browers);
                                        var fileInputInage = obj.FindElement(By.XPath("../../../..//*//*[contains(local-name(),'input')][contains(@type,'file')]"));
                                        fileInputInage.SendKeys(file_upload);
                                    }
                                }
                            }
                        }

                        Thread.Sleep(5000);
                        if (Common.Common.checkXpathExist(x_path_exists_cancel, browers))
                        {
                            browers.FindElement(By.XPath(x_path_exists_cancel)).Click();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    upload_file(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "clickThroughUrl", creative.iframe_url, 3));
                    Ultities.Telegram.pushNotify("upload_file " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("upload_file [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void selectOverlayCard(int index = 0)
        {
            if (creative.creative_template == "Code Tracking") return;

            try
            {
                if (creative.overlay_card != 2) return;

                string x_path_label_overlay_card = "//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//*[contains(@class,'label-container')]//span|//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//*[contains(@class,'input-container')]//span";
                string x_path_select_overlay_card = "//*[contains(@class,'material-dropdown-select-popup')][contains(@class,'visible')]//*[contains(local-name(),'material-select-dropdown-item')][2]";
                var elem = browers.FindElements(By.XPath(x_path_label_overlay_card));
                if (elem.Count > 0)
                {
                    foreach (var obj in elem)
                    {
                        if (obj.Text == "Overlay Card")
                        {
                            Common.Common.MoveToXpath(obj, browers);
                            obj.FindElement(By.XPath("//div[@aria-label='Overlay Card']")).Click();
                            //obj.FindElement(By.XPath("../../../..//*[contains(local-name(),'material-dropdown-select')]")).Click();                            
                            Thread.Sleep(100);
                            browers.FindElement(By.XPath(x_path_select_overlay_card)).Click();
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    selectOverlayCard(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "selectOverlayCard", creative.overlay_card.ToString(), 3));
                    Ultities.Telegram.pushNotify("selectOverlayCard " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("selectOverlayCard [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void selectInputsize(int index = 0)
        {
            if (creative.creative_template == "Code Tracking") return;

            try
            {
                if (creative.input_size == "") return;
                string x_path_label_input_size = "//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//*[contains(@class,'label-container')]//span|//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//*[contains(@class,'input-container')]//span";
                var elem = browers.FindElements(By.XPath(x_path_label_input_size));
                string elem_text = string.Empty;
                if (elem.Count > 0)
                {
                    foreach (var obj in elem)
                    {
                        if (obj.Text == "inputSize")
                        {

                            Common.Common.MoveToXpath(obj, browers);
                            obj.FindElement(By.XPath("../../../../..//*[contains(local-name(),'dropdown-button')]")).Click();
                            Thread.Sleep(500);
                            string x_path_item_input_size = "//*[contains(@class,'material-dropdown-select-popup')][contains(@class,'visible')]//*[contains(local-name(),'material-select-dropdown-item')]/span";
                            var input_size = creative.input_size;
                            var node = browers.FindElements(By.XPath(x_path_item_input_size));
                            int d = 0;
                            foreach (var item in node)
                            {
                                if (item.Text == input_size)
                                {
                                    item.FindElement(By.XPath("..")).Click();
                                    return;
                                }
                                d += 1;
                            }
                            if (d == 0)
                            {
                                Console.WriteLine("selectInputsize invalid !!! ");
                            }
                            else
                            {
                                node[0].FindElement(By.XPath("..")).Click();
                                Console.WriteLine("Inputsize not found !!! ");
                            }
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    selectInputsize(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "selectInputsize", creative.input_size, 3));
                    Ultities.Telegram.pushNotify("selectInputsize " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("selectInputsize [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        // Chọn sticky size nếu có
        public void selectStickySize(string ads_size)
        {
            try
            {
                string x_path_sticky_size = "//material-select-dropdown-item[.//span[text()='" + ads_size + "']]";
                string x_path_expand_sticky = "//dropdown-button[.//span[contains(text(), 'Sticky size')]]//material-icon[contains(., 'arrow_drop_down')]";
                string x_path_expand_banner = "//dropdown-button[.//span[contains(text(), 'Banner size')]]//material-icon[contains(., 'arrow_drop_down')]";

                string x_path_expand = null;
                if (Common.Common.checkXpathExist(x_path_expand_sticky, browers))
                {
                    x_path_expand = x_path_expand_sticky;
                }
                else if (Common.Common.checkXpathExist(x_path_expand_banner, browers))
                {
                    x_path_expand = x_path_expand_banner;
                }

                if (x_path_expand != null)
                {
                    var dropdownButton = browers.FindElement(By.XPath(x_path_expand));
                    dropdownButton.Click();
                    Thread.Sleep(1000);
                    if (Common.Common.checkXpathExist(x_path_sticky_size, browers))
                    {
                        browers.FindElement(By.XPath(x_path_sticky_size)).Click();
                    }
                }
            }
            catch (Exception ex)
            {
                lstError.Add(new ErrorModel("Created Creative", "selectStickySize", creative.creative_template, 3));
                Ultities.Telegram.pushNotify("selectStickySize " + ex.ToString(), tele_group_id, tele_token);
                Console.WriteLine(string.Format("selectStickySize [{0}] = {1}", ads_size, ex.ToString()));
            }
        }

        // Chọn banner size với Creative type: Creative template - Standard banner (Out of page)
        public void selectBannerSize(string ads_size)
        {
            try
            {
                if (creative.creative_template.ToLower().IndexOf("standard banner") >= 0)
                {

                    string x_path_sticky_size = "//material-select-dropdown-item[.//span[text()='" + ads_size + "']]";
                    string x_path_expand = "//dropdown-button[.//span[contains(text(), 'Sticky size')]]//material-icon[contains(., 'arrow_drop_down')]";

                    if (Common.Common.checkXpathExist(x_path_expand, browers))
                    {
                        var dropdownButton = browers.FindElement(By.XPath(x_path_expand));
                        dropdownButton.Click();
                        Thread.Sleep(1000);
                        if (Common.Common.checkXpathExist(x_path_sticky_size, browers))
                        {
                            browers.FindElement(By.XPath(x_path_sticky_size)).Click();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lstError.Add(new ErrorModel("Created Creative", "selectStickySize", creative.creative_template, 3));
                Ultities.Telegram.pushNotify("selectStickySize " + ex.ToString(), tele_group_id, tele_token);
                Console.WriteLine(string.Format("selectStickySize [{0}] = {1}", ads_size, ex.ToString()));
            }
        }


        public void selectRunMode(int index = 0)
        {
            try
            {
                if (creative.creative_template == "Code Tracking") return;

                string x_path_label_input_size = "//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//*[contains(@class,'label-container')]//span|//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//*[contains(@class,'input-container')]//span";
                var elem = browers.FindElements(By.XPath(x_path_label_input_size));
                string elem_text = string.Empty;
                if (elem.Count > 0)
                {
                    foreach (var obj in elem)
                    {
                        if (obj.Text.Replace(" ", "").ToLower() == "runmode")
                        {
                            Common.Common.MoveToXpath(obj, browers);
                            obj.FindElement(By.XPath("../../../../..//*[contains(local-name(),'dropdown-button')]")).Click();
                            Thread.Sleep(500);

                            string x_path_run_mode = "//material-select-dropdown-item[.//span[text()='" + creative.run_mode.ToUpper() + "']]|//material-select-dropdown-item[.//span[text()='" + creative.run_mode.ToLower() + "']]";
                            Thread.Sleep(1000);
                            if (Common.Common.checkXpathExist(x_path_run_mode, browers))
                            {
                                obj.FindElement(By.XPath(x_path_run_mode)).Click();
                            }
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    selectRunMode(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "selectRunMode", creative.creative_template, 3));
                    Ultities.Telegram.pushNotify("selectRunMode " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("selectRunMode [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void selectVideo(int index = 0)
        {
            if (creative.creative_template == "Code Tracking") return;

            try
            {
                if (add_size == ADD_SIZE.SIZE_BREAKS_PAGE_MOBILE)
                {
                    string x_path_type = "//*[contains(@debugid,'video-asset-type')]/material-radio[2]";
                    string x_path_scroll = "//*[contains(@ngcontrolgroup,'traffickedNativeVideoDto')]";
                    var actions = new Actions(browers);
                    var element = browers.FindElement(By.XPath(x_path_scroll));
                    actions.MoveToElement(element);
                    actions.Perform();
                    Thread.Sleep(100);
                    browers.FindElement(By.XPath(x_path_type)).Click();
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    selectVideo(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "selectVideo", add_size, 3));
                    Ultities.Telegram.pushNotify("selectVideo " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("selectVideo [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void indexIndustrial(int index = 0)
        {
            if (creative.creative_template == "Code Tracking") return;

            try
            {
                if (creative.index_industrial == string.Empty) return;

                string x_path_label_input_industrial = "//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//*[contains(@class,'label-container')]//span|//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//*[contains(@class,'input-container')]//span";
                var elem = browers.FindElements(By.XPath(x_path_label_input_industrial));
                if (elem.Count > 0)
                {
                    foreach (var obj in elem)
                    {
                        if (obj.Text == "Index_Industrial")
                        {
                            Common.Common.MoveToXpath(obj, browers);
                            obj.FindElement(By.XPath("../../../..//input")).Clear();
                            obj.FindElement(By.XPath("../../../..//input")).SendKeys(creative.index_industrial);
                            Thread.Sleep(100);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    indexIndustrial(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "indexIndustrial", creative.index_industrial, 3));
                    Ultities.Telegram.pushNotify("indexIndustrial " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("indexIndustrial [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void indexBrand(int index = 0)
        {
            if (creative.creative_template == "Code Tracking") return;

            try
            {
                if (creative.index_brand == string.Empty) return;

                string x_path_label_input_brand = "//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//*[contains(@class,'label-container')]//span|//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//*[contains(@class,'input-container')]//span";
                var elem = browers.FindElements(By.XPath(x_path_label_input_brand));
                if (elem.Count > 0)
                {
                    foreach (var obj in elem)
                    {
                        if (obj.Text == "Index_Brand")
                        {
                            Common.Common.MoveToXpath(obj, browers);
                            obj.FindElement(By.XPath("../../../..//input")).Clear();
                            obj.FindElement(By.XPath("../../../..//input")).SendKeys(creative.index_brand);
                            Thread.Sleep(100);
                            return;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    indexBrand(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "indexBrand", creative.index_brand, 3));
                    Ultities.Telegram.pushNotify("indexBrand " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("indexBrand [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void campaignName(int index = 0)
        {
            if (creative.creative_template == "Code Tracking") return;

            try
            {
                if (creative.campaign_name == string.Empty) return;

                string x_path_label_input_brand = "//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//*[contains(@class,'label-container')]//span|//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//*[contains(@class,'input-container')]//span";
                var elem = browers.FindElements(By.XPath(x_path_label_input_brand));
                if (elem.Count > 0)
                {
                    foreach (var obj in elem)
                    {
                        if (obj.Text == "Campaign_Name")
                        {
                            Common.Common.MoveToXpath(obj, browers);
                            obj.FindElement(By.XPath("../../../..//input")).Clear();
                            obj.FindElement(By.XPath("../../../..//input")).SendKeys(creative.campaign_name);
                            Thread.Sleep(100);
                            return;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    campaignName(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "campaignName", creative.campaign_name, 3));
                    Ultities.Telegram.pushNotify("campaignName " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("campaignName [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void thirdPartyImpression(int index = 0)
        {
            if (creative.creative_template == "Code Tracking") return;

            try
            {
                string x_path_label_input_third_party_tracking = "//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//*[contains(@class,'label-container')]//span|//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//*[contains(@class,'input-container')]//span";

                if (creative.third_party_tracking.Count > 0)
                {
                    string thirparty = creative.third_party_tracking[0];
                    if (thirparty != "")
                    {

                        var elem = browers.FindElements(By.XPath(x_path_label_input_third_party_tracking));
                        if (elem.Count > 0)
                        {
                            foreach (var obj in elem)
                            {
                                if (obj.Text.Trim().ToLower() == "third-party impression url" || obj.Text.Trim().ToLower() == "thirdpartyimpressiontracker" || obj.Text.Trim().ToLower() == "third-party impression tracker" || obj.Text.Trim().ToLower() == "thirdpartyimpressionurl")
                                {
                                    Common.Common.MoveToXpath(obj, browers);
                                    obj.FindElement(By.XPath("../../../..//input")).Clear();
                                    obj.FindElement(By.XPath("../../../..//input")).SendKeys(thirparty);
                                    Thread.Sleep(100);
                                    break;
                                }
                            }
                        }
                    }
                }

                if (creative.third_party_tracking.Count > 1)
                {
                    string thirparty2 = creative.third_party_tracking[1];
                    if (thirparty2 != "")
                    {
                        var elem = browers.FindElements(By.XPath(x_path_label_input_third_party_tracking));
                        if (elem.Count > 0)
                        {
                            foreach (var obj in elem)
                            {
                                if (obj.Text.Trim() == "ThirdpartyimpressionURL_2" || obj.Text.Trim() == "Third-party impression tracker2")
                                {
                                    Common.Common.MoveToXpath(obj, browers);
                                    obj.FindElement(By.XPath("../../../..//input")).Clear();
                                    obj.FindElement(By.XPath("../../../..//input")).SendKeys(thirparty2);
                                    Thread.Sleep(100);
                                    break;
                                }
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
                    thirdPartyImpression(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "thirdPartyImpression", JsonConvert.SerializeObject(creative.third_party_tracking), 3));
                    Ultities.Telegram.pushNotify("thirdPartyImpression " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("thirdPartyImpression [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addCodeScript(int index = 0)
        {
            if (creative.creative_template != "Code Tracking") return;

            try
            {
                string script = readfile().Result;
                if (script == string.Empty) return;
                script = script.Replace("\r\n", "");
                string x_path_input_code = "//*[contains(@ngcontrol,'thirdPartySnippet')]//*[contains(@aria-label,'Standard Press the Enter key to add or edit text')]//*[contains(@class,'CodeMirror-wrap')]//*[contains(local-name(),'textarea')]";
                Thread.Sleep(500);


                // check xpath
                if (Common.Common.checkXpathExist(x_path_input_code, browers))
                {
                    WebDriverWait wait = new WebDriverWait(browers, TimeSpan.FromSeconds(10));
                    IWebElement element = wait.Until(
                        SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(x_path_input_code))
                    );
                }
                else
                {
                    return;
                }

                var elem = browers.FindElement(By.XPath(x_path_input_code));
                Common.Common.RemoveCss(elem, browers);

                Common.Common.RemoveCss(elem.FindElement(By.XPath("..")), browers);

                Common.Common.MoveToXpath(elem, browers);

                elem.Clear();
                elem.SendKeys(script);
                Thread.Sleep(100);

                Common.Common.DisplayNoneElem(elem, browers);
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addCodeScript(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "addCodeScript", creative.iframe_url, 3));
                    Ultities.Telegram.pushNotify("addCodeScript " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addCodeScript [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public async Task<string> readfile(int index = 0)
        {
            string result = string.Empty;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(creative.iframe_url);
                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    return readfile(1).Result;
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "readfile", creative.iframe_url, 3));
                    Ultities.Telegram.pushNotify("readfile " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("readfile [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
            return result;
        }

        public void addSafeFrame(int index = 0)
        {
            try
            {
                if (creative.safe_frame == 2)
                {
                    string x_path = "//*[contains(local-name(),'drx-form-checkbox-field')][contains(@debugid,'isSafeFrameCompatible')]//*[contains(local-name(),'material-checkbox')][contains(@aria-checked,'true')]";
                    if (Common.Common.checkXpathExist(x_path, browers))
                    {
                        browers.FindElement(By.XPath(x_path)).Click();
                    }
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addSafeFrame(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "addSafeFrame", creative.safe_frame.ToString(), 3));
                    Ultities.Telegram.pushNotify("addSafeFrame " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addSafeFrame [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void saveCreative(int index = 0)
        {
            string link_creative = string.Empty;
            const string SUCCESS_INDICATOR = "creative_id";
            const string X_PATH_SAVE_BUTTON = "//*[contains(@primarybuttontext,'Save and preview')]//*[contains(local-name(),'material-button')][contains(@class,'btn-yes')]";

            try
            {
                // Click nút Save
                browers.FindElement(By.XPath(X_PATH_SAVE_BUTTON)).Click();
                Thread.Sleep(1500);

                // Lấy URL sau khi save
                link_creative = browers.Url;

                // Kiểm tra xem save có thành công không
                if (link_creative.Contains(SUCCESS_INDICATOR))
                {
                    Console.WriteLine("CREATE CREATIVE SUCCESS: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": " + creative.campaign_name);
                    return;
                }

                // Nếu chưa thành công, thử lại một lần nữa
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    saveCreative(1);
                }
                else
                {
                    // Đã retry nhưng vẫn không thành công
                    string url_error = Common.Common.TakeScreen(browers, request_id, product_id);
                    lstError.Add(new ErrorModel("Created Creative", "saveCreative", link_creative, 3));
                    Ultities.Telegram.pushNotify("saveCreative: Không tìm thấy creative_id trong URL sau khi save. URL: " + link_creative + " #$# " + HttpUtility.UrlEncode(url_error), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("saveCreative [{0}]: Không tìm thấy creative_id trong URL. URL: {1}", index.ToString(), link_creative));
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    saveCreative(1);
                }
                else
                {
                    string url_error = Common.Common.TakeScreen(browers, request_id, product_id);
                    lstError.Add(new ErrorModel("Created Creative", "saveCreative", link_creative, 3));
                    Ultities.Telegram.pushNotify("saveCreative " + ex.ToString() + " #$# " + HttpUtility.UrlEncode(url_error), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("saveCreative [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public int saveDatabase()
        {
            try
            {
                string link_order = browers.Url;
                Console.WriteLine("Link Creative: " + link_order);
                if (link_order.Contains("creative_id"))
                {
                    int parent_id = line_item_id;
                    int type = (Int32)dfp_setup_type.creative;
                    int iResult = Repository.updateStatusFilter(parent_id, link_order, type, product_id, request_id, creative.name, -1, creative.data_creative_id);
                    Console.WriteLine("CREATED CREATIVE SUSSECC: " + creative.name);
                    Ultities.Telegram.pushNotify("CREATED CREATIVE SUSSECC: " + creative.name + " Link Creative: " + HttpUtility.UrlEncode(link_order), tele_group_id, tele_token);
                    return iResult;
                }
                else
                {
                    Console.WriteLine("saveDatabase - saveDatabase = khong luu duoc database");
                    Ultities.Telegram.pushNotify("KHÔNG TẠO ĐƯỢC CREATIVE: " + creative.name, tele_group_id, tele_token);
                    return -1;
                }
            }
            catch (Exception ex)
            {
                lstError.Add(new ErrorModel("Created Creative", "saveDatabase", "Có lỗi trong quá trình cập nhật database", 3));
                Ultities.Telegram.pushNotify("saveDatabase " + ex.ToString(), tele_group_id, tele_token);
                Console.WriteLine("saveDatabase - saveDatabase = " + ex.ToString());
                return -1;
            }
        }

        #endregion END CREATIVE

        #region Submit Form CPM Creative Master Companion        

        public void creativeNameMC(int index = 0)
        {
            try
            {
                string x_path_creative_name_mc = "//settings-tab[@iscompanioncreative]//drx-form-section[@label='Settings']//drx-form-field[@label='Name']//input[@aria-label='Name']";//   "(//material-input[@debugid='creative-name'])[1]//input";
                browers.FindElement(By.XPath(x_path_creative_name_mc)).Clear();
                browers.FindElement(By.XPath(x_path_creative_name_mc)).SendKeys(creative.name + (is_production == "1" ? "" : "_BOT_" + DateTime.Now.ToString()));
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    creativeNameMC(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "creativeNameMC", creative.name, 3));
                    Ultities.Telegram.pushNotify("creativeNameMC " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("creativeNameMC [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void clickThroughUrlMC(int index = 0)
        {
            try
            {
                //string x_path_label_iframe_url = "//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//*[contains(@class,'label-container')]//span|//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//*[contains(@class,'input-container')]//span";
                //var elem = browers.FindElements(By.XPath(x_path_label_iframe_url));
                //string elem_text = string.Empty;
                //if (elem.Count > 0)
                //{
                //    foreach (var obj in elem)
                //    {
                //        if (obj.Text == "Click-through URL")
                //        {
                //            obj.FindElement(By.XPath("../../../..//input")).Clear();
                //            obj.FindElement(By.XPath("../../../..//input")).SendKeys(creative.click_through_url);
                //            return;
                //        }
                //    }
                //}

                string x_path_element_click_through = "//settings-tab[@iscompanioncreative]//drx-form-section[@label='User-defined variables']//input[@aria-label='Click-through URL']";
                var dropdownElement = browers.FindElement(By.XPath(x_path_element_click_through));
                if (dropdownElement != null)
                {
                    var element_click_through = browers.FindElement(By.XPath(x_path_element_click_through));

                    if (element_click_through != null)
                    {
                        browers.FindElement(By.XPath(x_path_element_click_through)).Clear();
                        browers.FindElement(By.XPath(x_path_element_click_through)).SendKeys(creative.click_through_url);
                    }
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    clickThroughUrlMC(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "clickThroughUrlMC", creative.click_through_url, 3));
                    Ultities.Telegram.pushNotify("clickThroughUrlMC " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("clickThroughUrlMC [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void iframeUrlMC(int index = 0)
        {
            try
            {
                if (creative.creative_template == "Iframer Tracking")
                {
                    //string x_path_label_iframe_url = "//*[contains(local-name(),'material-drawer')]//*[contains(@class,'drawer-content')]//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//label//span";
                    //var elem = browers.FindElements(By.XPath(x_path_label_iframe_url));
                    //string elem_text = string.Empty;
                    //if (elem.Count > 0)
                    //{
                    //    foreach (var obj in elem)
                    //    {
                    //        if (obj.Text == "iFrame URL" || obj.Text == "URL of the HTML5 ad" || obj.Text == "iframe_url" || obj.Text == "iFrameURL")
                    //        {
                    //            obj.FindElement(By.XPath("../../../..//input")).Clear();
                    //            obj.FindElement(By.XPath("../../../..//input")).SendKeys(creative.iframe_url);
                    //            return;
                    //        }
                    //    }
                    //}

                    string x_path_label_iframe_url = "//settings-tab[@iscompanioncreative]//drx-form-section[@label='User-defined variables']//input[@aria-label='URL of the HTML5 ad']";
                    var elem = browers.FindElements(By.XPath(x_path_label_iframe_url));
                    if (elem != null)
                    {
                        browers.FindElement(By.XPath(x_path_label_iframe_url)).Clear();
                        browers.FindElement(By.XPath(x_path_label_iframe_url)).SendKeys(creative.iframe_url);
                    }
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    iframeUrl(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "clickThroughUrlMC", creative.iframe_url, 3));
                    Ultities.Telegram.pushNotify("iframeUrl " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("iframeUrl [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void indexIndustrialMC(int index = 0)
        {
            try
            {
                if (creative.index_industrial == string.Empty) return;

                string x_path_label_input_industrial = "//*[contains(local-name(),'material-drawer')]//*[contains(@class,'drawer-content')]//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//label//span";
                var elem = browers.FindElements(By.XPath(x_path_label_input_industrial));
                if (elem.Count > 0)
                {
                    foreach (var obj in elem)
                    {
                        if (obj.Text == "Index_Industrial")
                        {
                            Common.Common.MoveToXpath(obj, browers);
                            obj.FindElement(By.XPath("../../../..//input")).Clear();
                            obj.FindElement(By.XPath("../../../..//input")).SendKeys(creative.index_industrial);
                            Thread.Sleep(100);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    indexIndustrialMC(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "indexIndustrialMC", creative.index_industrial, 3));
                    Ultities.Telegram.pushNotify("indexIndustrialMC " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("indexIndustrialMC [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void indexBrandMC(int index = 0)
        {
            try
            {
                if (creative.index_brand == string.Empty) return;

                string x_path_label_input_brand = "//*[contains(local-name(),'material-drawer')]//*[contains(@class,'drawer-content')]//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//label//span";
                var elem = browers.FindElements(By.XPath(x_path_label_input_brand));
                if (elem.Count > 0)
                {
                    foreach (var obj in elem)
                    {
                        if (obj.Text == "Index_Brand")
                        {
                            Common.Common.MoveToXpath(obj, browers);
                            obj.FindElement(By.XPath("../../../..//input")).Clear();
                            obj.FindElement(By.XPath("../../../..//input")).SendKeys(creative.index_brand);
                            Thread.Sleep(100);
                            return;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    indexBrandMC(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "indexBrandMC", creative.index_brand, 3));
                    Ultities.Telegram.pushNotify("indexBrandMC " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("indexBrandMC [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void campaignNameMC(int index = 0)
        {
            try
            {
                if (creative.campaign_name == string.Empty) return;

                string x_path_label_input_brand = "//*[contains(local-name(),'material-drawer')]//*[contains(@class,'drawer-content')]//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//label//span";
                var elem = browers.FindElements(By.XPath(x_path_label_input_brand));
                if (elem.Count > 0)
                {
                    foreach (var obj in elem)
                    {
                        if (obj.Text == "Campaign_Name")
                        {
                            Common.Common.MoveToXpath(obj, browers);
                            obj.FindElement(By.XPath("../../../..//input")).Clear();
                            obj.FindElement(By.XPath("../../../..//input")).SendKeys(creative.campaign_name);
                            Thread.Sleep(100);
                            return;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    campaignNameMC(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "campaignNameMC", creative.campaign_name, 3));
                    Ultities.Telegram.pushNotify("campaignNameMC " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("campaignNameMC [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void thirdPartyImpressionMC(int index = 0)
        {
            try
            {
                string x_path_label_input_third_party_tracking = "//*[contains(local-name(),'material-drawer')]//*[contains(@class,'drawer-content')]//*[contains(@ngcontrolgroup,'creativeTemplateVariableValues')]//label//span";

                if (creative.third_party_tracking.Count > 0)
                {
                    string thirparty = creative.third_party_tracking[0];
                    if (thirparty != "")
                    {

                        var elem = browers.FindElements(By.XPath(x_path_label_input_third_party_tracking));
                        if (elem.Count > 0)
                        {
                            foreach (var obj in elem)
                            {
                                if (obj.Text.Trim() == "Third-party impression URL" || obj.Text.Trim() == "Thirdpartyimpressiontracker" || obj.Text.Trim() == "Third-party impression tracker" || obj.Text.Trim() == "ThirdpartyimpressionURL")
                                {
                                    Common.Common.MoveToXpath(obj, browers);
                                    obj.FindElement(By.XPath("../../../..//input")).Clear();
                                    obj.FindElement(By.XPath("../../../..//input")).SendKeys(thirparty);
                                    Thread.Sleep(100);
                                    break;
                                }
                            }
                        }
                    }
                }

                if (creative.third_party_tracking.Count > 1)
                {
                    string thirparty2 = creative.third_party_tracking[1];
                    if (thirparty2 != "")
                    {
                        var elem = browers.FindElements(By.XPath(x_path_label_input_third_party_tracking));
                        if (elem.Count > 0)
                        {
                            foreach (var obj in elem)
                            {
                                if (obj.Text.Trim() == "ThirdpartyimpressionURL_2" || obj.Text.Trim() == "Third-party impression tracker2")
                                {
                                    Common.Common.MoveToXpath(obj, browers);
                                    obj.FindElement(By.XPath("../../../..//input")).Clear();
                                    obj.FindElement(By.XPath("../../../..//input")).SendKeys(thirparty2);
                                    Thread.Sleep(100);
                                    break;
                                }
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
                    thirdPartyImpressionMC(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created Creative", "thirdPartyImpressionMC", JsonConvert.SerializeObject(creative.third_party_tracking), 3));
                    Ultities.Telegram.pushNotify("thirdPartyImpressionMC " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("thirdPartyImpressionMC [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void saveCreativeMC(int index = 0)
        {
            try
            {
                string x_path = "//*[contains(@primarybuttontext,'Done')]//*[contains(local-name(),'material-button')][contains(@class,'yes')]";
                var elem = browers.FindElements(By.XPath(x_path));
                if (elem != null && elem.Count > 0)
                {
                    browers.FindElement(By.XPath(x_path)).Click();
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    saveCreativeMC(1);
                }
                else
                {
                    string url_error = Common.Common.TakeScreen(browers, request_id, product_id);
                    lstError.Add(new ErrorModel("Created Creative", "saveCreativeMC", "False", 3));
                    Ultities.Telegram.pushNotify("saveCreativeMC " + ex.ToString() + " #$# " + HttpUtility.UrlEncode(url_error), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("saveCreativeMC [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void saveAndPreview(int index = 0)
        {
            try
            {
                string x_path_save_preview = "//*[contains(@primarybuttontext,'Save and preview')]//*[contains(local-name(),'material-button')][contains(@class,'yes')]";
                browers.FindElement(By.XPath(x_path_save_preview)).Click();
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    saveAndPreview(1);
                }
                else
                {
                    string url_error = Common.Common.TakeScreen(browers, request_id, product_id);
                    lstError.Add(new ErrorModel("Created Creative", "saveAndPreview", "False", 3));
                    Ultities.Telegram.pushNotify("saveAndPreview " + ex.ToString() + " #$# " + HttpUtility.UrlEncode(url_error), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("saveAndPreview [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        #endregion END Submit Form CPM Creative Master Companion
    }
}
