using AppAutoSubmitBannerDFP.Common;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;

namespace AppAutoSubmitBannerDFP.Model
{
    public static class ConnectApiSc
    {
        private static string auth_username = ConfigurationManager.AppSettings["auth_username"];
        private static string auth_password = ConfigurationManager.AppSettings["auth_password"];
        private static string url_salecloud = ConfigurationManager.AppSettings["url_salecloud"];
        public static string getDataSalesCloudMethodGet(string endpoint, string _post)
        {
            try
            {
                // CuongLv Update ssl 14-11-2018
                // Đoạn này dùng để chứng thực SSL   

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

                string url = url_salecloud + endpoint; //"/api/editor/get-zone-map";               
                byte[] byteArray = Encoding.UTF8.GetBytes(_post);
                WebRequest request = WebRequest.Create(url);
                request.Credentials = new NetworkCredential(auth_username, auth_password);
                request.Method = "POST";
                request.ContentLength = byteArray.Length;
                request.ContentType = "application/x-www-form-urlencoded";
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                dataStream = response.GetResponseStream();

                StreamReader reader = new StreamReader(dataStream);
                string jsonContent = reader.ReadToEnd();
                // jsonContent = jsonContent.Replace("\"", "");

                return jsonContent;
            }
            catch (Exception ex)
            {
                ErrorWriter.WriteLog("D://", "getDataSalesCloud Error" + ex.ToString());
                return string.Empty;
            }
        }
    }
}
