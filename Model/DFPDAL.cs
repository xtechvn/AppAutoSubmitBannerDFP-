using AppAutoSubmitBannerDFP.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;

namespace DFPDataSetUpBannerConsumer.Lib
{
    public class DFPDAL
    {
        public static string LogPath = Assembly.GetEntryAssembly().Location.Substring(0, Assembly.GetEntryAssembly().Location.LastIndexOf("\\") + 1);
        static string ConnectionMySQL = ConfigurationManager.AppSettings.Get("ConnectionMySQLString");
        static int app_scl = Convert.ToInt32(ConfigurationManager.AppSettings["app_scl"]);
        public static string username = ConfigurationManager.AppSettings["auth_username"];
        public static string password = ConfigurationManager.AppSettings["auth_password"];
        public static string domain_salescloud = ConfigurationManager.AppSettings["domain_salescloud"];
        public DataSet GetData(int ProductId)
        {
            string _post = string.Empty;
            try
            {
                if (app_scl == 1)
                {
                    var dataSet = new DataSet();
                    //using (MySqlConnection conn = new MySqlConnection(ConnectionMySQL))
                    //{
                    //    conn.Open();
                    //    MySqlCommand sqlCommand = new MySqlCommand("sp_GetDFPDetailForSetUp", conn);
                    //    sqlCommand.Parameters.AddWithValue("@p_product_id", ProductId);
                    //    sqlCommand.CommandType = CommandType.StoredProcedure;
                    //    MySqlDataAdapter Adapter = new MySqlDataAdapter(sqlCommand);
                    //    Adapter.Fill(dataSet);
                    //    conn.Close();
                    //    conn.Dispose();
                    //    Adapter.Dispose();
                    //}
                    //return dataSet; 
                    string url = domain_salescloud + "/api/dfp-services/get-dfp-detail-for-set-up?product_id=" + ProductId.ToString();

                    byte[] byteArray = Encoding.UTF8.GetBytes(_post);
                    WebRequest request = WebRequest.Create(url);

                    string authInfo = $"{username}:{password}";
                    authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                    request.Headers["Authorization"] = $"Basic {authInfo}";

                    request.Method = "POST";
                    request.ContentLength = byteArray.Length;
                    request.ContentType = "application/x-www-form-urlencoded";
                    Stream dataStream = request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                    WebResponse response = request.GetResponse();
                    dataStream = response.GetResponseStream();

                    StreamReader reader = new StreamReader(dataStream);
                    var jsonContent = reader.ReadToEnd();
                  //  var JsonParr = JArray.Parse("[" + jsonContent + "]");
                   // string string_data_json = JsonParr[0]["Data"].ToString();
                   // var data_json = JArray.Parse("[" + string_data_json + "]");


                    // Parse JSON to a JObject
                    JObject jsonObj = JObject.Parse(jsonContent);

                    // Extract the 'data' array
                    JArray data_banner = (JArray)jsonObj["data"]["banner"];
                    JArray data_boooking = (JArray)jsonObj["data"]["booking"];
                    JArray data_location = (JArray)jsonObj["data"]["location"];
                    JArray data_age = (JArray)jsonObj["data"]["age"];


                    // Convert JSON array to DataTable
                    DataTable dt_banner = ConvertJsonToDataTable(data_banner);
                    dataSet.Tables.Add(dt_banner);

                    DataTable dt_booking = ConvertJsonToDataTable(data_boooking);
                    dataSet.Tables.Add(dt_booking);

                    DataTable dt_location = ConvertJsonToDataTable(data_location);
                    dataSet.Tables.Add(dt_location);

                    DataTable dt_age = ConvertJsonToDataTable(data_age);
                    dataSet.Tables.Add(dt_age);



                    return dataSet;
                    // Display the DataTable (for testing purposes)
                    //foreach (DataRow row in dataTable.Rows)
                    //{
                    //    foreach (DataColumn col in dataTable.Columns)
                    //    {
                    //        Console.Write($"{col.ColumnName}: {row[col]}, ");
                    //    }
                    //    Console.WriteLine();
                    //}


                }
                else
                {
                    SqlParameter[] objParam = new SqlParameter[1];
                    objParam[0] = new SqlParameter("@ProductId", ProductId);
                    DataSet ds = new DataSet();
                    ConnectSQL.Fill(ds, "sp_GetDFPDetailForSetUp", objParam);
                    return ds;
                }
            }
            catch (Exception ex)
            {
                ErrorWriter.WriteLog(LogPath, "GetData:" + ex.ToString());
            }
            return null;
        }


        public DataTable ConvertJsonToDataTable(JArray jsonArray)
        {
            DataTable dataTable = new DataTable();

            if (jsonArray.Count > 0)
            {
                // Tạo cột dựa trên các khóa của JSON
                foreach (JProperty prop in jsonArray[0].ToObject<JObject>().Properties())
                {
                    dataTable.Columns.Add(prop.Name, typeof(string)); // Giả sử tất cả các giá trị đều là chuỗi
                }

                // Thêm hàng
                foreach (JObject obj in jsonArray)
                {
                    DataRow row = dataTable.NewRow();
                    foreach (JProperty prop in obj.Properties())
                    {
                        row[prop.Name] = prop.Value.ToString();
                    }
                    dataTable.Rows.Add(row);
                }
            }

            return dataTable;
        }


    }


}
