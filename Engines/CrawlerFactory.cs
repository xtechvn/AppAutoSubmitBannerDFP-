using AppAutoSubmitBannerDFP.ActionPartial;
using AppAutoSubmitBannerDFP.Behaviors;
using AppAutoSubmitBannerDFP.Common;
using AppAutoSubmitBannerDFP.Interfaces;
using AppAutoSubmitBannerDFP.Model;
using AppAutoSubmitBannerDFP.RefSendMailV2;
using AppAutoSubmitBannerDFP.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

namespace AppAutoSubmitBannerDFP.Engines
{
    public class CrawlerFactory : ICrawlerFactory

    {
        private static int task_job_type = Convert.ToInt32(ConfigurationManager.AppSettings["task_job_type"]);
        private static string tele_group_id = ConfigurationManager.AppSettings["tele_group_id"];
        private static string tele_token = ConfigurationManager.AppSettings["tele_token"];
        private static string sMailerUrl = ConfigurationManager.AppSettings["MailUrl"];
        private static string is_production = ConfigurationManager.AppSettings["is_production"].ToString();
        private static string DOMAIN_WEBSITE_ORDER_DEFAULT = @"https://admanager.google.com/27973503#delivery/order/order_overview/order_id=2961633598&tab=line_items";

        private static string auth_username = ConfigurationManager.AppSettings["auth_username"];
        private static string auth_password = ConfigurationManager.AppSettings["auth_password"];
        private static string url_salecloud = ConfigurationManager.AppSettings["url_salecloud"];
        private static string key_connect_api_sc = ConfigurationManager.AppSettings["key_connect_api_sc"];

        //  private readonly ICrawlerService _crawler;
        private readonly IOderService order_service;
        private readonly ILineItemCPCService line_item_cpc_service;
        private readonly ICreativeService creative_service;
        private readonly IProposalService proposal_service;

        public CrawlerFactory(IOderService _order_service, ILineItemCPCService _line_item_cpc_service, ICreativeService _creative_service, IProposalService _proposal_service)
        {
            order_service = _order_service;
            line_item_cpc_service = _line_item_cpc_service;
            creative_service = _creative_service;
            proposal_service = _proposal_service;
        }

        public void MainProcess(string url, ChromeDriver browers, BannerEntities banner)
        {
            
            try
            {
                List<ErrorModel> lstError = new List<ErrorModel>();

                var BrowserLib = new BrowserActionLibMain(browers, banner, lstError);
                int iResultOrder = -1;
                int iResultLineItem = -1;

                BrowserLib.addUrl();
                //Tao Order
                Thread.Sleep(100);
                bool create_order = order_service.createOrder(browers, banner.order, banner.ProductId, banner.RequestId, lstError, out iResultOrder);

                string url_create_line_item = "";
                //Tao LineItem
                foreach (var line_item in banner.order.line_item)
                {
                    iResultLineItem = -1;

                    Thread.Sleep(2000);
                    url_create_line_item = BrowserLib.addUrlLineItem(url_create_line_item);

                    Thread.Sleep(2000);
                    BrowserLib.addAdType(line_item.ad_type);

                    bool create_line_item = line_item_cpc_service.submitForm(browers, line_item, banner.ProductId, banner.RequestId, iResultOrder, lstError, out iResultLineItem);

                    //Tao Creative
                    Thread.Sleep(1000);
                    BrowserLib.addTabCreative();

                    Thread.Sleep(500);
                    string url_create_creative = "";
                    bool creative_first = true;
                    string url_add_new_creative = "";
                    int d = 0;

                    //var obj_gr_creative = line_item.creative.GroupBy(c => (c.add_sizes,c.ads_size)).Select(group => new
                    //{
                    //    ads_size = group.Key,
                    //    creative = group.ToList()
                    //});
                    var obj_gr_creative = line_item.creative.GroupBy(c => new { c.add_sizes, c.ads_size }).Select(group => new
                    {
                        add_sizes = group.Key.add_sizes,
                        ads_size = group.Key.ads_size,
                        creative = group.ToList()
                    });

                    foreach (var item_creative in obj_gr_creative)
                    {
                        // foreach (var creative in item_creative.creative)
                        // {
                        var creative = item_creative.creative[0]; // lấy ra size đầu nếu lặp
                        // Chuyển tạo creative
                        if (d == 0)
                        {
                            url_add_new_creative = browers.Url; // Lưu lại link tạo creative ở lần đầu để submit cho các lần kế tiếp creative
                            url_create_creative = BrowserLib.addNewCreative(line_item.line_item_type, line_item.ad_type, url_create_creative, 0, creative.ads_size, creative.add_sizes);
                            if (url_create_creative == "") continue; //Ko tìm thấy link creative sẽ bỏ qua luôn
                        }
                        else
                        {
                            //browers.Url = url_add_new_creative;
                            //url_create_creative = BrowserLib.addNewCreative(line_item.line_item_type, line_item.ad_type, url_add_new_creative);
                            // với line item = master/companion thì sẽ set creative trong creative. Bỏ qua bước set từ line item
                            if (!((line_item.ad_type == (int)AdsType.master_companion && creative_first == false)))
                            {
                                browers.Navigate().GoToUrl(url_add_new_creative);
                                url_create_creative = BrowserLib.addNewCreative(line_item.line_item_type, line_item.ad_type, "", 0, creative.ads_size, creative.add_sizes);
                                if (url_create_creative == "") continue; //Ko tìm thấy link creative sẽ bỏ qua luôn
                            }
                        }


                        Thread.Sleep(3000);
                        // Với các size cụ thể thì sẽ đi qua Standard creative.
                        // Nếu thuộc theo vị trí thì bỏ qua bước này
                        bool is_native = (url_create_creative.IndexOf("NATIVE") == -1 && url_create_creative.IndexOf("template_id") == -1);
                        //is_native = true: là set theo vị trí  | Fasle: set theo size cụ thể

                        if (is_native)
                        {
                            BrowserLib.addStandardCreative(line_item.line_item_type, line_item.ad_type, creative.creative_type, creative.creative_template, creative.add_sizes, creative_first, 0, line_item.position_name);
                        }
                        Thread.Sleep(500);

                        // Bước này sẽ set Creative trong creative nếu là master_companion
                        if ((line_item.ad_type == (int)AdsType.master_companion && creative_first == false))
                        {
                            BrowserLib.addNewCreativeSync(line_item.ad_type, creative_first, creative.creative_template);
                        }
                        

                        bool create_creative = false;
                        if (line_item.ad_type == (int)AdsType.master_companion && creative_first == false)
                        {
                            create_creative = creative_service.submitFormCPMCreativeMasterCompanion(browers, creative, banner.ProductId, banner.RequestId, iResultLineItem, line_item.line_item_type, lstError);
                        }
                        else
                        {
                            create_creative = creative_service.submitForm(browers, creative, banner.ProductId, banner.RequestId, iResultLineItem, line_item.line_item_type, lstError);
                        }

                        creative_first = false;
                        d += 1;
                        // }
                    }
                }

                //CPD-SAME_SLOT
                if (banner.order.sync == 1) // 1: Đồng bộ | 0: ko đồng bộ
                {
                    var slot = "";
                    if (banner.order.line_item.Count > 0)
                    {
                        slot = banner.order.line_item[0].custom_targeting;
                    }
                    var BrowserActionLib = new BrowserActionLibLineItem(browers, null, banner.ProductId, banner.RequestId, -1, lstError);
                    BrowserActionLib.addSameSlot(slot);
                }

                //SEND MAIL
                sendMailToUser(banner, lstError);
            }
            catch (Exception ex)
            {
                Console.WriteLine(" [*] MainProcess." + ex.ToString());
                string url_error = Common.Common.TakeScreen(browers, banner.RequestId, banner.ProductId);
                Ultities.Telegram.pushNotify(" [*] MainProcess.  " + ex.ToString() + " #$# " + HttpUtility.UrlEncode(url_error), tele_group_id, tele_token);
            }
        }

        // Send mail toi user phụ trách bài
        private bool sendMailToUser(BannerEntities banner, List<ErrorModel> lstError)
        {
            string sBody = string.Empty;
            string response_sale = string.Empty;
            string mail_owner = string.Empty;
            try
            {
                // var objMailer = new ServiceMail();
                // objMailer.Url = sMailerUrl;
                //string link_order = string.Empty;
                //string link_line_item = string.Empty;
                //string link_creative = string.Empty;

                //if (string.IsNullOrEmpty(banner.order.trafficker))
                //{
                //    Console.WriteLine("trafficker is empty !...");
                //    return false;
                //}

                var obj_lst_link = getMailSetupDfp(banner.RequestId, banner.ProductId);

                if (obj_lst_link.Count() <= 0)
                {
                    return false;
                }

                if (is_production == "0")
                {
                    banner.order.trafficker = "cuonglv8";
                    mail_owner = banner.order.trafficker + "@fpt.com";
                }
                else
                {
                    // Lấy ra email của nv nhận triển khai
                    mail_owner = getEmailReceiverDeploy(banner.RequestId);
                    banner.order.trafficker = mail_owner.Split('@').First();
                }
                //  Ultities.Telegram.pushNotify(string.Format("mail_owner: Banner: {0}, is_production: {1}", mail_owner, is_production), tele_group_id, tele_token);




                string mail_saller_booking = "cuonglv8@fpt.com";

                string owner_name = banner.order.trafficker;

                sBody = "<div style='font-size:11pt;font-family: arial;'>";
                sBody += "Dear <b>" + owner_name + "</b>, </br></br>";
                sBody += "Banner Dfp của bạn đã được Bot submit tự động: </br></br>";

                sBody += "- Tên Banner: <b>" + banner.order.name + "</b> </br></br>";

                sBody += @"- link_order_salescloud: <b><a target='_blank' href='https://salescloud.fptonline.net/request/index/detail-v2?id=" + banner.RequestId.ToString() + "'>" + @"https://salescloud.fptonline.net/request/index/detail-v2?id=" + banner.RequestId.ToString() + "</a></b> </br></br>";

                var order_list = obj_lst_link.AsEnumerable().Where(x => x.Type == 1);

                foreach (var _order in order_list)
                {
                    string link_order = _order.Link;
                    int order_id = Convert.ToInt32(_order.Id);

                    sBody += "- link_order: <b><a target='_blank' href='" + link_order + "'>" + link_order + "</a></b> </br></br>";

                    var line_item_list = obj_lst_link.AsEnumerable().Where(x => x.Type == 2 && x.ParentId == order_id);
                    foreach (var _line in line_item_list)
                    {
                        string link_line_item = _line.Link.ToString();
                        int line_id = Convert.ToInt32(_line.Id);

                        sBody += "- link_line_item: <b><a target='_blank' href='" + link_line_item + "'>" + link_line_item + "</a></b> </br></br>";

                        var creative_list = obj_lst_link.AsEnumerable().Where(x => x.Type == 3 && x.ParentId == line_id);
                        foreach (var _creative in creative_list)
                        {
                            string link_creative = _creative.Link;
                            int creative_id = Convert.ToInt32(_creative.Id);

                            sBody += "- link_creative: <b><a target='_blank' href='" + link_creative + "'>" + link_creative + "</a></b> </br></br>";
                        }
                    }
                }

                if (lstError.Count > 0)
                {
                    sBody += "</br><b>Các bug phát sinh trong quá trình setup banner tự động:</b></br></br>";

                    var lst = from err in lstError
                              where err.type == 1
                              select err;
                    if (lst.Count() > 0)
                    {
                        sBody += "<b>- Lỗi tạo Order: </b></br></br>";
                        foreach (var err_order in lst)
                        {
                            sBody += "+ Tại hàm " + err_order.function + " : Giá trị: " + err_order.detail + "</br></br>";
                        }
                    }

                    lst = from err in lstError
                          where err.type == 2
                          select err;
                    if (lst.Count() > 0)
                    {
                        sBody += "<b>- Lỗi tạo LineItems: </b></br></br>";
                        foreach (var err_order in lst)
                        {
                            sBody += "+ Tại hàm " + err_order.function + " : Giá trị: " + err_order.detail + "</br></br>";
                        }
                    }

                    lst = from err in lstError
                          where err.type == 3
                          select err;
                    if (lst.Count() > 0)
                    {
                        sBody += "<b>- Lỗi tạo Creative: </b></br></br>";
                        foreach (var err_order in lst)
                        {
                            sBody += "+ Tại hàm " + err_order.function + " : Giá trị: " + err_order.detail + "</br></br>";
                        }
                    }
                }

                sBody += "</b></br></br>";

                //sBody += "Đây là phiên bản thử nghiệm. Mọi ý kiến đóng góp liên hệ bộ phận kỹ thuật để được hỗ trợ </br></br>";
                //sBody += " Thân ái, </br></br>";
                // sBody += " Ban quản trị Salescloud </br>";
                sBody += "-------------------------------------------- </br>";
                sBody += "<span style='font-style: italic;font-size:9pt; color:#666666;'>Đây là mail tự động từ hệ thống. Vui lòng KHÔNG trả lời mail.</span></br>";
                sBody += "</div>";

                //objMailer.SendMail("noreply@vnexpressmail.net", mail_saller_booking, mail_owner, "", "[BOT BANNER SUBMIT] '" + banner.order.name, sBody, mail_owner.ToString(), "[SALESCLOUD.BOT]");
                //Ultities.Telegram.pushNotify(string.Format("SEND MAIL SUCCESS : Banner: {0}, Request: {1}", banner.ProductId, banner.RequestId), tele_group_id, tele_token);

                var rs = SendMail("noreply@vnexpressmail.net", mail_owner, mail_saller_booking, "", "[BOT BANNER SUBMIT] '" + banner.order.name, sBody, mail_owner.ToString(), "[SALESCLOUD.BOT]");
                if (rs == "")
                {
                    Console.WriteLine("sendMailToUser: Lỗi sendmail " + banner.RequestId);
                    Ultities.Telegram.pushNotify(string.Format("SEND MAIL THAT BAI: Banner: {0}, Request: {1}", banner.ProductId, banner.RequestId), tele_group_id, tele_token);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("sendMailToUser: " + ex.ToString());
                Ultities.Telegram.pushNotify(string.Format("SEND MAIL THAT BAI: Banner: {0}, Request: {1}, Error: {2}", banner.ProductId, banner.RequestId, ex.ToString()), tele_group_id, tele_token);
                return false;
            }
        }
        private static List<LinkModel> getMailSetupDfp(int request_id, int product_id)
        {
            try
            {
                string param = "request_id=" + request_id + "&product_id=" + product_id;

                var res = ConnectApiSc.getDataSalesCloudMethodGet("/api/dfp-services/send-mail-setup-dfp", param);

                var data = JObject.Parse(res);
                if (data["error"].ToString() == "0")
                {
                    var obj_link = JsonConvert.DeserializeObject<List<LinkModel>>(data["data"].ToString());
                    return obj_link;
                }
                return new List<LinkModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("getMailSetupDfp: " + ex.ToString());
                Ultities.Telegram.pushNotify(string.Format("getMailSetupDfp: request_id: {0},Error: {1}", request_id, ex.ToString()), tele_group_id, tele_token);
                return new List<LinkModel>();
            }
        }
        //private static string updateStatusFilter(int parent_id, string link, int type, int product_Id, int request_id, string keyname, int Slot, int data_id)
        //{
        //    try
        //    {
        //        string param = "parent_id=" + parent_id + "link=" + link + "type=" + type + "product_Id=" 
        //            + product_Id + "request_id=" + request_id + "keyname=" + keyname + "Slot=" + Slot + "data_id=" + data_id;

        //        var res = getDataSalesCloudMethodGet("/api/dfp-services/insert-dfp-setup", param);

        //        var data = JObject.Parse(res);
        //        if (data["error"].ToString() == "0")
        //        {
        //            string email = data["data"][0]["link"].ToString();
        //            return email;
        //        }
        //        return "cuonglv8@fpt.com";
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("getEmailReceiverDeploy: " + ex.ToString());
        //        Ultities.Telegram.pushNotify(string.Format("getEmailReceiverDeploy: request_id: {0},Error: {1}", request_id, ex.ToString()), tele_group_id, tele_token);
        //        return "";
        //    }
        //}
        private static string getEmailReceiverDeploy(int request_id)
        {
            try
            {
                string param = "request_id=" + request_id;

                var res = ConnectApiSc.getDataSalesCloudMethodGet("/api/dfp-services/get-email-deploy-dfp", param);

                var data = JObject.Parse(res);
                if (data["error"].ToString() == "0")
                {
                    string email = data["data"][0]["email"].ToString();
                    return email;
                }
                return "cuonglv8@fpt.com";
            }
            catch (Exception ex)
            {
                Console.WriteLine("getEmailReceiverDeploy: " + ex.ToString());
                Ultities.Telegram.pushNotify(string.Format("getEmailReceiverDeploy: request_id: {0},Error: {1}", request_id, ex.ToString()), tele_group_id, tele_token);
                return "";
            }
        }
        private static string SendMail(string from, string to, string cc, string bcc, string subject, string body, string user, string alias)
        {
            try
            {
                string param = "from=" + from + "&email_received=" + to + "&cc=" + cc + "&bcc=" + bcc + "&subject=" + subject + "&body=" + HttpUtility.UrlEncode(body) + "&user_received=" + user + "&alias=" + alias;
                //string data = JsonConvert.SerializeObject(prr);
                // string token = AppAutoSubmitBannerDFP.Common.Common.Encode(data, "5fDmJ8Ze"); // Lay ra content   
                var rs_mail_scl = getDataSalesCloudMethodPost("/api/mail-services/send-mail", param);
                if (!rs_mail_scl)
                {
                    var objMailer = new ServiceMail();
                    objMailer.Url = sMailerUrl;

                    // Gửi mail qua service server win
                    objMailer.SendMail("noreply@vnexpressmail.net", to, cc, bcc, "[BOT BANNER SUBMIT] '" + subject, body, user.ToString(), "[SALECLOUD]");
                }

                return "success";
            }
            catch (Exception ex)
            {
                Ultities.Telegram.pushNotify(string.Format("SendMail: request_id: {0},Error: {1}", subject, ex.ToString()), tele_group_id, tele_token);
                return "";
            }
        }

        //public static string getDataSalesCloudMethodGet(string endpoint, string _post)
        //{
        //    try
        //    {
        //        // CuongLv Update ssl 14-11-2018
        //        // Đoạn này dùng để chứng thực SSL   

        //        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        //        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

        //        string url = url_salecloud + endpoint; //"/api/editor/get-zone-map";               
        //        byte[] byteArray = Encoding.UTF8.GetBytes(_post);
        //        WebRequest request = WebRequest.Create(url);
        //        request.Credentials = new NetworkCredential(auth_username, auth_password);
        //        request.Method = "POST";
        //        request.ContentLength = byteArray.Length;
        //        request.ContentType = "application/x-www-form-urlencoded";
        //        Stream dataStream = request.GetRequestStream();
        //        dataStream.Write(byteArray, 0, byteArray.Length);
        //        dataStream.Close();
        //        WebResponse response = request.GetResponse();
        //        dataStream = response.GetResponseStream();

        //        StreamReader reader = new StreamReader(dataStream);
        //        string jsonContent = reader.ReadToEnd();
        //        // jsonContent = jsonContent.Replace("\"", "");

        //        return jsonContent;
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorWriter.WriteLog("D://", "getDataSalesCloud Error" + ex.ToString());
        //        return string.Empty;
        //    }
        //}

        // Gửi mail bên SC qua api
        public static bool getDataSalesCloudMethodPost(string endpoint, string _post)
        {
            try
            {

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;


                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                string contentType = "multipart/form-data; boundary=" + boundary;

                // Prepare the form data
                string formDataTemplate = "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n";
                StringBuilder formDataBuilder = new StringBuilder();
                formDataBuilder.AppendFormat(formDataTemplate, boundary, "param_submit_banner", _post);

                // If you want to add more fields, append them like this
                // formDataBuilder.AppendFormat(formDataTemplate, boundary, "param2", "value2");

                formDataBuilder.AppendFormat("--{0}--\r\n", boundary);

                byte[] byteArray = Encoding.UTF8.GetBytes(formDataBuilder.ToString());

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url_salecloud + endpoint);
                request.Credentials = new NetworkCredential(auth_username, auth_password);
                request.Method = "POST";
                request.ContentType = contentType;
                request.ContentLength = byteArray.Length;

                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }

                using (WebResponse response = request.GetResponse())
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(dataStream))
                        {
                            string responseContent = reader.ReadToEnd();

                            Ultities.Telegram.pushNotify("Send mail dfp response content: " + responseContent, tele_group_id, tele_token);
                            Console.WriteLine(responseContent);

                            return responseContent.ToLower().IndexOf("success") >= 0 ? true : false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Ultities.Telegram.pushNotify("Error job task queue crawl getDataSalesCloudMethodPost " + "-- error: " + ex.ToString(), tele_group_id, tele_token);
                ErrorWriter.WriteLog("D://", "getDataSalesCloud Error" + ex.ToString() + "---> endpoint = " + endpoint + " -- post = " + _post + " --- url_salecloud = " + url_salecloud);
                return false;
            }
        }


    }
}
