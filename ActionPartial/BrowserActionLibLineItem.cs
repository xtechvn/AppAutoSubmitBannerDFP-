using AppAutoSubmitBannerDFP.Common;
using AppAutoSubmitBannerDFP.Model;
using AppAutoSubmitBannerDFP.ViewModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Newtonsoft.Json;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace AppAutoSubmitBannerDFP.ActionPartial
{

    /// <summary>
    /// Thực thi các module chung trên 1 page
    /// </summary>

    public class BrowserActionLibLineItem
    {
        private static string LogPath = AppDomain.CurrentDomain.BaseDirectory;
        private static string tele_group_id = ConfigurationManager.AppSettings["tele_group_id"];
        private static string tele_token = ConfigurationManager.AppSettings["tele_token"];
        private static string is_production = ConfigurationManager.AppSettings["is_production"].ToString();
        private ChromeDriver browers;
        private LineItemViewModel line_item;
        private int product_id;
        private int request_id;
        private int order_id;
        private List<ErrorModel> lstError;
        WebDriverWait wait = null;

        public BrowserActionLibLineItem(ChromeDriver _browers, LineItemViewModel _line_item, int _product_id, int _request_id, int _order_id, List<ErrorModel> _lstError)
        {
            browers = _browers;
            line_item = _line_item;
            product_id = _product_id;
            request_id = _request_id;
            order_id = _order_id;
            lstError = _lstError;
            wait = new WebDriverWait(_browers, TimeSpan.FromSeconds(10))
            {
                PollingInterval = TimeSpan.FromMilliseconds(500)
            };
        }

        #region Helper Methods với Explicit Waits

        /// <summary>
        /// Tạo WebDriverWait với cấu hình tối ưu
        /// </summary>
        private WebDriverWait GetWait(int timeoutSeconds = 15)
        {
            return new WebDriverWait(browers, TimeSpan.FromSeconds(timeoutSeconds))
            {
                PollingInterval = TimeSpan.FromMilliseconds(500)
            };
        }

        /// <summary>
        /// Tìm element với explicit wait và retry
        /// </summary>
        private IWebElement WaitForElement(By locator, int timeoutSeconds = 10, bool clickable = false)
        {
            try
            {
                WebDriverWait wait = GetWait(timeoutSeconds);
                wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));

                if (clickable)
                {
                    return wait.Until(ExpectedConditions.ElementToBeClickable(locator));
                }
                else
                {
                    return wait.Until(ExpectedConditions.ElementIsVisible(locator));
                }
            }
            catch (WebDriverTimeoutException)
            {
                return null;
            }
        }

        /// <summary>
        /// Check element exists với polling
        /// </summary>
        private bool WaitForElementExists(By locator, int timeoutSeconds = 5)
        {
            try
            {
                WebDriverWait wait = GetWait(timeoutSeconds);
                wait.Until(ExpectedConditions.ElementExists(locator));
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        /// Click element với retry và explicit wait
        /// </summary>
        private bool SafeClick(By locator, int maxRetries = 3, int timeoutSeconds = 10)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    closeLightBox();
                    var element = WaitForElement(locator, timeoutSeconds, clickable: true);
                    if (element != null)
                    {
                        Common.Common.MoveToXpath(element, browers);
                        element.Click();
                        return true;
                    }
                }
                catch (StaleElementReferenceException)
                {
                    if (i == maxRetries - 1) throw;
                    Thread.Sleep(500);
                    continue;
                }
                catch (ElementClickInterceptedException)
                {
                    if (i == maxRetries - 1) throw;
                    Thread.Sleep(500);
                    continue;
                }
            }
            return false;
        }

        /// <summary>
        /// SendKeys với retry và explicit wait
        /// </summary>
        private bool SafeSendKeys(By locator, string text, bool clearFirst = true, int maxRetries = 3, int timeoutSeconds = 20)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    closeLightBox();
                    var element = WaitForElement(locator, timeoutSeconds);
                    if (element != null)
                    {
                        if (clearFirst)
                        {
                            element.Clear();
                        }
                        element.SendKeys(text);
                        return true;
                    }
                }
                catch (StaleElementReferenceException)
                {
                    if (i == maxRetries - 1) throw;
                    Thread.Sleep(500);
                    continue;
                }
            }
            return false;
        }

        /// <summary>
        /// Wait cho dropdown list xuất hiện
        /// </summary>
        private bool WaitForDropdownVisible(int timeoutSeconds = 10)
        {
            try
            {
                WebDriverWait wait = GetWait(timeoutSeconds);
                string xpathDropdown = "//*[contains(@class,'material-dropdown-select-popup')][contains(@class,'visible')]";
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(xpathDropdown)));
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Click dropdown và chọn item
        /// </summary>
        private bool SelectDropdownItem(By dropdownButton, int itemIndex, int timeoutSeconds = 10)
        {
            try
            {
                closeLightBox();

                // Click dropdown button
                var button = WaitForElement(dropdownButton, timeoutSeconds, clickable: true);
                if (button == null)
                {
                    Console.WriteLine($"SelectDropdownItem: Cannot find dropdown button with locator: {dropdownButton}");
                    return false;
                }

                Common.Common.MoveToXpath(button, browers);
                button.Click();

                // Wait for dropdown to appear
                if (!WaitForDropdownVisible(timeoutSeconds))
                {
                    Console.WriteLine($"SelectDropdownItem: Dropdown did not appear after clicking button");
                    return false;
                }

                Thread.Sleep(300); // Small delay for animation

                // Select item
                string xpathItem = $"//*[contains(@class,'material-dropdown-select-popup')][contains(@class,'visible')]//*[contains(local-name(),'material-select-dropdown-item')][{itemIndex}]";
                var item = WaitForElement(By.XPath(xpathItem), timeoutSeconds, clickable: true);
                if (item == null)
                {
                    Console.WriteLine($"SelectDropdownItem: Cannot find dropdown item at index {itemIndex}");
                    return false;
                }

                item.Click();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SelectDropdownItem error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Retry wrapper cho các operations
        /// </summary>
        private T RetryOperation<T>(Func<T> operation, int maxRetries = 3, int delayMs = 1000, string operationName = "")
        {
            Exception lastException = null;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return operation();
                }
                catch (StaleElementReferenceException ex)
                {
                    lastException = ex;
                    if (i < maxRetries - 1)
                    {
                        Thread.Sleep(delayMs);
                        continue;
                    }
                }
                catch (NoSuchElementException ex)
                {
                    lastException = ex;
                    if (i < maxRetries - 1)
                    {
                        Thread.Sleep(delayMs);
                        continue;
                    }
                }
                catch (ElementNotInteractableException ex)
                {
                    lastException = ex;
                    if (i < maxRetries - 1)
                    {
                        Thread.Sleep(delayMs);
                        continue;
                    }
                }
                catch (WebDriverTimeoutException ex)
                {
                    lastException = ex;
                    if (i < maxRetries - 1)
                    {
                        Thread.Sleep(delayMs);
                        continue;
                    }
                }
            }

            if (!string.IsNullOrEmpty(operationName))
            {
                lstError.Add(new ErrorModel("Created LineItem", operationName, "Retry failed", 2));
            }

            throw lastException ?? new Exception("Operation failed after retries");
        }

        /// <summary>
        /// Retry wrapper cho void operations
        /// </summary>
        private void RetryOperation(Action operation, int maxRetries = 3, int delayMs = 1000, string operationName = "")
        {
            RetryOperation(() =>
            {
                operation();
                return true;
            }, maxRetries, delayMs, operationName);
        }

        #endregion

        public void closeLightBox(int index = 0)
        {
            try
            {
                string x_path_popup_note = "//*[contains(local-name(),'global-search')]//*[contains(local-name(),'input')][contains(@aria-label,'Search')]";

                // Sử dụng explicit wait thay vì checkXpathExist
                var element = WaitForElement(By.XPath(x_path_popup_note), timeoutSeconds: 3, clickable: true);
                if (element != null)
                {
                    element.Click();
                }
            }
            catch (Exception ex)
            {
                // Chỉ log nếu không phải timeout (timeout là bình thường nếu không có popup)
                if (!(ex is WebDriverTimeoutException))
                {
                    if (index == 0)
                    {
                        Thread.Sleep(1000);
                        closeLightBox(1);
                    }
                    else
                    {
                        Ultities.Telegram.pushNotify("CloseLightBox " + ex.ToString(), tele_group_id, tele_token);
                        Console.WriteLine(string.Format("CloseLightBox [{0}] = {1}", index.ToString(), ex.ToString()));
                    }
                }
            }
        }

        public void CloseNote(int index = 0)
        {
            try
            {
                string x_path_popup_note = "//*[contains(local-name(),'release-notes')]//*[contains(@class,'release-notes-popup-container')][contains(@class,'hidden')]";
                string x_path_close_note = "//*[contains(local-name(),'release-notes')]//*[contains(local-name(),'material-button')][contains(@class,'close')]";

                // Kiểm tra popup có hidden không (nếu hidden thì không cần đóng)
                bool isHidden = WaitForElementExists(By.XPath(x_path_popup_note), timeoutSeconds: 2);
                if (!isHidden)
                {
                    // Popup đang hiển thị, cần đóng
                    var closeButton = WaitForElement(By.XPath(x_path_close_note), timeoutSeconds: 3, clickable: true);
                    if (closeButton != null)
                    {
                        closeButton.Click();
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
                string x_path_name = "//*[contains(@debugid,'name-field')]//*[contains(local-name(),'textarea')]";
                wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(x_path_name)));
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

        public void addTargetingName(int index = 0)
        {
            try
            {
                RetryOperation(() =>
                {
                    closeLightBox();
                    string x_path_name = "//*[contains(@debugid,'name-field')]//*[contains(local-name(),'textarea')]";

                    var element = WaitForElement(By.XPath(x_path_name), timeoutSeconds: 15);
                    if (element == null)
                    {
                        throw new Exception("Cannot find name field element");
                    }

                    element.Clear();
                    string nameValue = line_item.name + (is_production == "1" ? "" : "_BOT_" + DateTime.Now.ToString());
                    element.SendKeys(nameValue);
                }, maxRetries: 3, operationName: "addTargetingName");
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addTargetingName(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "addTargetingName", line_item.name, 2));
                    Ultities.Telegram.pushNotify(string.Format("addTargetingName [{0}] = {1}", index.ToString(), ex.ToString()), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addTargetingName [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void creativeSize(int index = 0)
        {
            try
            {
                if (line_item.ad_type == (Int16)AdsType.master_companion)
                {
                    if (!string.IsNullOrEmpty(line_item.add_sizes))
                    {
                        RetryOperation(() =>
                        {
                            closeLightBox();
                            string x_path = "//*[contains(@aria-label,'Enter a value or select one from the list')]";

                            // Sử dụng SafeSendKeys thay vì FindElement + SendKeys
                            if (!SafeSendKeys(By.XPath(x_path), line_item.add_sizes, clearFirst: true, timeoutSeconds: 15))
                            {
                                throw new Exception("Cannot send keys to creative size input");
                            }

                            // Wait for dropdown to appear thay vì Thread.Sleep
                            string x_path_list_box = "//*[contains(@class,'pane selections')][contains(@class,'visible')]//*[contains(@class,'list-group')]/material-select-dropdown-item[1]";
                            var dropdownItem = WaitForElement(By.XPath(x_path_list_box), timeoutSeconds: 5, clickable: true);
                            if (dropdownItem != null)
                            {
                                dropdownItem.Click();
                            }
                            else
                            {
                                throw new Exception("Dropdown item not found");
                            }
                        }, maxRetries: 3, operationName: "creativeSize");
                    }
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    creativeSize(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "creativeSize", line_item.add_sizes, 2));
                    Ultities.Telegram.pushNotify(string.Format("creativeSize [{0}] = {1}", index.ToString(), ex.ToString()), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("creativeSize [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void companionSizes(int index = 0)
        {
            try
            {
                if (line_item.ad_type == (Int16)AdsType.master_companion)
                {
                    if (line_item.companion_sizes != null)
                    {
                        RetryOperation(() =>
                        {
                            closeLightBox();
                            foreach (var item in line_item.companion_sizes)
                            {
                                if (item.IndexOf(line_item.add_sizes) == -1 && line_item.category_name.IndexOf("ĐB") != -1)
                                {
                                    string x_path = "//*[contains(@aria-label,'Companion sizes')]";
                                    string companionSizes = line_item.category_name.IndexOf("ĐB") != -1 ? "300x600" : item;

                                    // Sử dụng SafeSendKeys
                                    if (!SafeSendKeys(By.XPath(x_path), companionSizes, clearFirst: false, timeoutSeconds: 15))
                                    {
                                        throw new Exception($"Cannot send keys for companion size: {companionSizes}");
                                    }

                                    // Wait for dropdown thay vì Thread.Sleep
                                    string x_path_list_box = "//*[contains(@class,'pane selections')][contains(@class,'visible')]//*[contains(@class,'list-group')]/material-select-dropdown-item[1]";
                                    var dropdownItem = WaitForElement(By.XPath(x_path_list_box), timeoutSeconds: 5, clickable: true);
                                    if (dropdownItem != null)
                                    {
                                        dropdownItem.Click();
                                    }
                                    else
                                    {
                                        throw new Exception("Companion size dropdown item not found");
                                    }
                                }
                            }
                        }, maxRetries: 3, operationName: "companionSizes");
                    }
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    companionSizes(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "companionSizes", JsonConvert.SerializeObject(line_item.companion_sizes), 2));
                    Ultities.Telegram.pushNotify(string.Format("companionSizes [{0}] = {1}", index.ToString(), ex.ToString()), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("companionSizes [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addLineItem(int index = 0)
        {
            try
            {
                if (line_item.item_type == 2) return;

                RetryOperation(() =>
                {
                    closeLightBox();
                    string x_path_line_item_type = "//*[contains(@ngcontrol,'lineItemType')]";

                    // Sử dụng SelectDropdownItem helper
                    if (!SelectDropdownItem(By.XPath(x_path_line_item_type), line_item.item_type, timeoutSeconds: 15))
                    {
                        throw new Exception($"Cannot select line item type: {line_item.item_type}");
                    }
                }, maxRetries: 3, operationName: "addLineItem");
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addLineItem(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "addLineItem", line_item.item_type.ToString(), 2));
                    Ultities.Telegram.pushNotify("addLineItem " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addLineItem [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addLineItemPriority(int index = 0)
        {
            try
            {
                switch (line_item.line_item_type)
                {
                    case (Int16)line_item_type.DFP_CPM:
                        if (line_item.item_type == 2)
                        {
                            RetryOperation(() =>
                            {
                                closeLightBox();
                                string x_path = "//*[contains(@ngcontrol,'standardTypePriority')]/dropdown-button";

                                // Sử dụng SelectDropdownItem helper
                                if (!SelectDropdownItem(By.XPath(x_path), 1, timeoutSeconds: 15))
                                {
                                    throw new Exception("Cannot select line item priority");
                                }
                            }, maxRetries: 3, operationName: "addLineItemPriority");
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addLineItemPriority(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "addLineItemPriority", "DFP-CPM", 2));
                    Ultities.Telegram.pushNotify("addLineItemPriority " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addLineItemPriority [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        /// <summary>
        /// Thêm expected creatives cho line item
        /// </summary>
        public void addExpectedCreatives(int index = 0)
        {
            try
            {
                // Chỉ xử lý nếu không phải master_companion và có add_sizes
                if (line_item.ad_type == (Int16)AdsType.master_companion || string.IsNullOrEmpty(line_item.add_sizes))
                {
                    return;
                }

                RetryOperation(() =>
                {
                    closeLightBox();

                    // Định nghĩa các XPath locators
                    By locatorAddSize = By.XPath("//*[contains(@class,'creative-sizes')]//*[contains(@placeholder,'Add sizes or native formats')]");
                    By locatorCheckFirst = By.XPath("//*[contains(@id,'acx-overlay-container-TRAFFICKING')]//*[contains(@pane-id,'TRAFFICKING-')][contains(@class,'visible')]//*[contains(local-name(),'material-select-item')][1]//*[contains(local-name(),'material-checkbox')]");
                    By locatorButtonClear = By.XPath("//*[contains(@ngcontrolgroup,'creativePlaceholders')]//*[contains(local-name(),'material-button-dropdown')][contains(@buttonarialabel,'Open more actions')]");
                    By locatorSelectClear = By.XPath("//*[contains(@id,'acx-overlay-container-TRAFFICKING')]//*[contains(@pane-id,'TRAFFICKING-')][contains(@class,'visible')]//*[contains(local-name(),'material-list-item')][1]");

                    // Clear existing selections
                    if (!SafeClick(locatorButtonClear, timeoutSeconds: 10))
                    {
                        throw new Exception("Cannot find or click clear button");
                    }

                    // Wait for dropdown to appear and select clear option
                    var clearOption = WaitForElement(locatorSelectClear, timeoutSeconds: 5, clickable: true);
                    if (clearOption == null)
                    {
                        throw new Exception("Cannot find clear option in dropdown");
                    }
                    clearOption.Click();

                    // Add main size
                    if (!AddCreativeSize(locatorAddSize, locatorCheckFirst, line_item.add_sizes))
                    {
                        throw new Exception($"Cannot add main size: {line_item.add_sizes}");
                    }

                    // Add companion sizes nếu là DFP_CPM_TOI_UU
                    if (line_item.line_item_type == (Int16)line_item_type.DFP_CPM_TOI_UU && line_item.companion_sizes != null)
                    {
                        foreach (string adSize in line_item.companion_sizes)
                        {
                            // Chỉ thêm nếu size chưa được thêm (kiểm tra cả 2 chiều)
                            if (line_item.add_sizes.IndexOf(adSize, StringComparison.OrdinalIgnoreCase) == -1 &&
                                adSize.IndexOf(line_item.add_sizes, StringComparison.OrdinalIgnoreCase) == -1)
                            {
                                if (!AddCreativeSize(locatorAddSize, locatorCheckFirst, adSize))
                                {
                                    Console.WriteLine($"Warning: Cannot add companion size: {adSize}");
                                }
                            }
                        }
                    }

                    // Add creative size nếu có category_name và creative

                    //if (line_item.category_name != null && line_item.creative != null && line_item.creative.Count > 0 && !string.IsNullOrEmpty(line_item.creative[0].ads_size))
                    //{
                    //    string creativeSize = line_item.creative[0].ads_size;

                    //    // Kiểm tra xem size đã được chọn trong UI chưa
                    //    if (!IsCreativeSizeAlreadyAdded(creativeSize))
                    //    {
                    //        if (!AddCreativeSize(locatorAddSize, locatorCheckFirst, creativeSize))
                    //        {
                    //            Console.WriteLine($"Warning: Cannot add creative size: {creativeSize}");
                    //        }
                    //    }
                    //    else
                    //    {
                    //        Console.WriteLine($"Info: Creative size '{creativeSize}' already added (found in chips), skipping");
                    //    }
                    //}

                }, maxRetries: 3, operationName: "addExpectedCreatives");
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addExpectedCreatives(1);
                }
                else
                {
                    string errorData = line_item.add_sizes + "|" + JsonConvert.SerializeObject(line_item.companion_sizes);
                    lstError.Add(new ErrorModel("Created LineItem", "addExpectedCreatives", errorData, 2));
                    Ultities.Telegram.pushNotify("addExpectedCreatives " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addExpectedCreatives [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        /// <summary>
        /// Helper method để thêm một creative size
        /// </summary>
        private bool AddCreativeSize(By locatorAddSize, By locatorCheckFirst, string size)
        {
            try
            {
                // Bước 1: Tìm và click vào input để mở dropdown
                var inputElement = WaitForElement(locatorAddSize, timeoutSeconds: 10, clickable: true);
                if (inputElement == null)
                {
                    Console.WriteLine($"AddCreativeSize: Cannot find input element for size '{size}'");
                    return false;
                }

                // Click vào input để focus và mở dropdown
                inputElement.Click();
                Thread.Sleep(300); // Đợi dropdown mở

                // Bước 2: Đợi cho overlay container và dropdown xuất hiện
                string xpathOverlayVisible = "//*[contains(@id,'acx-overlay-container-TRAFFICKING')]//*[contains(@pane-id,'TRAFFICKING-')][contains(@class,'visible')]";
                try
                {
                    WebDriverWait wait = GetWait(10);
                    wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(xpathOverlayVisible)));
                }
                catch (WebDriverTimeoutException)
                {
                    Console.WriteLine($"AddCreativeSize: Timeout waiting for dropdown overlay to appear for size '{size}'");
                    // Thử click lại một lần nữa
                    inputElement.Click();
                    Thread.Sleep(500);
                    try
                    {
                        WebDriverWait waitRetry = GetWait(5);
                        waitRetry.Until(ExpectedConditions.ElementIsVisible(By.XPath(xpathOverlayVisible)));
                    }
                    catch
                    {
                        return false;
                    }
                }

                // Bước 3: Clear và send keys vào input
                inputElement.Clear();
                Thread.Sleep(200);
                inputElement.SendKeys(size);
                Thread.Sleep(500); // Đợi suggestion list được filter/render

                // Bước 4: Đợi cho suggestion list items xuất hiện (checkbox)
                var checkbox = WaitForElement(locatorCheckFirst, timeoutSeconds: 10, clickable: true);
                if (checkbox == null)
                {
                    Console.WriteLine($"AddCreativeSize: Cannot find checkbox for size '{size}' after sending keys");
                    return false;
                }

                // Kiểm tra xem checkbox có visible và enabled không
                if (!checkbox.Displayed || !checkbox.Enabled)
                {
                    Console.WriteLine($"AddCreativeSize: Checkbox for size '{size}' is not displayed or enabled");
                    return false;
                }

                // Bước 5: Click vào checkbox
                checkbox.Click();

                // Đợi một chút để đảm bảo click được xử lý và dropdown đóng
                Thread.Sleep(400);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddCreativeSize error for size '{size}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra xem creative size đã được chọn trong UI chưa (dựa trên các chip đã hiển thị)
        /// </summary>
        private bool IsCreativeSizeAlreadyAdded(string size)
        {
            try
            {
                // XPath để tìm tất cả các chip đã được chọn
                // Từ DOM: //material-chips//drx-chip//material-chip//div[contains(@class, 'content')]
                By locatorChips = By.XPath("//material-chips//drx-chip//material-chip//div[contains(@class, 'content')]");

                var chipElements = browers.FindElements(locatorChips);

                foreach (var chipElement in chipElements)
                {
                    string chipText = chipElement.Text?.Trim() ?? "";

                    // So sánh không phân biệt hoa thường và kiểm tra cả 2 chiều
                    if (!string.IsNullOrWhiteSpace(chipText) &&
                        (chipText.IndexOf(size, StringComparison.OrdinalIgnoreCase) != -1 ||
                         size.IndexOf(chipText, StringComparison.OrdinalIgnoreCase) != -1))
                    {
                        return true; // Đã tìm thấy size trong chip
                    }
                }

                return false; // Không tìm thấy
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IsCreativeSizeAlreadyAdded error for size '{size}': {ex.Message}");
                // Nếu có lỗi, return false để tiếp tục thử thêm
                return false;
            }
        }

        public void addStartTime(int index = 0)
        {
            try
            {
                RetryOperation(() =>
                {
                    closeLightBox();
                    string x_path = "//*[contains(@debugid,'start-date-picker')]//*[contains(@aria-label,'Date')]";
                    string x_path_now = "//*[contains(@id,'acx-overlay-container-TRAFFICKING')]//*[contains(@pane-id,'TRAFFICKING-')][contains(@class,'visible')]//*[contains(local-name(),'material-list-item')][1]";

                    var _start_time = Convert.ToDateTime(line_item.start_time);

                    // Sử dụng SafeSendKeys
                    if (!SafeSendKeys(By.XPath(x_path), _start_time.ToString("MM/dd/yyyy"), clearFirst: true, timeoutSeconds: 15))
                    {
                        throw new Exception("Cannot set start time");
                    }

                    if (_start_time < DateTime.Now)
                    {
                        // Wait for "Now" button to be clickable
                        var nowButton = WaitForElement(By.XPath(x_path_now), timeoutSeconds: 5, clickable: true);
                        if (nowButton != null)
                        {
                            nowButton.Click();
                        }
                    }
                }, maxRetries: 3, operationName: "addStartTime");
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addStartTime(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "addStartTime", line_item.start_time, 2));
                    Ultities.Telegram.pushNotify("addStartTime " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addStartTime [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addEndTime(int index = 0)
        {
            try
            {
                RetryOperation(() =>
                {
                    closeLightBox();
                    string x_path = "//*[contains(@debugid,'end-date-picker')]//*[contains(@aria-label,'Date')]";

                    var _end_time = Convert.ToDateTime(line_item.end_time);
                    if (_end_time > DateTime.Now)
                    {
                        // Sử dụng SafeSendKeys
                        if (!SafeSendKeys(By.XPath(x_path), _end_time.ToString("MM/dd/yyyy"), clearFirst: true, timeoutSeconds: 15))
                        {
                            throw new Exception("Cannot set end time");
                        }
                    }
                    else
                    {
                        lstError.Add(new ErrorModel("Created LineItem", "khong duoc set qua khu->addEndTime", line_item.end_time, 2));
                    }
                }, maxRetries: 3, operationName: "addEndTime");
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addEndTime(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "addEndTime", line_item.end_time, 2));
                    Ultities.Telegram.pushNotify("addEndTime " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addEndTime [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addStartHour(int index = 0)
        {
            try
            {
                RetryOperation(() =>
                {
                    closeLightBox();
                    string x_path_click_out = "//*[contains(@debugid,'end-date-picker')]//*[contains(@aria-label,'Date')]";
                    var _start_date = Convert.ToDateTime(line_item.start_time);
                    if (_start_date < DateTime.Now)
                    {
                        var clickOutElement = WaitForElement(By.XPath(x_path_click_out), timeoutSeconds: 5, clickable: true);
                        if (clickOutElement != null)
                        {
                            clickOutElement.Click();
                        }
                        return;
                    }

                    var _start_time = Convert.ToDateTime(line_item.start_time).ToString("h:mm tt");
                    string x_path = "//*[contains(@debugid,'start-date-picker')]//*[contains(local-name(),'dropdown-button')]";
                    string x_path_input_time = "//*[contains(@class,'time-picker-popup')][contains(@class,'visible')]//*[contains(@class,'input')][contains(@type,'text')]";

                    // Click dropdown button
                    var dropdownButton = WaitForElement(By.XPath(x_path), timeoutSeconds: 10, clickable: true);
                    if (dropdownButton == null) throw new Exception("Cannot find start hour dropdown");
                    dropdownButton.Click();

                    // Wait for time picker popup
                    var timeInput = WaitForElement(By.XPath(x_path_input_time), timeoutSeconds: 5);
                    if (timeInput == null) throw new Exception("Cannot find time input");

                    timeInput.Clear();
                    timeInput.SendKeys(_start_time);
                }, maxRetries: 3, operationName: "addStartHour");
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addStartHour(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "addStartHour", line_item.start_time, 2));
                    Ultities.Telegram.pushNotify("addStartHour " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addStartHour [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addEndHour(int index = 0)
        {
            try
            {
                RetryOperation(() =>
                {
                    closeLightBox();
                    var _end_time = Convert.ToDateTime(line_item.end_time).ToString("h:mm tt");
                    string x_path = "//*[contains(@debugid,'end-date-picker')]//*[contains(local-name(),'dropdown-button')]";
                    string x_path_input_time = "//*[contains(@class,'time-picker-popup')][contains(@class,'visible')]//*[contains(@class,'input')][contains(@type,'text')]";

                    // Click dropdown button
                    var dropdownButton = WaitForElement(By.XPath(x_path), timeoutSeconds: 10, clickable: true);
                    if (dropdownButton == null) throw new Exception("Cannot find end hour dropdown");
                    dropdownButton.Click();

                    // Wait for time picker popup
                    var timeInput = WaitForElement(By.XPath(x_path_input_time), timeoutSeconds: 5);
                    if (timeInput == null) throw new Exception("Cannot find time input");

                    timeInput.Clear();
                    timeInput.SendKeys(_end_time);
                    timeInput.Click(); // Click to close
                }, maxRetries: 3, operationName: "addEndHour");
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addEndHour(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "addEndHour", line_item.end_time, 2));
                    Ultities.Telegram.pushNotify("addEndHour " + ex.ToString(), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addEndHour [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addGoal(int index = 0)
        {
            try
            {
                if (line_item.goal <= 0) return;
                if (line_item.line_item_type == (Int32)line_item_type.DFP_CPD)
                {
                    RetryOperation(() =>
                    {
                        closeLightBox();
                        string x_path = "//*[contains(@ngcontrol,'units')]//*[contains(local-name(),'input')]";

                        if (!SafeSendKeys(By.XPath(x_path), line_item.goal.ToString(), clearFirst: true, timeoutSeconds: 15))
                        {
                            throw new Exception("Cannot set goal");
                        }
                    }, maxRetries: 3, operationName: "addGoal");
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addGoal(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "addGoal", line_item.goal.ToString(), 2));
                    Ultities.Telegram.pushNotify(string.Format("addGoal [{0}]= {1}", index.ToString(), ex.ToString()), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addGoal [{0}]= {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void goalType(int index = 0)
        {
            try
            {
                if (line_item.item_type == (Int16)item_type.DFP_CPM_DEFAULT) return;

                if (line_item.item_type == 5) // Tối ưu hoặc quốc tế
                {
                    RetryOperation(() =>
                    {
                        closeLightBox();
                        string x_path = "//*[contains(@ngcontrol,'goalType')]";

                        // Sử dụng SelectDropdownItem helper
                        if (!SelectDropdownItem(By.XPath(x_path), 2, timeoutSeconds: 15))
                        {
                            throw new Exception("Cannot select goal type");
                        }
                    }, maxRetries: 3, operationName: "goalType");
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    goalType(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "goalType", "DFP-TOI-UU", 2));
                    Ultities.Telegram.pushNotify(string.Format("goalType [{0}]= {1}", index.ToString(), ex.ToString()), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("goalType [{0}]= {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addRate(int index = 0)
        {
            try
            {
                RetryOperation(() =>
                {
                    closeLightBox();
                    string x_path = "//*[contains(@ngcontrol,'costPerUnit')]//*[contains(@aria-label,'Rate')]";

                    string rate = (line_item.rate + line_item.rate * (line_item.vat / 100)).ToString();

                    if (!SafeSendKeys(By.XPath(x_path), rate, clearFirst: true, timeoutSeconds: 15))
                    {
                        throw new Exception("Cannot set rate");
                    }
                }, maxRetries: 3, operationName: "addRate");
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addRate(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "addRate", line_item.rate.ToString(), 2));
                    Ultities.Telegram.pushNotify(string.Format("addRate [{0}]= {1}", index.ToString(), ex.ToString()), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addRate [{0}]= {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        /// <summary>
        /// Thêm currency cho line item
        /// Dropdown structure: Index 1 = VND, Index 2 = USD
        /// usd values: 0 = VND (default, skip), 1 = USD (already set, skip), 2 = USD (need to set at index 2)
        /// </summary>
        public void addCurrency(int index = 0)
        {
            try
            {
                // Early return nếu đã là USD (usd == 1) hoặc VND mặc định (usd == 0)
                if (line_item.usd == 1 || line_item.usd == 0)
                {
                    return;
                }

                // Validate usd value
                if (line_item.usd < 0)
                {
                    Console.WriteLine($"Warning: Invalid usd value: {line_item.usd}, skipping currency selection");
                    return;
                }

                RetryOperation(() =>
                {
                    closeLightBox();

                    // Định nghĩa locator cho currency dropdown
                    By locatorCurrencyDropdown = By.XPath("//*[contains(@ngcontrolgroup,'rate')]//*[contains(local-name(),'currency-picker')]//*[contains(local-name(),'material-dropdown-select')]");

                    // Map usd value to dropdown index
                    // Dropdown: Index 1 = VND, Index 2 = USD
                    // usd == 2 means need to select USD at index 2
                    int dropdownIndex = line_item.usd == 2 ? 2 : line_item.usd;

                    // Sử dụng SelectDropdownItem helper để chọn currency
                    if (!SelectDropdownItem(locatorCurrencyDropdown, dropdownIndex, timeoutSeconds: 15))
                    {
                        throw new Exception($"Cannot select currency at dropdown index: {dropdownIndex} (usd value: {line_item.usd}). Expected: Index 1 = VND, Index 2 = USD");
                    }
                }, maxRetries: 3, operationName: "addCurrency");
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
                    string errorData = line_item.usd.ToString();
                    lstError.Add(new ErrorModel("Created LineItem", "addCurrency", errorData, 2));
                    Ultities.Telegram.pushNotify(string.Format("addCurrency [{0}] = {1}", index.ToString(), ex.ToString()), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addCurrency [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addDiscount(int index = 0)
        {
            if (line_item.item_type == (Int16)item_type.DFP_CPM_DEFAULT) return;
            try
            {
                RetryOperation(() =>
                {
                    closeLightBox();
                    string x_path = "//*[contains(@ngcontrol,'discount')]//*[contains(local-name(),'input')]";

                    if (!SafeSendKeys(By.XPath(x_path), line_item.discount.ToString(), clearFirst: true, timeoutSeconds: 15))
                    {
                        throw new Exception("Cannot set discount");
                    }
                }, maxRetries: 3, operationName: "addDiscount");
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addDiscount(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "addDiscount", line_item.discount.ToString(), 2));
                    Ultities.Telegram.pushNotify(string.Format("addDiscount [{0}]= {1}", index.ToString(), ex.ToString()), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addDiscount [{0}]= {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addQuantity(int index = 0)
        {
            if (line_item.line_item_type == (Int16)line_item_type.DFP_CPD) return;
            if (line_item.item_type == (Int16)item_type.DFP_CPM_DEFAULT) return;

            try
            {
                RetryOperation(() =>
                {
                    closeLightBox();
                    string x_path = "//*[contains(@ngcontrol,'units')]//input";

                    if (!SafeSendKeys(By.XPath(x_path), line_item.quantity.ToString(), clearFirst: true, timeoutSeconds: 15))
                    {
                        throw new Exception("Cannot set quantity");
                    }
                }, maxRetries: 3, operationName: "addQuantity");
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addQuantity(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "addQuantity", line_item.quantity.ToString(), 2));
                    Ultities.Telegram.pushNotify(string.Format("addQuantity [{0}]= {1}", index.ToString(), ex.ToString()), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addQuantity [{0}]= {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addUnitType(int index = 0)
        {
            try
            {
                if (line_item.item_type == (Int16)item_type.DFP_CPM_DEFAULT) return;

                RetryOperation(() =>
                {
                    closeLightBox();
                    string x_path_discount_type = "//*[contains(@ngcontrol,'discountType')]";

                    // Sử dụng SelectDropdownItem helper
                    if (!SelectDropdownItem(By.XPath(x_path_discount_type), line_item.percentage, timeoutSeconds: 15))
                    {
                        throw new Exception($"Cannot select unit type: {line_item.percentage}");
                    }
                }, maxRetries: 3, operationName: "addUnitType");
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addUnitType(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "addUnitType", line_item.percentage.ToString(), 2));
                    Ultities.Telegram.pushNotify(string.Format("addUnitType [{0}] = {1}", index.ToString(), ex.ToString()), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addUnitType [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }
        /// <summary>
        /// Delivery Settings - Event (optional)
        /// </summary>
        /// <param name="index"></param>
        public void addUnit(int index = 0)
        {
            try
            {
                if (line_item.line_item_type == (Int32)line_item_type.DFP_CPM_TOI_UU && line_item.unit == 2)
                {
                    RetryOperation(() =>
                    {
                        closeLightBox();
                        string x_path_discount_type = "//*[contains(@ngcontrol,'unitType')]";

                        // Sử dụng SelectDropdownItem helper
                        if (!SelectDropdownItem(By.XPath(x_path_discount_type), line_item.unit, timeoutSeconds: 15))
                        {
                            throw new Exception($"Cannot select unit: {line_item.unit}");
                        }
                    }, maxRetries: 3, operationName: "addUnit");
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addUnit(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "addUnit", line_item.unit.ToString(), 2));
                    Ultities.Telegram.pushNotify(string.Format("addUnit [{0}] = {1}", index.ToString(), ex.ToString()), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addUnit [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addDisplayCompanions(int index = 0)
        {
            try
            {
                switch (line_item.line_item_type)
                {
                    case (Int16)line_item_type.DFP_CPM:
                    case (Int16)line_item_type.DFP_CPM_TOI_UU:
                        RetryOperation(() =>
                        {
                            closeLightBox();

                            // XPath có OR operator, cần thử từng XPath một
                            string[] xpaths = new string[]
                            {
                                "//*[contains(@ngcontrol,'companionDeliveryOption')]/dropdown-button",
                                "//*[contains(@ngcontrol,'roadblockingType')]/dropdown-button"
                            };

                            bool success = false;
                            foreach (var xpath in xpaths)
                            {
                                if (SelectDropdownItem(By.XPath(xpath), line_item.display_creative, timeoutSeconds: 10))
                                {
                                    success = true;
                                    break;
                                }
                            }

                            if (!success)
                            {
                                throw new Exception($"Cannot select display companion: {line_item.display_creative}");
                            }
                        }, maxRetries: 3, operationName: "addDisplayCompanions");
                        break;
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addDisplayCompanions(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "addDisplayCompanions", line_item.display_creative.ToString(), 2));
                    Ultities.Telegram.pushNotify(string.Format("addDisplayCompanions [{0}] = {1}", index.ToString(), ex.ToString()), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addDisplayCompanions [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addDeliveryTime(int index = 0)
        {
            try
            {
                if (line_item.line_item_type == (Int16)line_item_type.DFP_CPD) return;
                if (line_item.start_deliver_time == "" && line_item.end_deliver_time == "") return;

                Thread.Sleep(500);

                closeLightBox();

                string x_path_check = "//*[contains(local-name(),'daypart-targeting-picker')]//*[contains(local-name(),'material-checkbox')]";
                string x_path_select_start_time = "//*[contains(local-name(),'daypart-targeting-picker')]//*[contains(local-name(),'material-dropdown-select')][contains(@debugid,'start-date-time')]";
                string x_path_select_end_time = "//*[contains(local-name(),'daypart-targeting-picker')]//*[contains(local-name(),'material-dropdown-select')][contains(@debugid,'end-date-time')]";
                string x_path_item_select = "//*[contains(@id,'acx-overlay-container-TRAFFICKING')]//*[contains(@pane-id,'TRAFFICKING-')][contains(@class,'visible')]//*[contains(local-name(),'material-select-dropdown-item')][item_index]";
                string x_path_item_label = "//*[contains(@id,'acx-overlay-container-TRAFFICKING')]//*[contains(@pane-id,'TRAFFICKING-')][contains(@class,'visible')]//*[contains(local-name(),'material-select-dropdown-item')]//*[contains(local-name(),'span')]";
                string x_path_day = "//*[contains(@class,'gm-weekday')]";
                string pattern = "[^0-9:APM]";
                var elem_check = browers.FindElement(By.XPath(x_path_check));
                Common.Common.MoveToXpath(elem_check, browers);
                var elem_value = elem_check.GetAttribute("aria-checked");
                if (elem_value == "false")
                {
                    browers.FindElement(By.XPath(x_path_check)).Click();
                }
                Thread.Sleep(500);
                browers.FindElement(By.XPath(x_path_select_start_time)).Click();
                Thread.Sleep(500);
                var elem_start_time_label = browers.FindElementsByXPath(x_path_item_label);
                string time_format = "";
                string start_time = line_item.start_deliver_time;
                string end_time = line_item.end_deliver_time;
                if (elem_start_time_label.Count > 0)
                {
                    time_format = elem_start_time_label[0].Text;
                    if (time_format.Contains("AM") || time_format.Contains("PM"))
                    {
                        start_time = CovertTime24To12(line_item.start_deliver_time, 1);
                        end_time = CovertTime24To12(line_item.end_deliver_time, 2);
                    }

                    if (start_time == "" || end_time == "") return;
                    for (int k = 0; k <= elem_start_time_label.Count - 1; k++)
                    {
                        time_format = Regex.Replace(elem_start_time_label[k].Text, pattern, " ");
                        if (time_format == start_time)
                        {
                            var elem_time_start = browers.FindElement(By.XPath(x_path_item_select.Replace("item_index", (k + 1).ToString())));
                            Common.Common.MoveToXpath(elem_time_start, browers);
                            Thread.Sleep(100);
                            elem_time_start.Click();
                            break;
                        }
                    }
                }
                Thread.Sleep(500);
                browers.FindElement(By.XPath(x_path_select_end_time)).Click();
                Thread.Sleep(500);
                var elem_end_time_label = browers.FindElementsByXPath(x_path_item_label);
                if (elem_end_time_label.Count > 0)
                {
                    for (int k = 0; k <= elem_end_time_label.Count - 1; k++)
                    {
                        time_format = Regex.Replace(elem_end_time_label[k].Text, pattern, " ");
                        if (time_format.Contains(end_time))
                        {
                            var elem_time_end = browers.FindElement(By.XPath(x_path_item_select.Replace("item_index", (k + 1).ToString())));
                            Common.Common.MoveToXpath(elem_time_end, browers);
                            Thread.Sleep(100);
                            elem_time_end.Click();
                            break;
                        }
                    }
                }

                var elem_day = browers.FindElementsByXPath(x_path_day);
                if (elem_day.Count > 0)
                {
                    foreach (var elem in elem_day)
                    {
                        if (!elem.GetAttribute("class").Contains("mdc-chip--selected"))
                        {
                            elem.Click();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addDeliveryTime(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "addDeliveryTime", line_item.start_deliver_time + "|" + line_item.end_deliver_time, 2));
                    Ultities.Telegram.pushNotify(string.Format("addDeliveryTime [{0}] = {1}", index.ToString(), ex.ToString()), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addDeliveryTime [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addFrequency(int index = 0)
        {
            try
            {
                if (line_item.line_item_type == (Int16)line_item_type.DFP_CPD) return;
                if (line_item.frequency <= 0) return;

                Thread.Sleep(500);

                closeLightBox();

                string x_path_check = "//*[contains(local-name(),'frequency-cap-picker')]//*[contains(local-name(),'material-checkbox')]";
                string x_path_input = "//*[contains(local-name(),'frequency-cap-picker')]//*[contains(local-name(),'input')][contains(@aria-label,'Number of impressions')]";
                var elem_check = browers.FindElement(By.XPath(x_path_check));
                Common.Common.MoveToXpath(elem_check, browers);
                var elem_value = elem_check.GetAttribute("aria-checked");
                if (elem_value == "false")
                {
                    browers.FindElement(By.XPath(x_path_check)).Click();
                }
                browers.FindElement(By.XPath(x_path_input)).Clear();
                browers.FindElement(By.XPath(x_path_input)).SendKeys(line_item.frequency.ToString());
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addFrequency(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "addDeliveryTime", line_item.frequency.ToString(), 2));
                    Ultities.Telegram.pushNotify(string.Format("addFrequency [{0}] = {1}", index.ToString(), ex.ToString()), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addFrequency [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addTargetingInventory(int index = 0)
        {
            try
            {
                if (line_item.adunit.Count() > 0)
                {
                    closeLightBox();
                    string x_path_search_inventory = "//*[contains(local-name(),'material-picker-search')]//*[contains(local-name(),'input')][contains(@aria-label,'Search')]";
                    string x_path_check_load_more = "//*[contains(@class,'more-results-message')]";
                    string x_path_result_tree = "//*[contains(local-name(),'inventory-picker-item')]";
                    string x_path_tick = "(//*[contains(local-name(),'inventory-section-items')]//*[contains(local-name(),'material-button')][contains(@aria-label,'{0}')])[{1}]";
                    string x_path_inventory_targeting = "//*[contains(local-name(),'inventory-dimension')][contains(@ngcontrol,'inventoryTargeting')]";
                    string x_path_inventory_targeting_detail = "//*[contains(local-name(),'inventory-dimension-picker-section')]";
                    string x_path_label_load_ad_unit = "//*[contains(@aria-label,'Ad units')]";
                    string x_path_clear_ad_unit = "//*[contains(local-name(),'inventory-dimension-picker-section')]//*[contains(local-name(),'material-button')][contains(@aria-label,'Clear all selected items')]";
                    string x_path_ad_unit = "(//*[contains(local-name(),'material-picker-lobby-section')])[1]";

                    var elem = browers.FindElement(By.XPath(x_path_inventory_targeting));
                    Common.Common.MoveToXpath(elem, browers);

                    //click inventory
                    if (!Common.Common.checkXpathExist(x_path_inventory_targeting_detail, browers))
                    {
                        elem.Click();
                        Thread.Sleep(500);
                    }

                    //click adunit
                    if (!Common.Common.checkXpathExist(x_path_label_load_ad_unit, browers))
                    {
                        browers.FindElement(By.XPath(x_path_ad_unit)).Click();
                        Thread.Sleep(500);
                    }

                    //clear all data adunit
                    if (Common.Common.checkXpathExist(x_path_clear_ad_unit, browers))
                    {
                        browers.FindElement(By.XPath(x_path_clear_ad_unit)).Click();
                        Thread.Sleep(500);
                    }

                    Common.Common.waitForElement(20, x_path_result_tree, browers);

                    foreach (var path in line_item.adunit)
                    {
                        //string note_last = path.Replace(" ", "").Split('>').Last().ToString();
                        string note_last = path.adunit.Replace("/", ">").Split('>').Last().ToString();
                        browers.FindElement(By.XPath(x_path_search_inventory)).Clear();
                        Thread.Sleep(1000);

                        browers.FindElement(By.XPath(x_path_search_inventory)).SendKeys(note_last);
                        Thread.Sleep(1000);
                        // check load tree
                        bool is_check_load_tree = Common.Common.checkXpathExist(x_path_result_tree, browers);
                        if (is_check_load_tree)
                        {
                            //check load more
                            bool is_check_load_more = true;
                            while (is_check_load_more)
                            {
                                is_check_load_more = Common.Common.checkXpathExist(x_path_check_load_more, browers);
                                if (is_check_load_more)
                                {
                                    var elem_more = browers.FindElement(By.XPath(x_path_check_load_more));
                                    elem_more.Click();
                                    Thread.Sleep(2000);
                                    Common.Common.waitForElement(20, x_path_result_tree, browers);
                                }
                            }
                        }

                        var inventory_node = browers.FindElements(By.XPath(x_path_result_tree));

                        // Duyệt từng NODE
                        int d = 1;

                        foreach (var node in inventory_node)
                        {
                            string note_html = node.Text.Replace(System.Environment.NewLine, " ").Replace(" > ", ">").Split(' ').First();
                            //if (path.adunit.Replace("/", ">").IndexOf(note_html) > -1)
                            if (path.adunit.Replace(">" + note_last, "") == note_html)
                            {
                                string x_path_check = string.Format(x_path_tick, path.include == 1 ? "Include" : "Exclude", d.ToString());
                                var elem_check = browers.FindElement(By.XPath(x_path_check));
                                var elem_status = elem_check.GetAttribute("class");

                                if (path.include == 1)
                                {
                                    if (!elem_status.ToLower().Replace("include-icon", "").Contains("include"))
                                    {
                                        elem_check.Click();
                                    }
                                }
                                else
                                {
                                    if (!elem_status.ToLower().Replace("exclude-icon", "").Contains("exclude"))
                                    {
                                        elem_check.Click();
                                    }
                                }
                                Thread.Sleep(100);
                                break;
                            }
                            d += 1;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addTargetingInventory(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "addTargetingInventory", JsonConvert.SerializeObject(line_item.adunit), 2));
                    Ultities.Telegram.pushNotify(string.Format("addTargetingInventory [{0}] = {1}", index.ToString(), ex.ToString()), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addTargetingInventory [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void checkSizeAdUnitsAndPlacements(int index = 0)
        {
            try
            {
                if (line_item.line_item_type == (Int16)line_item_type.DFP_CPM || line_item.line_item_type == (Int16)line_item_type.DFP_CPM_TOI_UU)
                {
                    closeLightBox();
                    string x_path_error_placements = "//*[contains(local-name(),'inventory-dimension')]//*[contains(local-name(),'size-filter-panel')]//*[contains(local-name(),'html-notification-renderer')]//span";
                    string x_path_undo_placements = "//*[contains(local-name(),'inventory-dimension')]//*[contains(local-name(),'size-filter-panel')]//*[contains(local-name(),'notification-action')]//material-button";
                    string x_path_clear_placements = "//*[contains(local-name(),'inventory-dimension-picker-section')]//*[contains(local-name(),'material-button')][contains(@aria-label,'Clear all selected items')]";
                    string label_value = "";
                    if (Common.Common.checkXpathExist(x_path_error_placements, browers))
                    {
                        label_value = browers.FindElement(By.XPath(x_path_error_placements)).Text;
                    }

                    if (label_value.ToLower().IndexOf("inventory filtered based on size") >= 0)
                    {
                        var elem_undo = browers.FindElement(By.XPath(x_path_undo_placements));
                        Common.Common.MoveToXpath(elem_undo, browers);

                        browers.FindElement(By.XPath(x_path_clear_placements)).Click();
                        Thread.Sleep(300);
                        elem_undo.Click();
                        Thread.Sleep(300);
                        if (line_item.palcement.Count() > 0)
                        {
                            addPlacements(0);
                        }
                        else if (line_item.adunit.Count() > 0)
                        {
                            addTargetingInventory(0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    checkSizeAdUnitsAndPlacements(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "checkSizeAdUnitsAndPlacements", JsonConvert.SerializeObject(line_item.palcement), 2));
                    Ultities.Telegram.pushNotify(string.Format("checkSizeAdUnitsAndPlacements [{0}] = {1}", index.ToString(), ex.ToString()), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("checkSizeAdUnitsAndPlacements [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addPlacements(int index = 0)
        {
            try
            {
                if (line_item.palcement.Count() > 0)
                {
                    closeLightBox();
                    string x_path_inventory_targeting = "//*[contains(local-name(),'inventory-dimension')][contains(@ngcontrol,'inventoryTargeting')]";
                    string x_path_inventory_targeting_detail = "//*[contains(local-name(),'inventory-dimension-picker-section')]";
                    string x_path_label_load_placement = "//*[contains(@aria-label,'Placements')]";
                    string x_path_placements = "(//*[contains(local-name(),'material-picker-lobby-section')])[2]";
                    string x_path_search_placements = "//*[contains(local-name(),'material-picker-search')]//*[contains(local-name(),'input')][contains(@aria-label,'Search')]";
                    string x_path_select_placements = "*//material-picker-lobby//*/drx-include-only-picker-item[1]/div/material-checkbox/div";
                    string x_path_clear_ad_unit = "//*[contains(local-name(),'inventory-dimension-picker-section')]//*[contains(local-name(),'material-button')][contains(@aria-label,'Clear all selected items')]";

                    var elem = browers.FindElement(By.XPath(x_path_inventory_targeting));
                    Common.Common.MoveToXpath(elem, browers);

                    //click inventory
                    if (!Common.Common.checkXpathExist(x_path_inventory_targeting_detail, browers))
                    {
                        elem.Click();
                        Thread.Sleep(500);
                    }

                    //click adunit
                    if (!Common.Common.checkXpathExist(x_path_label_load_placement, browers))
                    {
                        browers.FindElement(By.XPath(x_path_placements)).Click();
                        Thread.Sleep(500);
                    }

                    //clear all placement
                    if (Common.Common.checkXpathExist(x_path_clear_ad_unit, browers))
                    {
                        browers.FindElement(By.XPath(x_path_clear_ad_unit)).Click();
                        Thread.Sleep(1500);
                    }
                    string name = "";
                    foreach (var item in line_item.palcement)
                    {
                        var arr_palcement = item.placement.Split(';');
                        foreach (var item_p in arr_palcement.ToList())
                        {
                            if (item_p == name)
                            {
                                continue;
                            }
                            else
                            {
                                name = item_p;
                            }


                            browers.FindElement(By.XPath(x_path_search_placements)).Clear();
                            Thread.Sleep(1000);
                            browers.FindElement(By.XPath(x_path_search_placements)).SendKeys(name);
                            Thread.Sleep(1500);

                            // select item first
                            if (Common.Common.checkXpathExist(x_path_select_placements, browers))
                            {
                                browers.FindElement(By.XPath(x_path_select_placements)).Click();
                                Thread.Sleep(1500);
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
                    addPlacements(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "addPlacements", JsonConvert.SerializeObject(line_item.palcement), 2));
                    Ultities.Telegram.pushNotify(string.Format("addPlacements [{0}] = {1}", index.ToString(), ex.ToString()), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addPlacements [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public int addSlot()
        {
            if (line_item.custom_targeting == "") return -1;

            //var comma = new Dictionary<int, string>()
            //{
            //    { 0, "0,5,10" },
            //    { 1, "1,6,11" },
            //    { 2, "2,7,12" },
            //    { 3, "3,8,13" },
            //    { 4, "4,9,14" }
            //};

            var comma = GetListSlot(line_item.custom_targeting);

            int islot = -1;
            string list_slot = "";
            if (line_item.line_item_type != (Int16)line_item_type.DFP_CPD) return islot;
            if (line_item.custom_targeting != "")
            {
                islot = comma.FirstOrDefault(x => x.Value == line_item.custom_targeting).Key;
                while (islot >= 0)
                {
                    list_slot = comma[islot];
                    List<string> lst = new List<string>();
                    var arr = list_slot.Split(',');
                    foreach (string item in arr)
                    {
                        lst.Add(item);
                    }
                    bool result = addTargeting(1, lst, "CPD slot (CPD)", "CPD slot", 0);
                    if (result)
                    {
                        //if (islot == 0) return 0;
                        //Check Slot Avaiable
                        bool check_avaiable = checkSlotAvaiable();
                        if (check_avaiable)
                        {
                            return islot;
                        }
                    }
                    islot = islot - 1;
                }
            }
            lstError.Add(new ErrorModel("Created LineItem", "addSlot", "False", islot));
            return -1;
        }

        private Dictionary<int, string> GetListSlot(string slot)
        {
            var result = new Dictionary<int, string>();
            string temp_slot = slot;
            var arr = slot.Split(',');
            int iSlot = Convert.ToInt32(arr[0]);
            while (iSlot >= 0)
            {
                result.Add(iSlot, temp_slot);
                arr = temp_slot.Split(',');
                temp_slot = "";
                foreach (string item in arr)
                {
                    if (temp_slot != "") temp_slot = temp_slot + ",";
                    temp_slot = temp_slot + (Convert.ToInt32(item) - 1).ToString();
                }
                iSlot = iSlot - 1;
            }

            return result;
        }

        private bool checkSlotAvaiable(int index = 0)
        {
            bool result = false;
            try
            {
                closeLightBox();

                string x_path_check_inventory = "//*[contains(@debugid,'check-inventory-forecast')] | //material-button[@debugid='check-inventory-button']";
                string x_path_check_deliver = "//*[contains(local-name(),'forecast-chart')]/forecast-headline/div[contains(@class,'to-deliver')]";

                if (Common.Common.checkXpathExist(x_path_check_inventory, browers))
                {
                    var elem = browers.FindElement(By.XPath(x_path_check_inventory));
                    Common.Common.MoveToXpath(elem, browers);
                    Thread.Sleep(500);
                    elem.Click();

                    string label_value = string.Empty;

                    wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(x_path_check_deliver)));

                    Thread.Sleep(500);

                    if (Common.Common.checkXpathExist(x_path_check_deliver, browers))
                    {
                        label_value = browers.FindElement(By.XPath(x_path_check_deliver)).Text;
                    }

                    if (label_value.ToLower().IndexOf("is unlikely to deliver") == -1)
                    {
                        return true;
                    }

                    return false;
                }
                else
                {
                    return true;
                }

            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    checkSlotAvaiable(1);
                }
                else
                {
                    result = false;
                    lstError.Add(new ErrorModel("Created LineItem", "checkSlotAvaiable", "False", 2));
                    //  Ultities.Telegram.pushNotify(string.Format("checkSlotAvaiable [{0}] = {1}", index.ToString(), ex.ToString()), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("checkSlotAvaiable [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
            return result;
        }

        public void addAge()
        {
            int include = 0;
            if (line_item.line_item_type == (Int16)line_item_type.DFP_CPD) return;
            if (line_item.age.Count <= 0) return;
            List<string> lst = new List<string>();
            foreach (Age age in line_item.age)
            {
                lst.Add(age.age);
                include = age.include;
            }

            Thread.Sleep(500);

            addTargeting(0, lst, "Age (myage)", "myage", 1);
        }

        public void addAudience()
        {
            if (line_item.line_item_type == (Int16)line_item_type.DFP_CPD) return;
            if (line_item.audience.Count <= 0) return;
            List<string> lst = new List<string>();
            foreach (Audience audience in line_item.audience)
            {
                lst.Add(audience.audience);
            }

            Thread.Sleep(500);

            addTargeting(1, lst, "Target Segment (mysegment)", "mysegment", 1, 0, 1);
        }

        public void addGender()
        {
            int include = 0;
            if (line_item.line_item_type == (Int16)line_item_type.DFP_CPD) return;
            if (line_item.gender.Count <= 0) return;
            List<string> lst = new List<string>();
            foreach (Gender gender in line_item.gender)
            {
                lst.Add(gender.gender);
                include = gender.include;
            }

            Thread.Sleep(500);

            addTargeting(include, lst, "Gender (mygender)", "mygender");
        }

        public void addArticle()
        {
            if (line_item.article.Count <= 0) return;
            List<string> lst = new List<string>();
            foreach (Article article in line_item.article)
            {
                lst.Add(article.article);
            }

            Thread.Sleep(500);

            addTargeting(1, lst, "Article ID (article)", "Article ID", 0, 0, 1);
        }

        public void addTag()
        {
            if (line_item.tag.Count <= 0) return;
            List<string> lst = new List<string>();
            foreach (Tag tag in line_item.tag)
            {
                lst.Add(tag.tag);
            }

            Thread.Sleep(500);

            addTargeting(1, lst, "Tags Article (tags)", "Tags Article", 0);
        }

        public void addBrandsafe()
        {
            if (line_item.brandsafe.Count <= 0) return;
            List<string> lst = new List<string>();
            foreach (Brandsafe brandsafe in line_item.brandsafe)
            {
                lst.Add(brandsafe.brandsafe);
            }

            Thread.Sleep(500);

            addTargeting(1, lst, "BrandSafe (brs)", "BrandSafe", 0);
        }

        public void addCategoryId()
        {
            if (line_item.categoryid.Count <= 0) return;
            List<string> lst = new List<string>();
            foreach (CategoryId category in line_item.categoryid)
            {
                lst.Add(category.categoryid);
            }

            Thread.Sleep(500);

            addTargeting(1, lst, "Category ID (category)", "Category ID", 0, 0, 1);
        }

        public bool addTargeting(int include, List<string> lst, string sType, string SearchType, int like = 1, int index = 0, int auto_add = 0)
        {
            bool result = false;
            try
            {
                if (lst.Count == 0) return false;
                closeLightBox();
                string last_text = "";
                int last_index = 1;
                bool exists_param = false;
                string x_path_custom_targeting = "//*[contains(local-name(),'custom-targeting-dimension')][contains(@ngcontrol,'customTargeting')]";
                string x_path_custom_targeting_open = "//*[contains(local-name(),'custom-targeting-dimension-picker-section')]";
                string x_path_custom_targeting_select = "//*[contains(local-name(),'custom-targeting-dimension')][contains(@ngcontrol,'customTargeting')]//*[contains(local-name(),'drx-targeting-dimension')]//*[contains(@class,'statement-key')]";
                string x_path_custom_targeting_dropdownlist = "(//*[contains(local-name(),'material-tree-dropdown')][contains(@popupclass,'material-tree-popup')])[item_index]";
                string x_path_text_drop = "(//*[contains(local-name(),'material-tree-filter')]//*[contains(local-name(),'input')][contains(@type,'text')])[item_index]";
                string x_path_custom_targeting_and_button = "(//*[contains(local-name(),'drx-targeting-dimension')]//*[contains(@class,'row-actions')])[item_index]//*[contains(local-name(),'material-button')]";
                string x_path_item_first = "(//*[contains(local-name(),'material-tree')][contains(@class,'tree-in-popup')]//*[contains(local-name(),'material-tree-group')])[1]";
                string x_path_elem_segment = "//*[contains(@id,'acx-overlay-container-TRAFFICKING')]//*[contains(@pane-id,'TRAFFICKING-')][contains(@class,'visible')]//*[contains(local-name(),'material-select-item')]";
                string x_path_empty_elem_segment = "//*[contains(@id,'acx-overlay-container-TRAFFICKING')]//*[contains(@pane-id,'TRAFFICKING-')][contains(@class,'visible')]//div[@empty-label]//span[contains(text(), 'No matches')]";
                string x_path_elem_drop_button = "(//*[contains(local-name(),'drx-targeting-dimension')]//*[contains(local-name(),'material-button-dropdown')][contains(@buttonarialabel,'Open more actions')])[item_index]";
                string x_path_elem_drop_clear_all = "//*[contains(@id,'acx-overlay-container-TRAFFICKING')]//*[contains(@pane-id,'TRAFFICKING-')][contains(@class,'visible')]//*[contains(local-name(),'material-list-item')][1]";
                string x_path_any_select = "(//*[contains(local-name(),'drx-targeting-dimension')]//*[contains(local-name(),'dropdown-button')])[item_index]";
                string x_path_any_select_value = "//*[contains(@id,'acx-overlay-container-TRAFFICKING')]//*[contains(@pane-id,'TRAFFICKING-')][contains(@class,'visible')]//*[contains(local-name(),'material-select-dropdown-item')][item_index]";
                string x_path_input_item = "(//*[contains(local-name(),'drx-targeting-dimension')]//*[contains(local-name(),'multi-suggest-input')])[item_index]//*[contains(local-name(),'input')][contains(@placeholder, 'Search or paste a comma-separated list')]";
                string x_path_create_item = "//*[contains(@id,'acx-overlay-container-TRAFFICKING')]//*[contains(@pane-id,'TRAFFICKING-')][contains(@class,'visible')]//*[contains(local-name(),'material-select-item')]";
                string x_path_button_ok_create = "//*[contains(@id,'acx-overlay-container-TRAFFICKING')]//*[contains(@pane-id,'TRAFFICKING-')][contains(@class,'visible')]//*[contains(local-name(),'material-button')][contains(@class,'btn-yes')]";
                var elem_custom_target = browers.FindElement(By.XPath(x_path_custom_targeting));
                Common.Common.MoveToXpath(elem_custom_target, browers);
                Thread.Sleep(200);

                if (!Common.Common.checkXpathExist(x_path_custom_targeting_open, browers))
                {
                    browers.FindElement(By.XPath(x_path_custom_targeting)).Click();
                    Thread.Sleep(500);
                }

                var elem_select = browers.FindElementsByXPath(x_path_custom_targeting_select);

                if (elem_select.Count > 0)
                {
                    last_text = string.Empty;
                    last_index = elem_select.Count;
                    for (int i = 0; i <= elem_select.Count - 1; i++)
                    {
                        var text = string.Empty;
                        var elem = elem_select[i];
                        var lable_select = elem.FindElement(By.XPath(".//*[contains(@class,'button-text')]"));
                        if (lable_select != null)
                        {
                            text = lable_select.Text;
                            if (text == sType)
                            {
                                last_text = "Select...";
                                last_index = i + 1;
                                exists_param = true;
                                browers.FindElement(By.XPath(x_path_any_select.Replace("item_index", (i + 1).ToString()))).Click();
                                Thread.Sleep(500);
                                browers.FindElement(By.XPath(x_path_any_select_value.Replace("item_index", include == 1 ? "1" : "2"))).Click();
                                Thread.Sleep(500);
                                browers.FindElement(By.XPath(x_path_elem_drop_button.Replace("item_index", (i + 1).ToString()))).Click();
                                Thread.Sleep(200);
                                browers.FindElement(By.XPath(x_path_elem_drop_clear_all)).Click();
                                Thread.Sleep(500);
                                goto Label_select_item;
                            }
                        }
                        if (i == elem_select.Count - 1)
                        {
                            last_text = text;
                            if (text != "Select...")
                            {
                                var elem_button = browers.FindElement(By.XPath(x_path_custom_targeting_and_button.Replace("item_index", (i + 1).ToString())));
                                if (elem_button != null)
                                {
                                    var attr = elem_button.GetAttribute("aria-disabled");
                                    if (attr == "false")
                                    {
                                        elem_button.Click();
                                        last_text = "Select...";
                                        last_index++;
                                        Thread.Sleep(1000);
                                    }
                                }
                            }
                        }
                    }
                Label_select_item:
                    if (last_text == "Select...")
                    {
                        if (exists_param == false)
                        {
                            browers.FindElement(By.XPath(x_path_custom_targeting_dropdownlist.Replace("item_index", last_index.ToString()))).Click();
                            Thread.Sleep(1000);
                            browers.FindElement(By.XPath(x_path_text_drop.Replace("item_index", last_index.ToString()))).Clear();
                            browers.FindElement(By.XPath(x_path_text_drop.Replace("item_index", last_index.ToString()))).SendKeys(SearchType);
                            Thread.Sleep(1000);
                            browers.FindElement(By.XPath(x_path_item_first)).Click();
                            Thread.Sleep(1000);
                            browers.FindElement(By.XPath(x_path_any_select.Replace("item_index", last_index.ToString()))).Click();
                            Thread.Sleep(500);
                            browers.FindElement(By.XPath(x_path_any_select_value.Replace("item_index", include == 1 ? "1" : "2"))).Click();
                            Thread.Sleep(500);
                        }

                        if (sType == "CPD slot (CPD)")
                        {
                            Thread.Sleep(500);
                            var elem_input = browers.FindElement(By.XPath(x_path_input_item.Replace("item_index", last_index.ToString())));
                            elem_input.Clear();
                            elem_input.SendKeys(String.Join(",", lst));
                        }
                        else
                        {
                            foreach (string item in lst)
                            {
                                bool exist_item = false;
                                Thread.Sleep(500);
                                var elem_input = browers.FindElement(By.XPath(x_path_input_item.Replace("item_index", last_index.ToString())));
                                elem_input.Clear();
                                elem_input.SendKeys(item);
                                Thread.Sleep(300);
                                elem_input.Click();
                                Thread.Sleep(1000);
                                wait.Until(ExpectedConditions.ElementExists(By.XPath(x_path_elem_segment + "|" + x_path_empty_elem_segment)));
                                Thread.Sleep(100);
                                var elem_segment = browers.FindElementsByXPath(x_path_elem_segment);

                                foreach (var elem_item in elem_segment)
                                {
                                    var segemt_text = elem_item.FindElement(By.XPath(".//*[contains(local-name(),'span')]")).Text;
                                    if (like == 1)
                                    {
                                        if (segemt_text.Contains(item) && segemt_text.ToString().ToLower().Contains("create values") == false)
                                        {
                                            exist_item = true;
                                            elem_item.Click();
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (segemt_text == item)
                                        {
                                            exist_item = true;
                                            elem_item.Click();
                                            break;
                                        }
                                    }

                                    if (auto_add == 1 && segemt_text.ToString().ToLower().Contains("create values"))
                                    {
                                        exist_item = true;
                                        elem_item.Click();
                                        wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(x_path_button_ok_create)));
                                        browers.FindElement(By.XPath(x_path_button_ok_create)).Click();
                                        Thread.Sleep(1500);
                                        break;
                                    }
                                }

                                if (!exist_item && auto_add == 1)
                                {
                                    Thread.Sleep(700);
                                    if (Common.Common.checkXpathExist(x_path_create_item, browers))
                                    {
                                        browers.FindElement(By.XPath(x_path_create_item)).Click();
                                        wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(x_path_button_ok_create)));
                                        browers.FindElement(By.XPath(x_path_button_ok_create)).Click();
                                        Thread.Sleep(1500);
                                    }
                                }
                            }
                        }
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    return addTargeting(include, lst, sType, SearchType, like, 1, auto_add);
                }
                else
                {
                    result = false;
                    lstError.Add(new ErrorModel("Created LineItem", "addTargeting", include + "|" + JsonConvert.SerializeObject(lst) + "|" + sType + "|" + SearchType + "|" + like.ToString() + "|" + auto_add.ToString(), 2));
                    Ultities.Telegram.pushNotify(string.Format(sType + " [{0}] = {1}", index.ToString(), ex.ToString()), tele_group_id, tele_token);
                    Console.WriteLine(string.Format(sType + " [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }

            return result;
        }

        public void addDevice(int index = 0)
        {
            try
            {
                if (line_item.line_item_type == (Int16)line_item_type.DFP_CPD) return;
                if (line_item.device.Count <= 0) return;

                Thread.Sleep(500);

                closeLightBox();

                string x_path_device_category = "//*[contains(local-name(),'device-category-dimension')]";
                string x_path_device_category_open = "//*[contains(local-name(),'device-category-dimension')]//*[contains(local-name(),'material-picker-search')]";
                string x_path_device_item = "//*[contains(local-name(),'device-category-dimension')]//*[contains(local-name(),'material-picker-lobby')]//*[contains(local-name(),'drx-include-exclude-picker-item')]";
                string x_path_device_clear_all = "//*[contains(local-name(),'device-category-dimension')]//*[contains(local-name(),'material-button')][contains(@aria-label,'Clear all selected items')][contains(@aria-disabled,'false')]";

                var elem_click = browers.FindElement(By.XPath(x_path_device_category));
                Common.Common.MoveToXpath(elem_click, browers);

                if (!Common.Common.checkXpathExist(x_path_device_category_open, browers))
                {
                    elem_click.Click();
                    Thread.Sleep(500);
                }

                if (Common.Common.checkXpathExist(x_path_device_clear_all, browers))
                {
                    browers.FindElement(By.XPath(x_path_device_clear_all)).Click();
                    Thread.Sleep(500);
                }

                var elem_device = browers.FindElementsByXPath(x_path_device_item);
                if (elem_device.Count > 0)
                {
                    foreach (var elem in elem_device)
                    {
                        var item = elem.FindElement(By.XPath(".//*[contains(local-name(),'span')]"));
                        if (item != null)
                        {
                            foreach (Device device in line_item.device)
                            {
                                if (device.device == item.Text)
                                {
                                    elem.FindElement(By.XPath(".//*[contains(local-name(),'material-button')][contains(@aria-label,'Include')]")).Click();
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
                    addDevice(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "addDevice", JsonConvert.SerializeObject(line_item.device), 2));
                    Ultities.Telegram.pushNotify(string.Format("addDevice [{0}] = {1}", index.ToString(), ex.ToString()), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addDevice [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void addSameSlot(string slot, int index = 0)
        {
            try
            {
                int retry = 0;

            re_check_iventory:

                var banner_dfp = Repository.getBannerDfpDetail(product_id, request_id);
                if (banner_dfp == null) return;
                if (banner_dfp.Rows.Count == 0) return;

                //var comma = new Dictionary<int, string>()
                //{
                //    { 0, "0,5,10" },
                //    { 1, "1,6,11" },
                //    { 2, "2,7,12" },
                //    { 3, "3,8,13" },
                //    { 4, "4,9,14" }
                //};

                var comma = GetListSlot(slot);

                int count_lineitem = banner_dfp.Rows.Count;
                int min_slot = banner_dfp.AsEnumerable().Min(x => x.Field<int>("Slot"));
                if (min_slot < 0) min_slot = 0;
                int count_slot_together = banner_dfp.AsEnumerable().Where(x => x.Field<int>("Slot") == min_slot).Count();
                if (count_slot_together != count_lineitem && retry <= 10)
                {
                    foreach (DataRow row in banner_dfp.Rows)
                    {
                        int key_id = Convert.ToInt32(row["Id"]);
                        int index_slot = Convert.ToInt32(row["Slot"]);
                        int ProductId = Convert.ToInt32(row["ProductId"]);
                        string link = row["Link"].ToString();
                        if (min_slot != index_slot)
                        {
                            if (line_item == null)
                            {
                                line_item = new LineItemViewModel();
                                line_item.line_item_type = (Int32)line_item_type.DFP_CPD;
                                line_item.custom_targeting = comma[min_slot];
                            }

                            browers.Url = link;
                            Thread.Sleep(4000);

                            int islot = addSlot();

                            Thread.Sleep(500);
                            saveLineItem();

                            Thread.Sleep(500);
                            Repository.updateSlotSyncBannerDFP(key_id, islot);

                            if (min_slot > islot)
                            {
                                goto re_check_iventory;
                            }
                        }
                    }
                    retry = retry + 1;
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addSameSlot(slot, 1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "addSameSlot", "False", 2));
                    Ultities.Telegram.pushNotify(string.Format("addSameSlot [{0}] = {1}", index.ToString(), ex.ToString()), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addSameSlot [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void selectSegment(string custom_targeting, int index = 0)
        {
            try
            {
                switch (line_item.line_item_type)
                {
                    case (Int16)line_item_type.DFP_CPD:
                        closeLightBox();
                        string x_path_node_segment = "//div[@class='list-group _ngcontent-TRAFFICKING-28']//material-select-dropdown-item//*[contains(@class,'text-segmen')]";
                        string x_path_check_box = "(//div[@class='list-group _ngcontent-TRAFFICKING-28']//material-select-dropdown-item//*[contains(local-name(),'material-checkbox')])[{index_node}]";
                        string x_path_input_segment = "//*[contains(@aria-label,'Search or paste a comma')]";
                        string x_path_more_action = "//*[contains(local-name(),'custom-value-picker')]//*[contains(@buttonarialabel,'Open more actions')]";
                        string x_path_clear_all = "//*[contains(@class,'visible')]//*[contains(@size,'auto')]/material-list-item[1]";

                        Thread.Sleep(2000);

                        // Select more action
                        browers.FindElement(By.XPath(x_path_more_action)).Click();

                        Thread.Sleep(2000);

                        browers.FindElement(By.XPath(x_path_clear_all)).Click();

                        browers.FindElement(By.XPath(x_path_input_segment)).Click();

                        // waiting....
                        Thread.Sleep(2000);

                        //select segment
                        var segment = "," + custom_targeting + ",";
                        var segment_node = browers.FindElements(By.XPath(x_path_node_segment));
                        int d = 0;
                        foreach (var item in segment_node)
                        {
                            string value_node = "," + item.Text.Trim() + ",";
                            if (segment.IndexOf(value_node) >= 0)
                            {
                                //  check node
                                browers.FindElement(By.XPath(x_path_check_box.Replace("{index_node}", (d + 1).ToString()))).Click();
                                Thread.Sleep(400);
                            }
                            d += 1;
                        }
                        if (d == 0) Console.WriteLine("segment invalid !!! ");
                        break;
                    case (Int16)line_item_type.DFP_CPM:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                lstError.Add(new ErrorModel("Created LineItem", "selectSegment", "False", 2));
                Ultities.Telegram.pushNotify("selectSegment " + ex.ToString(), tele_group_id, tele_token);
                Console.WriteLine("selectSegment = " + ex.ToString());
            }
        }

        public void clickCheckInventory(int index = 0)
        {
            try
            {
                RetryOperation(() =>
                {
                    closeLightBox();
                    string x_path = "//*[contains(@debugid,'check-inventory-forecast')]";

                    if (!SafeClick(By.XPath(x_path), maxRetries: 3, timeoutSeconds: 15))
                    {
                        throw new Exception("Cannot click check inventory button");
                    }
                }, maxRetries: 3, operationName: "clickCheckInventory");
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    clickCheckInventory(1);
                }
                else
                {
                    lstError.Add(new ErrorModel("Created LineItem", "clickCheckInventory", "False", 2));
                    Ultities.Telegram.pushNotify(string.Format("clickCheckInventory [{0}] = {1}", index.ToString()), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("clickCheckInventory [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public int checkInventory(int slot_check_async = -1)
        {
            try
            {
                int slot = -1;
                switch (line_item.line_item_type)
                {
                    case (Int16)line_item_type.DFP_CPD:
                        string custom_targeting = line_item.custom_targeting; //(!(string.IsNullOrEmpty(slot_check_async))) ? "" : line_item.custom_targeting;
                        string x_path_check_deliver = "//*[contains(local-name(),'forecast-chart')]/forecast-headline/div[contains(@class,'to-deliver')]";
                        string x_path_more_action = "//*[contains(local-name(),'custom-value-picker')]//*[contains(@buttonarialabel,'Open more actions')]";
                        string x_path_clear_all = "//*[contains(@class,'visible')]//*[contains(@size,'auto')]/material-list-item[1]";

                        // dịch chuyển vị trí
                        var comma = new Dictionary<int, string>()
                        {
                            { 0, "0,5,10" },
                            { 1, "1,6,11" },
                            { 2, "2,7,12" },
                            { 3, "3,8,13" },
                            { 4, "4,9,14" }
                        };

                        // Check banner đồng bộ
                        if (slot_check_async >= 0)
                        {
                            custom_targeting = comma.FirstOrDefault(x => x.Key == slot_check_async).Value;
                        }

                        if (!string.IsNullOrEmpty(custom_targeting))
                        {
                            bool is_unlikely = true;
                            while (is_unlikely)
                            {
                                // Chọn Segment
                                selectSegment(custom_targeting);

                                var filter = comma.FirstOrDefault(x => x.Value == custom_targeting);

                                if (filter.Key == 0)
                                {
                                    slot = filter.Key;
                                    break; // Nếu là index 0 thì k cần check Inventory kể cả đã có rồi
                                }

                                // Click button
                                clickCheckInventory();

                                wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.XPath(x_path_check_deliver)));

                                Thread.Sleep(500);

                                // Check đã có chưa
                                string label_value = string.Empty;
                                if (Common.Common.checkXpathExist(x_path_check_deliver, browers))
                                {
                                    label_value = browers.FindElement(By.XPath(x_path_check_deliver)).Text;
                                }
                                else
                                {
                                    break;
                                }

                                if (label_value.ToLower().IndexOf("is unlikely to deliver") == -1)
                                {
                                    // Chưa có ko check tiếp.
                                    slot = filter.Key;
                                    break;
                                }
                                else
                                {
                                    int _key = filter.Key;
                                    filter = comma.FirstOrDefault(x => x.Key == _key - 1); // nếu dịch lên ở vị trí đầu tiên thì dừng lại                                                                                           
                                    custom_targeting = filter.Value;
                                    // Select more action
                                    browers.FindElement(By.XPath(x_path_more_action)).Click();

                                    Thread.Sleep(2000);
                                    browers.FindElement(By.XPath(x_path_clear_all)).Click();

                                    continue;
                                }
                            }
                        }
                        break;
                    case (Int16)line_item_type.DFP_CPM:
                        return -1;
                }

                return slot;
            }
            catch (Exception ex)
            {
                Ultities.Telegram.pushNotify("checkInventory " + ex.ToString(), tele_group_id, tele_token);
                Console.WriteLine("checkInventory = " + ex.ToString());
                return -1;
            }
        }

        public void addGeography(int index = 0)
        {
            try
            {
                bool is_location_empty = true;

                if (line_item.line_item_type == (Int16)line_item_type.DFP_CPD) return;

                if (line_item.usd == 2)
                {
                    LocationViewModel objLocation = new LocationViewModel();
                    objLocation.location_id = 0;
                    objLocation.location_name = "Vietnam Country";
                    objLocation.region = 0;
                    objLocation.include = 2;
                    line_item.location.Add(objLocation);
                }

                // Không phải là CPM, chỉ là dạng GÓI
                if (line_item.location == null || line_item.location.Count == 0){
                    LocationViewModel objLocation = new LocationViewModel();
                    objLocation.location_id = 0;
                    objLocation.location_name = "VietNam";
                    objLocation.region = 0;
                    objLocation.include = 1;
                    line_item.location.Add(objLocation);
                    is_location_empty = false;
                }
                
                //if (line_item.location == null) return;
                //if (line_item.location.Count == 0) return;

                Thread.Sleep(1000);

                string x_path_input = "//*[contains(@activityprefix,'TargetingPresetPanel')]//input";
                string x_path_list_box = "//*[contains(@class,'pane selections')][contains(@class,'visible')]//*[contains(@class,'list-group')]/material-select-dropdown-item[1]";
                string x_path_geo = "//*[contains(local-name(),'geo-dimension')]";
                string x_path_search = "//*[contains(local-name(),'material-picker-search')]//*[contains(local-name(),'input')][contains(@aria-label,'Search')]";
                string x_path_select_item_include = "(//*[contains(local-name(),'drx-include-exclude-picker-item')])[1]//*[contains(local-name(),'material-button')][contains(@aria-label,'Include')][1]";
                string x_path_select_item_exclude = "(//*[contains(local-name(),'drx-include-exclude-picker-item')])[1]//*[contains(local-name(),'material-button')][contains(@aria-label,'Exclude')][1]";
                string x_path_clear_geo = "//*[contains(local-name(),'geo-dimension-picker-section')]//*[contains(local-name(),'material-button')][contains(@aria-label,'Clear all selected items')][contains(@aria-disabled,'false')]";
                string x_path_geo_detail = "//*[contains(local-name(),'geo-dimension-picker-section')]";

                closeLightBox();

                var elem = browers.FindElement(By.XPath(x_path_geo));
                Common.Common.MoveToXpath(elem, browers);

                if (!Common.Common.checkXpathExist(x_path_geo_detail, browers))
                {
                    elem.Click();
                    Thread.Sleep(1000);
                }

                if (Common.Common.checkXpathExist(x_path_clear_geo, browers))
                {
                    browers.FindElement(By.XPath(x_path_clear_geo)).Click();
                    Thread.Sleep(1000);
                }

                var lst = (from x in line_item.location
                           where x.region == 1
                           select new { location_name = x.location_name, include = 1 }).Distinct();
                if (lst.Count() > 0)
                {
                    elem = browers.FindElement(By.XPath(x_path_input));
                    Common.Common.MoveToXpath(elem, browers);
                    Thread.Sleep(800);
                    foreach (var item in lst)
                    {
                        browers.FindElement(By.XPath(x_path_input)).Clear();
                        Thread.Sleep(800);
                        browers.FindElement(By.XPath(x_path_input)).SendKeys(item.location_name);

                        var elem_list_box = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(x_path_list_box)));
                        Common.Common.MoveToXpath(elem_list_box, browers);
                        Thread.Sleep(1000);
                        elem_list_box.Click();
                        Thread.Sleep(2000);
                    }
                }

                lst = (from x in line_item.location
                       where x.region == 0
                       select new { location_name = x.location_name, include = x.include }).Distinct();
                if (lst.Count() > 0)
                {
                    if (Common.Common.checkXpathExist(x_path_search, browers))
                    {
                        elem = browers.FindElement(By.XPath(x_path_search));
                        Common.Common.MoveToXpath(elem, browers);
                        Thread.Sleep(800);
                    }

                    // Dictionary chứa các cặp giá trị cần thay thế: key (từ) -> value (sang)
                    var locationNameReplacements = new Dictionary<string, string>
                    {
                        { "da nang", "Đà Nẵng" },
                        { "ho chi minh city v", "TP HCM" }
                    };

                    foreach (var item in lst)
                    {
                        bool success = false;

                        // Thử dùng input "Add targeting preset" trước
                        string x_path_add_targeting_preset = "//input[@aria-label='Add targeting preset' and @role='combobox']";
                        if (Common.Common.checkXpathExist(x_path_add_targeting_preset, browers))
                        {
                            try
                            {
                                var inputElement = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(x_path_add_targeting_preset)));
                                Common.Common.MoveToXpath(inputElement, browers);
                                Thread.Sleep(1000);

                                inputElement.Clear();
                                Thread.Sleep(1000);

                                // Áp dụng các thay thế từ dictionary
                                string locationName = item.location_name.ToLower();
                                foreach (var replacement in locationNameReplacements)
                                {
                                    locationName = locationName.Replace(replacement.Key, replacement.Value);
                                }
                                inputElement.SendKeys(locationName);
                                Thread.Sleep(1000);
                                inputElement.SendKeys(Keys.Enter);
                                Thread.Sleep(1000); // Đợi dữ liệu được nạp
                                success = true;
                            }
                            catch (Exception exPreset)
                            {
                                ErrorWriter.WriteLog(LogPath, "addGeography", string.Format("Error with targeting preset input: {0}", exPreset.Message));
                            }
                        }

                        // Nếu không tìm thấy input "Add targeting preset" hoặc không có dữ liệu, dùng cách cũ: target theo tỉnh thành lẻ
                       // if (!success || !is_location_empty)
                       // {
                            try
                            {
                                // Đảm bảo search input có thể interact được
                                var elem_search = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(x_path_search)));
                                Common.Common.MoveToXpath(elem_search, browers);
                                Thread.Sleep(1000);

                                elem_search.Clear();
                                Thread.Sleep(1000);
                                elem_search.SendKeys(item.location_name);
                                Thread.Sleep(2000);

                                // Chờ và click vào include/exclude button
                                string x_path_select = item.include == 1 ? x_path_select_item_include : x_path_select_item_exclude;
                                if (Common.Common.checkXpathExist(x_path_select, browers))
                                {
                                    var elem_check = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(x_path_select)));
                                    var elem_status = elem_check.GetAttribute("class");
                                    bool needClick = false;

                                    if (item.include == 1)
                                    {
                                        if (!elem_status.ToLower().Replace("include-icon", "").Contains("include"))
                                        {
                                            needClick = true;
                                        }
                                    }
                                    else
                                    {
                                        if (!elem_status.ToLower().Replace("exclude-icon", "").Contains("exclude"))
                                        {
                                            needClick = true;
                                        }
                                    }

                                    if (needClick)
                                    {
                                        Common.Common.MoveToXpath(elem_check, browers);
                                        Thread.Sleep(1000);
                                        elem_check.Click();
                                        Thread.Sleep(1000);
                                    }
                                }
                            }
                            catch (Exception exSearch)
                            {
                                ErrorWriter.WriteLog(LogPath, "addGeography", string.Format("Error with search input for '{0}': {1}", item.location_name, exSearch.Message));
                            }
                       // }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log lỗi
                string errorMsg = string.Format("Exception occurred - index={0}, error={1}, stackTrace={2}, location_data={3}",
                    index, ex.Message, ex.StackTrace, JsonConvert.SerializeObject(line_item.location));
                ErrorWriter.WriteLog(LogPath, "addGeography - ERROR", errorMsg);

                if (index == 0)
                {
                    Thread.Sleep(1000);
                    addGeography(1);
                }
                else
                {
                    string errorDetails = JsonConvert.SerializeObject(line_item.location);
                    ErrorWriter.WriteLog(LogPath, "addGeography - FINAL ERROR", string.Format("Failed after retry. Details: {0}, Error: {1}", errorDetails, ex.ToString()));
                    lstError.Add(new ErrorModel("Created LineItem", "addGeography", errorDetails, 2));
                    Ultities.Telegram.pushNotify(string.Format("addGeography [{0}] = {1}", index.ToString(), ex.ToString()), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("addGeography [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public void saveLineItem(int index = 0)
        {
            string link_order = string.Empty;
            try
            {
                closeLightBox();
                string x_path = "//*[contains(@primarybuttontext,'Save')]//*[contains(local-name(),'material-button')][contains(@class,'btn-yes')] | //material-button[.//div[contains(@class,'content') and normalize-space(text())='Save']]"; // %             
                browers.FindElement(By.XPath(x_path)).Click();
                Thread.Sleep(1500);
                link_order = browers.Url;
                if (link_order.Contains("line_item_id"))
                {
                    Console.WriteLine("CREATE LINEITEM SUCCESS: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": " + line_item.name);
                }
                else
                {
                    Thread.Sleep(1000);
                    if (!link_order.Contains("line_item_id"))
                    {
                        index = index + 1;
                        if (index == 1)
                        {
                            saveLineItem(1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (index == 0)
                {
                    Thread.Sleep(1000);
                    saveLineItem(1);
                }
                else
                {
                    string url_error = Common.Common.TakeScreen(browers, request_id, product_id);
                    lstError.Add(new ErrorModel("Created LineItem", "saveLineItem", link_order, 2));
                    Ultities.Telegram.pushNotify("saveLineItem " + ex.ToString() + " #$# " + HttpUtility.UrlEncode(url_error), tele_group_id, tele_token);
                    Console.WriteLine(string.Format("saveLineItem [{0}] = {1}", index.ToString(), ex.ToString()));
                }
            }
        }

        public int saveDatabase(int slot = -1)
        {
            try
            {
                string link_order = browers.Url;
                Console.WriteLine("Link LineItems: " + link_order);
                if (link_order.Contains("line_item_id"))
                {
                    int parent_id = order_id;
                    int type = (Int32)dfp_setup_type.line_item;
                    int iResult = Repository.updateStatusFilter(parent_id, link_order, type, product_id, request_id, "", slot, line_item.data_line_item_id);
                    Ultities.Telegram.pushNotify("CREATE LINEITEM SUCCESS: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": " + line_item.name + " Link LineItem: " + HttpUtility.UrlEncode(link_order), tele_group_id, tele_token);
                    return iResult;
                }
                else
                {
                    Ultities.Telegram.pushNotify("KHÔNG TẠO ĐƯỢC LINEITEM: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ": " + line_item.name, tele_group_id, tele_token);
                    return -1;
                }
            }
            catch (Exception ex)
            {
                lstError.Add(new ErrorModel("Created LineItem", "saveDatabase", "Có lỗi trong quá trình cập nhật database", 2));
                Ultities.Telegram.pushNotify("saveDatabase " + ex.ToString(), tele_group_id, tele_token);
                Console.WriteLine("BrowserActionLibLineItem - saveDatabase = " + ex.ToString());
                return -1;
            }
        }

        private string CovertTime24To12(string time, int type)
        {
            if (time == "00:00") return "12:00 AM";

            var arr = time.Split(':');
            if (arr.Length > 0)
            {
                if (Convert.ToInt32(arr[0]) > 12)
                {
                    return (Convert.ToInt32(arr[0]) - 12).ToString() + ":" + arr[1] + " PM";
                }
                else if (Convert.ToInt32(arr[0]) == 12)
                {
                    return "12:" + arr[1] + " PM";
                }
                else
                {
                    return Convert.ToInt32(arr[0]).ToString() + ":" + arr[1] + " AM";
                }
            }
            return "";
        }
    }
}
