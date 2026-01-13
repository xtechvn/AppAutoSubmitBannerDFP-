
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LibCommon;
using WDSE.Decorators;
using WDSE.ScreenshotMaker;
using OpenQA.Selenium.Support.Extensions;
using WDSE;
using POLY.ResizeImg;

namespace AppAutoSubmitBannerDFP.Common
{
    public static class Common
    {
        private static string tele_group_id = ConfigurationManager.AppSettings["tele_group_id"];
        private static string tele_token = ConfigurationManager.AppSettings["tele_token"];
        private static string url_upload = ConfigurationManager.AppSettings["url_upload"];
        private static string url_read_image = ConfigurationManager.AppSettings["url_read_image"];

        public static byte[] SerializeObject(object iObject)
        {
            try
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                bf.Serialize(ms, iObject);
                ms.Flush();
                ms.Position = 0;
                byte[] bytBuffer = new byte[ms.Length];
                ms.Read(bytBuffer, 0, Convert.ToInt32(ms.Length));
                ms.Close();
                ms = null;
                bf = null;
                return bytBuffer;
            }
            catch (Exception ex)
            {

                Console.Write(ex.ToString());
                return null;
            }
        }
        public static string RandString()
        {
            return new Random().Next(10000, 99999).ToString();
        }
        public static bool IsInteger(double num)
        {
            if (Math.Ceiling(num) == num && Math.Floor(num) == num)
                return true;
            else
                return false;
        }



        /// <summary>
        ///decrypt string
        ///@param <string> strString
        ///@param <string> strKeyphrase
        ///@return <string>
        /// </summary>
        /// 

        // Hàm này dùng để mã hóa chuỗi password theo chuẩn MD5
        public static string Encrypt(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            var md5 = new MD5CryptoServiceProvider();
            byte[] valueArray = Encoding.ASCII.GetBytes(value);
            valueArray = md5.ComputeHash(valueArray);
            var sb = new StringBuilder();
            for (int i = 0; i < valueArray.Length; i++)
                sb.Append(valueArray[i].ToString("x2").ToLower());
            return sb.ToString();
        }


        public static string Decode(string strString, string strKeyPhrase)
        {
            Byte[] byt = Convert.FromBase64String(strString);
            strString = System.Text.Encoding.UTF8.GetString(byt);
            strString = KeyED(strString, strKeyPhrase);
            return strString;
        }


        public static string Encode(string strString, string strKeyPhrase)
        {

            strString = KeyED(strString, strKeyPhrase);
            Byte[] byt = System.Text.Encoding.UTF8.GetBytes(strString);
            strString = Convert.ToBase64String(byt);
            return strString;
        }

        private static string KeyED(string strString, string strKeyphrase)
        {
            int strStringLength = strString.Length;
            int strKeyPhraseLength = strKeyphrase.Length;

            System.Text.StringBuilder builder = new System.Text.StringBuilder(strString);

            for (int i = 0; i < strStringLength; i++)
            {
                int pos = i % strKeyPhraseLength;
                int xorCurrPos = (int)(strString[i]) ^ (int)(strKeyphrase[pos]);
                builder[i] = Convert.ToChar(xorCurrPos);
            }

            return builder.ToString();
        }

        public static int getQuantityFake(long product_id)
        {
            try
            {
                return 0;
                //string s_product_id = product_id.ToString();
                //string s_numb_last = product_id.ToString().Substring(s_product_id.Length - 1, 1);
                //int _round = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(s_numb_last) / 2));
                //return _round;
            }
            catch (Exception ex)
            {

                Console.Write(ex.ToString());
                return 0;
            }
        }

        public static string IsNullOrEmpty(String value)
        {
            return value == null ? "" : value;
        }

        public static double convertToPound(double value, string unit)
        {
            double rs = 0;
            switch (unit)
            {
                case "ounces":
                    rs = value * 0.0625;
                    break;
                case "grams":
                    rs = value * 0.0022046;
                    break;
                case "kilograms":
                    rs = value * 2204622.6218;
                    break;
                case "tonne":
                    rs = value * 2204.62262;
                    break;
                case "kiloton":
                    rs = value * 2.000000;
                    break;
                default:
                    rs = value;
                    break;
            }
            return rs;

        }

        public static string LoaiBoDau(string str)
        {
            var regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string strFormD = str.Normalize(System.Text.NormalizationForm.FormD).ToLower();
            return (regex.Replace(strFormD, String.Empty).Replace("đ", "d").Replace("Đ", "D")).ToString().Trim();
        }

        public static string ReverDateTimeTiny(string strDate)
        {
            if (strDate != "")
            {
                strDate = strDate.Replace('/', '-');
                string[] ArrDate = strDate.Split('-');

                string DD = ArrDate[0].ToString();
                string MM = ArrDate[1].ToString();
                string YYYY = ArrDate[2].ToString().Split(' ')[0];
                string JoinDate = MM + "-" + DD + "-" + YYYY;
                return JoinDate;
            }
            else
            {
                return "";
            }
        }

        public static void WriteLog(string path, string msg)
        {
            if (path.Trim() != "")
            {
                StreamWriter sw;
                string sFullName = string.Format("{0:dd-MM-yyyy}.log", DateTime.Now);
                string sDirName = path + @"Log\" + string.Format("{0:yyyy}", DateTime.Now) + @"\" + string.Format("{0:MM}", DateTime.Now);

                if (!Directory.Exists(sDirName + "\\")) Directory.CreateDirectory(sDirName + "\\");
                sFullName = sDirName + "\\" + sFullName;
                if (File.Exists(sFullName))
                {
                    sw = System.IO.File.AppendText(sFullName);
                    sw.WriteLine(string.Format("{0:hh:mm:ss tt}", DateTime.Now) + ": " + msg);
                    sw.Flush();
                }
                else
                {
                    sw = new StreamWriter(sFullName);
                    sw.WriteLine(string.Format("{0:hh:mm:ss tt}", DateTime.Now) + ": " + msg);
                }

                if (sw != null) sw.Close();
            }
        }

        public static string ConvertToUnSign(string strText)
        {
            strText = UnicodeToPlain(strText);
            strText = System.Text.RegularExpressions.Regex.Replace(strText, " ", "-");
            strText = System.Text.RegularExpressions.Regex.Replace(strText, "[^0-9a-zA-Z\\-]+", "");
            strText = System.Text.RegularExpressions.Regex.Replace(strText, "--", "-", System.Text.RegularExpressions.RegexOptions.Multiline);
            return strText.ToLower();
        }


        //public static string BuildLinkProductAmazon(string ASIN, string Title)
        //{
        //    string Link = string.Empty;
        //    if (Title.Length > 100) { Title = Title.Substring(0, 100); }
        //    Link = "/amazon/item/" + Utilities.Common.ConvertToUnSign(Title) + "-" + ASIN + "/";
        //    return Link;
        //}

        public static string TrimWords(string strText, int quantity = 12)
        {
            string[] splitText = strText.Trim().Replace("  ", " ").Replace("    ", " ").Split(' ');
            return splitText.Length > 12 ? String.Join(" ", splitText.Skip(0).Take(quantity)) + "..." : strText;
        }

        public static string UnicodeToPlain(string strEncode)
        {
            if (string.IsNullOrEmpty(strEncode)) return string.Empty;
            string oStr = null;
            int p1 = 0;
            int p2 = 0;
            p2 = strEncode.Length;
            p1 = 0;
            oStr = "";
            while (p1 < p2)
            {
                switch (strEncode.Substring(p1, 1))
                {
                    case "à":
                    case "á":
                    case "ạ":
                    case "ả":
                    case "ã":
                    case "ă":
                    case "ằ":
                    case "ắ":
                    case "ẳ":
                    case "ặ":
                    case "ẵ":
                    case "â":
                    case "ầ":
                    case "ấ":
                    case "ẩ":
                    case "ẫ":
                    case "ậ":
                        oStr += "a";
                        break;
                    case "À":
                    case "Á":
                    case "Ạ":
                    case "Ả":
                    case "Ã":
                    case "Ă":
                    case "Ằ":
                    case "Ắ":
                    case "Ẳ":
                    case "Ặ":
                    case "Ẵ":
                    case "Â":
                    case "Ầ":
                    case "Ấ":
                    case "Ẩ":
                    case "Ẫ":
                    case "Ậ":
                        oStr += "A";
                        break;
                    case "è":
                    case "é":
                    case "ẹ":
                    case "ẻ":
                    case "ẽ":
                    case "ê":
                    case "ề":
                    case "ế":
                    case "ể":
                    case "ệ":
                    case "ễ":
                        oStr += "e";
                        break;
                    case "È":
                    case "É":
                    case "Ẹ":
                    case "Ẻ":
                    case "Ẽ":
                    case "Ê":
                    case "Ề":
                    case "Ế":
                    case "Ể":
                    case "Ệ":
                    case "Ễ":
                        oStr += "E";
                        break;
                    case "ò":
                    case "ó":
                    case "ọ":
                    case "ỏ":
                    case "õ":
                    case "ơ":
                    case "ờ":
                    case "ớ":
                    case "ở":
                    case "ợ":
                    case "ỡ":
                    case "ô":
                    case "ồ":
                    case "ố":
                    case "ổ":
                    case "ộ":
                    case "ỗ":
                        oStr += "o";
                        break;
                    case "Ò":
                    case "Ó":
                    case "Ọ":
                    case "Ỏ":
                    case "Õ":
                    case "Ơ":
                    case "Ờ":
                    case "Ớ":
                    case "Ở":
                    case "Ợ":
                    case "Ỡ":
                    case "Ô":
                    case "Ồ":
                    case "Ố":
                    case "Ổ":
                    case "Ộ":
                    case "Ỗ":
                        oStr += "O";
                        break;
                    case "ù":
                    case "ú":
                    case "ụ":
                    case "ủ":
                    case "ũ":
                    case "ư":
                    case "ừ":
                    case "ứ":
                    case "ử":
                    case "ự":
                    case "ữ":
                        oStr += "u";
                        break;
                    case "Ù":
                    case "Ú":
                    case "Ụ":
                    case "Ủ":
                    case "Ũ":
                    case "Ư":
                    case "Ừ":
                    case "Ứ":
                    case "Ử":
                    case "Ự":
                    case "Ữ":
                        oStr += "U";
                        break;
                    case "ỳ":
                    case "ý":
                    case "ỵ":
                    case "ỷ":
                    case "ỹ":
                        oStr += "y";
                        break;
                    case "Ỳ":
                    case "Ý":
                    case "Ỵ":
                    case "Ỷ":
                    case "Ỹ":
                        oStr += "Y";
                        break;
                    case "ì":
                    case "í":
                    case "ị":
                    case "ỉ":
                    case "ĩ":
                        oStr += "i";
                        break;
                    case "Ì":
                    case "Í":
                    case "Ị":
                    case "Ỉ":
                    case "Ĩ":
                        oStr += "I";
                        break;
                    case "đ":
                        oStr += "d";
                        break;
                    case "Đ":
                        oStr += "D";
                        break;
                    default:
                        oStr += strEncode.Substring(p1, 1);
                        break;
                }
                p1 = p1 + 1;
            }
            return oStr;
        }

        public static string RemoveNameSite(string domain, string input)
        {
            string result = "/";
            try
            {
                result = System.Text.RegularExpressions.Regex.Replace(input, domain, "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            catch (Exception)
            {
            }
            return result.ToLower();
        }

        public static string GetFormatDateTimeForDetail(DateTime dt)
        {
            try
            {
                System.Globalization.CultureInfo ci = null;
                System.Globalization.DateTimeFormatInfo dtfi = null;
                ci = System.Globalization.CultureInfo.CreateSpecificCulture("vi");
                dtfi = ci.DateTimeFormat;
                return dtfi.GetDayName(dt.DayOfWeek) + ", " + dt.ToString("dd/MM/yyyy, HH:mm");
            }
            catch (Exception)
            {
            }
            return "";
        }

        private static Dictionary<string, object> ObjectSaved = new Dictionary<string, object>();

        public static Dictionary<string, object> SaveData
        {
            get { return Common.ObjectSaved; }
            set { Common.ObjectSaved = value; }
        }

        public static string GetFormatDateForComment(DateTime date, bool format = true)
        {
            try
            {
                string value = "", hours = "", minutes = "";
                TimeSpan valueTime = DateTime.Now - date;
                int day = valueTime.Days;
                int hour = valueTime.Hours;
                int minute = valueTime.Minutes;
                int second = valueTime.Seconds;
                // format mặc định là giờ:phút|ngày
                // ngược lại là 1 ngày hoặc 1 giờ trước
                if (format == true)
                {
                    if (day > 0)
                    {

                        hours = date.Hour.ToString();
                        if (date.Minute < 10)
                        {
                            minutes = "0" + date.Minute.ToString();
                        }
                        else
                        {
                            minutes = date.Minute.ToString();
                        }
                        value = hours + ":" + minutes + " | " + date.Day + "-" + date.Month + "-" + date.Year;
                    }
                    else
                    {
                        if (hour > 0)
                        {
                            hours = valueTime.Hours.ToString();
                            if (valueTime.Minutes == 0)
                            {

                                value = hours + " giờ trước đó";
                            }
                            if (valueTime.Minutes < 10)
                            {
                                minutes = "0" + valueTime.Minutes.ToString();
                                value = hours + ":" + minutes + " phút trước đó";
                            }
                            else
                            {
                                minutes = valueTime.Minutes.ToString();
                                value = hours + ":" + minutes + " phút trước đó";
                            }

                        }
                        else
                        {
                            if (minute > 0)
                            {
                                if (second > 0)
                                {
                                    value = minute + " phút " + second + " giây trước đó";
                                }
                                else
                                {
                                    value = minute.ToString() + " phút trước đó";
                                }
                            }
                            else
                            {
                                value = second.ToString() + " giây trước đó";
                            }
                        }
                    }
                }
                else
                {
                    // định dạng ở trang home box bình luận mới
                    if (day > 0)
                    {
                        value = day.ToString() + " ngày trước";
                    }
                    else if (hour > 0)
                    {
                        value = hour.ToString() + " giờ trước";
                    }
                    else if (minute > 0)
                    {
                        value = minute.ToString() + " phút trước";
                    }
                    else
                    {
                        value = second.ToString() + " giây trước";
                    }
                }
                return value;
            }
            catch
            {
                return "";
            }
        }


        public static string GetNameToUnSignNotSpace(string strText)
        {
            strText = UnicodeToPlain(strText);
            strText = System.Text.RegularExpressions.Regex.Replace(strText, " ", "", System.Text.RegularExpressions.RegexOptions.Multiline);
            strText = System.Text.RegularExpressions.Regex.Replace(strText, "-", "", System.Text.RegularExpressions.RegexOptions.Multiline);
            return strText.ToLower();
        }

        public static string HelperShowTimeFolder(DateTime publishdate)
        {
            TimeSpan valueTime = DateTime.Now - publishdate;
            int second = 1;
            int minute = second * 60;
            int hour = minute * 60;
            int day = hour * 24;
            string result = "";
            if (valueTime.TotalSeconds < minute)
            {
                result = valueTime.Seconds.ToString() + " giây trước";
            }
            else if (valueTime.TotalSeconds < hour)
            {
                result = valueTime.Minutes.ToString() + " phút trước";
            }
            else if (valueTime.TotalSeconds < day)
            {
                result = valueTime.Hours.ToString() + " giờ trước";
            }
            else if (valueTime.TotalSeconds < (day * 2))
            {
                result = valueTime.Days.ToString() + " ngày trước";
            }
            else
            {
                result = GetFormatDateTimeForDetail(publishdate);
            }
            return result;
        }


        public static string RemoveSpecialCharacters(string input)
        {
            Regex r = new Regex("(?:[^a-z0-9 ]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            return r.Replace(input, String.Empty);
        }

        public static string GetFormatDateTimeFolder(DateTime dt)
        {
            try
            {
                return dt.ToString("dd/MM/yyyy HH:mm");
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string GetFormatDateTimeAM(DateTime dt)
        {
            try
            {
                return dt.ToString("h:mm tt | dd/MM/yyyy");
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string SplitLead(string strText, int length)
        {
            if (!string.IsNullOrEmpty(strText))
            {
                if (length > 0 && length < strText.Length)
                {
                    return strText.Substring(0, length) + " [...]";
                }
                else
                {
                    return strText;
                }
            }
            else
            {
                return string.Empty;
            }

        }

        public static string ConvertTimeFormat(DateTime date)
        {
            try
            {
                string str = "";
                int second = date.Second;
                int minute = date.Minute;
                int hour = date.Hour;
                if (hour > 0)
                {
                    str = hour + ":" + minute + " " + date.Day + "/" + date.Month + "/" + date.Year;
                }
                else
                {
                    if (minute > 0)
                    {
                        str = minute + " minutes ago";
                    }
                    else
                    {
                        str = second + " seconds ago";
                    }
                }
                return str;
            }
            catch
            {
                return date.ToString();
            }
        }


        public static bool ContainsUnicodeCharacter(string input)
        {
            const int MaxAnsiCode = 255;

            return input.Any(c => c > MaxAnsiCode);
        }


        #region Ham Chung | CreateBy: Kuonglv


        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }

        public static DateTime ConvertTimestampToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        #endregion

        public static bool checkXpathExist(string s_x_path, ChromeDriver browers)
        {
            try
            {
                List<IWebElement> list_input_link_elements = browers.FindElements(By.XPath(s_x_path)).ToList();
                if (list_input_link_elements.Count() == 0)
                {
                    return false;
                }
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static string startupPath = AppDomain.CurrentDomain.BaseDirectory;
        
        public static void waitForElement(int seconds, string x_path, ChromeDriver browers)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(browers, TimeSpan.FromSeconds(seconds));
                wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.XPath(x_path)));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() + " error xpath-->" + x_path);
            }
        }

        public static string getContentByXpath(string html_source, string x_path)
        {
            try
            {
                var document = new HtmlDocument();
                document.LoadHtml(html_source);
                var nodes = document.DocumentNode.SelectNodes(x_path);

                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        var result = (node.InnerText).Replace("&nbsp;", " ");
                        return result;
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return string.Empty;

            }
        }

        public static string ToUnixTimeMilliSeconds(DateTime dateTime)
        {
            DateTimeOffset dto = new DateTimeOffset(dateTime.ToUniversalTime());
            return dto.ToUnixTimeMilliseconds().ToString();
        }

        public static void MoveToXpath(IWebElement element, ChromeDriver browers)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)browers;
            js.ExecuteScript("arguments[0].scrollIntoView({'block':'center','inline':'center'});", element);
            System.Threading.Thread.Sleep(100);
        }

        public static void RemoveCss(IWebElement element, ChromeDriver browers)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)browers;
            js.ExecuteScript("arguments[0].removeAttribute('style');", element);
            System.Threading.Thread.Sleep(100);
        }

        public static void DisplayNoneElem(IWebElement element, ChromeDriver browers)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)browers;
            js.ExecuteScript("arguments[0].style.display = 'none';", element);
            System.Threading.Thread.Sleep(100);
        }

        public static void MoveToBottomPage1(ChromeDriver browers)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)browers;
            long height = (long)js.ExecuteScript("return document.body.scrollHeight;");            
            js.ExecuteScript("window.scrollTo(0, " + height + ");");
            System.Threading.Thread.Sleep(100);           
        }

        public static string getLineItemId(string url)
        {
            if (!url.Contains("line_item_id="))
            {
                System.Threading.Thread.Sleep(1500);
            }
            int start = url.IndexOf("line_item_id=");
            string sub_line_item = url.Substring(start + 13);
            start = sub_line_item.IndexOf("&");
            if (start >= 0)
            {
                return sub_line_item.Substring(0, start);
            }
            else
            {
                return sub_line_item;
            }
        }

        public static string getOrderId(string url)
        {
            if (!url.Contains("order_id="))
            {
                System.Threading.Thread.Sleep(1500);
            }
            
            if (!url.Contains("order_id=")) return "";

            int start = url.IndexOf("order_id=");
            string sub_order = url.Substring(start + 9);
            start = sub_order.IndexOf("&");
            if (start >= 0)
            {
                return sub_order.Substring(0, start);
            }
            else
            {
                return sub_order;
            }
        }

        public static string TakeScreen(ChromeDriver browers, int requestId, int productId)
        {
            string url = "";
            try
            {
                Bitmap bmp;                
                var obj_push_image = new RefUploadFile.Upload();
                obj_push_image.Url = url_upload + "/Upload.asmx";
                string path = "/auto_setup_banner/image/" + DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + "/" + requestId.ToString() + "_" + productId.ToString() + "_" + Guid.NewGuid().ToString() + ".png";                Screenshot screen_shoot = ((ITakesScreenshot)browers).GetScreenshot();
                byte[] screen_shot_as_byte_array = screen_shoot.AsByteArray;                
                using (var ms = new MemoryStream(screen_shot_as_byte_array))
                {
                    bmp = new Bitmap(ms);
                }
                var obj_img = new imgTinBai();
                obj_img.img = bmp;
                string response_push_img = obj_push_image.SendFileV2(SerializeObjectImage(obj_img), path, bmp.Width, bmp.Height);
                if (response_push_img == "OK")
                {
                    return url_read_image + path;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi chup hinh: " + ex.ToString());
            }
            return url;
        }       

        public static byte[] SerializeObjectImage(object iObject)
        {
            try
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                bf.Serialize(ms, iObject);
                ms.Flush();
                ms.Position = 0;
                byte[] bytBuffer = new byte[ms.Length];
                ms.Read(bytBuffer, 0, Convert.ToInt32(ms.Length));
                ms.Close();
                ms = null;
                bf = null;
                return bytBuffer;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return null;
            }
        }
    }
    public class Crypt_Xor
    {
        /// <summary>
        ///Gen Key Private
        ///@param <string> strString
        ///@param <string> strKeyphrase
        ///@return <string>
        /// </summary>
        /// 
        private static string KeyED(string strString, string strKeyphrase)
        {
            int strStringLength = strString.Length;
            int strKeyPhraseLength = strKeyphrase.Length;

            System.Text.StringBuilder builder = new System.Text.StringBuilder(strString);

            for (int i = 0; i < strStringLength; i++)
            {
                int pos = i % strKeyPhraseLength;
                int xorCurrPos = (int)(strString[i]) ^ (int)(strKeyphrase[pos]);
                builder[i] = Convert.ToChar(xorCurrPos);
            }

            return builder.ToString();
        }

        /// <summary>
        ///encrypt string
        ///@param <string> strString
        ///@param <string> strKeyphrase
        ///@return <string>
        /// </summary>
        /// 
        public static string Encode(string strString, string strKeyPhrase)
        {

            strString = KeyED(strString, strKeyPhrase);
            Byte[] byt = System.Text.Encoding.UTF8.GetBytes(strString);
            strString = Convert.ToBase64String(byt);
            return strString;
        }

        /// <summary>
        ///decrypt string
        ///@param <string> strString
        ///@param <string> strKeyphrase
        ///@return <string>
        /// </summary>
        /// 
        public static string Decode(string strString, string strKeyPhrase)
        {
            Byte[] byt = Convert.FromBase64String(strString);
            strString = System.Text.Encoding.UTF8.GetString(byt);
            strString = KeyED(strString, strKeyPhrase);
            return strString;
        }
    }
}
