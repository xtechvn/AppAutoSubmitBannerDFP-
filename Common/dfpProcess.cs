using DFPDataSetUpBannerConsumer.Lib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace AppAutoSubmitBannerDFP.Common
{
    public class dfpProcess
    {

        private static string EncryptApi = WebConfigurationManager.AppSettings["ApiClient"]; // Đây là key để decode ra chuỗi json
        public static string LogPath = Assembly.GetEntryAssembly().Location.Substring(0, Assembly.GetEntryAssembly().Location.LastIndexOf("\\") + 1);
        public static string Environment = ConfigurationManager.AppSettings["Environment"];
        private static string SecondaryTrafficker = WebConfigurationManager.AppSettings["SecondaryTrafficker"];
        public static string tele_token = ConfigurationManager.AppSettings["tele_token"];
        public static string tele_group_id = ConfigurationManager.AppSettings["tele_group_id"];
        public static string location_mien_bac = ConfigurationManager.AppSettings["location_mien_bac"];
        public static string location_mien_trung = ConfigurationManager.AppSettings["location_mien_trung"];
        public static string location_mien_nam = ConfigurationManager.AppSettings["location_mien_nam"];
        public static string username = ConfigurationManager.AppSettings["auth_username"];
        public static string password = ConfigurationManager.AppSettings["auth_password"];

        /// <summary>
        /// Hàm chính: Lấy dữ liệu DFP theo RequestId và xây dựng DFPModel
        /// </summary>
        /// <param name="request_id">ID của request cần lấy dữ liệu</param>
        /// <returns>DFPModel chứa đầy đủ thông tin Order và LineItems</returns>
        public DFPModel GetDataByRequestId(int request_id)
        {
            string _post = string.Empty;
            try
            {
                // Khởi tạo DFPModel và gán RequestId
                DFPModel objDfpModel = new DFPModel();
                objDfpModel.RequestId = request_id;

                // Gọi API để lấy dữ liệu JSON
                var data_json = CallApiGetData(request_id, out _post);
                if (data_json == null || data_json.Count == 0)
                {
                    return null;
                }

                // Parse dữ liệu Order từ JSON
                string campiagn = "";
                ParseOrderData(data_json, objDfpModel, out campiagn);

                // Xây dựng LineItems từ dữ liệu database và JSON
                BuildLineItems(data_json, objDfpModel, campiagn);

                return objDfpModel;
            }
            catch (Exception ex)
            {
                ErrorWriter.WriteLog(LogPath, "RequestId: ", " #$# Token: " + _post + " #$# Error: " + ex.ToString());
                // TelegramWriter.WriteMessage("RequestId: " + message + " #$# Token: " + _post + " #$# Error: " + ex.ToString(), tele_group_id, tele_token);
                return null;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Gọi API để lấy dữ liệu JSON từ SalesCloud
        /// </summary>
        /// <param name="request_id">ID của request</param>
        /// <param name="postData">Chuỗi POST data đã gửi (để log lỗi)</param>
        /// <returns>JArray chứa dữ liệu JSON từ API</returns>
        private JArray CallApiGetData(int request_id, out string postData)
        {
            postData = string.Empty;
            try
            {
                // Tạo token mã hóa để gửi lên API
                string Token = Crypt_Xor.Encode(JsonConvert.SerializeObject(new { sKey = "8bce76a0883a14a8b6d3d574c95d57a1", RequestID = request_id }), EncryptApi);
                postData = "token=" + Token;
                string url = "https://salescloud.fptonline.net/api/index/get-data-implement-banner";

                // Tạo HTTP request với Basic Authentication
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                WebRequest request = WebRequest.Create(url);

                string authInfo = $"{username}:{password}";
                authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                request.Headers["Authorization"] = $"Basic {authInfo}";

                request.Method = "POST";
                request.ContentLength = byteArray.Length;
                request.ContentType = "application/x-www-form-urlencoded";

                // Gửi request và nhận response
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                WebResponse response = request.GetResponse();
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string jsonContent = reader.ReadToEnd();

                // Parse JSON response
                var JsonParr = JArray.Parse("[" + jsonContent + "]");
                string string_data_json = JsonParr[0]["Data"].ToString();
                return JArray.Parse("[" + string_data_json + "]");
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Parse dữ liệu Order từ JSON và gán vào DFPModel
        /// </summary>
        /// <param name="data_json">Dữ liệu JSON từ API</param>
        /// <param name="objDfpModel">DFPModel cần gán dữ liệu</param>
        /// <param name="campaign">Tên chiến dịch (output)</param>
        private void ParseOrderData(JArray data_json, DFPModel objDfpModel, out string campaign)
        {
            campaign = "";
            OrderModel order = new OrderModel();

            // Khởi tạo giá trị mặc định cho Order
            order.order_default = 0;
            order.secondary_trafficker = SecondaryTrafficker;
            order.name = string.Empty;
            order.order_url = "";

            // Lấy thông tin advertiser (tên khách hàng)
            order.advertiser = data_json[0]["ten_khach_hang"].ToString().Trim();

            // Lấy ID article
            string id_article = data_json[0]["id_article"].ToString().Trim();

            // Parse thông tin trafficker (nhân viên DFP nhận triển khai)
            string json_trafficker = data_json[0]["nhan_vien_dfp_nhan_trien_khai"].ToString().Trim();
            var data_trafficker = JArray.Parse(json_trafficker);
            if (data_trafficker.Count > 0)
            {
                order.trafficker = data_trafficker[0]["email"].ToString().Replace("@fpt.com.vn", "").Replace("@fpt.com", "");
            }
            else
            {
                order.trafficker = "";
            }

            // Lấy tên chiến dịch
            campaign = data_json[0]["ten_chien_dich"].ToString().Trim();

            // Parse thông tin sales care (nhân viên chăm sóc khách hàng)
            string json_sales_care = data_json[0]["sales_care"].ToString().Trim();
            var sales_care = JArray.Parse(json_sales_care);
            int index = 1;
            order.sales_person = "";
            order.secondary_salespeople = "";

            foreach (var sale in sales_care)
            {
                string email = sale["email"].ToString().Replace("@fpt.com.vn", "").Replace("@fpt.com", "").Trim();
                if (index == 1)
                {
                    // Người đầu tiên là sales person chính
                    order.sales_person = email;
                }
                else
                {
                    // Các người sau là secondary salespeople, phân cách bằng dấu ;
                    if (order.secondary_salespeople != "") order.secondary_salespeople += ";";
                    order.secondary_salespeople += email;
                }
                index++;
            }

            objDfpModel.order = order;
        }

        /// <summary>
        /// Xây dựng danh sách LineItems từ dữ liệu database và JSON
        /// </summary>
        /// <param name="data_json">Dữ liệu JSON từ API</param>
        /// <param name="objDfpModel">DFPModel cần gán dữ liệu</param>
        /// <param name="campaign">Tên chiến dịch</param>
        private void BuildLineItems(JArray data_json, DFPModel objDfpModel, string campaign)
        {
            // Lấy ProductId từ JSON
            int ProductId = Convert.ToInt32(data_json[0]["product_id"]);
            objDfpModel.ProductId = ProductId;

            // Lấy dữ liệu từ database
            DFPDAL dfpDal = new DFPDAL();
            DataSet ds = dfpDal.GetData(ProductId);

            if (ds == null || ds.Tables.Count == 0)
            {
                return;
            }

            // Khởi tạo DataTable để lưu thông tin AdUnits
            DataTable obj_dt_addUnit = InitializeDataTable();

            // Load dữ liệu từ DataSet vào các biến và DataTable
            string location_full = "";
            string location = "";
            List<LocationModel> lstLocation = new List<LocationModel>();
            List<string> lstAge = new List<string>();
            string slot = "";
            int goal = -1;
            string position_lite = "";
            string Platform = "";
            int MngPositionId = 0;
            DataRow row = null;

            LoadDataFromDataSet(ds, obj_dt_addUnit, ref lstAge, ref lstLocation, ref location,
                ref slot, ref goal, ref position_lite, ref Platform, ref MngPositionId, ref row, ref location_full);

            if (row == null)
            {
                return;
            }

            // Lấy thông tin từ row đầu tiên
            string landingpage = row["LandingPage"].ToString();
            string index_industrial = row["Industry"].ToString();
            string index_brand = row["Label"].ToString();
            string deliver_time = row["DeliverTime"].ToString();
            string id_article = data_json[0]["id_article"].ToString().Trim();
            int ProductType = Convert.ToInt32(row["ProductType"]);
            int AdTypeID = Convert.ToInt32(row["AdTypeID"]);

            // Cập nhật thông tin Order từ database
            objDfpModel.order.order_url = row["OrderURL"].ToString();
            objDfpModel.order.sync = Convert.ToInt32(row["Sync"]);
            objDfpModel.order.order_default = Convert.ToInt32(row["Default"]);
            objDfpModel.order.line_item = new List<LineItemModel>();

            // Parse danh sách landing pages từ JSON
            List<string> lstLandingpage = ParseLandingPages(data_json, landingpage);

            // Xây dựng danh sách Creatives từ JSON
            List<CreativeModel> lstCreative = BuildCreativesFromJson(data_json, index_industrial, index_brand, campaign, lstLandingpage);

            // Lấy danh sách Positions đã được group
            var lstPosition = GetGroupedPositions(obj_dt_addUnit);

            // Kiểm tra xem có phải Inread không (dựa vào PositionName và Creative size)
            bool Inread = CheckInread(lstPosition, lstCreative);

            // Xây dựng từng LineItem
            int Index_LineItem = System.Linq.Enumerable.Count(lstPosition);
            int lineItemIndex = 0; // Đếm số LineItem thực sự được tạo
            for (int i = 1; i <= Index_LineItem; i++)
            {
                // Bỏ qua nếu là Inread và không phải IP12
                if (Inread && lstPosition[i - 1].PositionName != "IP12")
                {
                    continue;
                }

                lineItemIndex++; // Tăng index cho LineItem thực sự được tạo

                // Xây dựng một LineItem
                LineItemModel line_item = BuildSingleLineItem(
                    row, lstPosition[i - 1], obj_dt_addUnit, ds, lstLocation, location_full, location,
                    lstAge, id_article, deliver_time, ProductType, AdTypeID, goal, slot,
                    position_lite, ProductId, objDfpModel.RequestId, campaign, Inread, objDfpModel.order.order_default, lineItemIndex);

                // Xử lý Creatives cho LineItem này
                ProcessCreativesForLineItem(line_item, lstPosition[i - 1], obj_dt_addUnit, lstCreative, Inread, ProductType, AdTypeID);

                // cuonglv update 01-07-2026: neu ma khong co componane size thi bo qua vi no khong map voi file thiet ke
                if (line_item.companion_sizes.Count == 0) continue; 

                // Thêm LineItem vào danh sách
                objDfpModel.order.line_item.Add(line_item);

                // Cập nhật tên Order từ LineItem đầu tiên
                if (lineItemIndex == 1)
                {
                    objDfpModel.order.name = line_item.name;
                }

                // Cập nhật tên LineItem nếu có nhiều hơn 1 LineItem
                if (Index_LineItem > 1)
                {
                    string positionName = Inread ? lstPosition[i - 1].PositionName.Replace("[", "").Replace("]", "").Replace("IP12", "Inread")
                        : lstPosition[i - 1].PositionName.Replace("[", "").Replace("]", "");
                    line_item.name = line_item.name + "(" + positionName + ")";
                }
            }

            // Thêm các Creatives chưa sử dụng vào các LineItems
            AddUnusedCreativesToLineItems(objDfpModel.order.line_item, lstCreative);
        }

        /// <summary>
        /// Khởi tạo DataTable với các cột cần thiết để lưu thông tin AdUnits
        /// </summary>
        /// <returns>DataTable đã được khởi tạo với đầy đủ cột</returns>
        private DataTable InitializeDataTable()
        {
            var obj_dt_addUnit = new DataTable();
            obj_dt_addUnit.Columns.Add(new DataColumn("Id", typeof(int)));
            obj_dt_addUnit.Columns.Add(new DataColumn("BookingId", typeof(int)));
            obj_dt_addUnit.Columns.Add(new DataColumn("BookingName"));
            obj_dt_addUnit.Columns.Add(new DataColumn("FromDate"));
            obj_dt_addUnit.Columns.Add(new DataColumn("ToDate"));
            obj_dt_addUnit.Columns.Add(new DataColumn("SiteName"));
            obj_dt_addUnit.Columns.Add(new DataColumn("MngPositionId", typeof(int)));
            obj_dt_addUnit.Columns.Add(new DataColumn("PositionName"));
            obj_dt_addUnit.Columns.Add(new DataColumn("CategoryName"));
            obj_dt_addUnit.Columns.Add(new DataColumn("Platform"));
            obj_dt_addUnit.Columns.Add(new DataColumn("PositionLite"));
            obj_dt_addUnit.Columns.Add(new DataColumn("Ads_Size"));
            obj_dt_addUnit.Columns.Add(new DataColumn("CreativeType"));
            obj_dt_addUnit.Columns.Add(new DataColumn("Size"));
            obj_dt_addUnit.Columns.Add(new DataColumn("Include", typeof(int)));
            obj_dt_addUnit.Columns.Add(new DataColumn("SetLineItem", typeof(int)));
            obj_dt_addUnit.Columns.Add(new DataColumn("PercentQuantiy", typeof(int)));
            obj_dt_addUnit.Columns.Add(new DataColumn("IsPlacement", typeof(int)));
            obj_dt_addUnit.Columns.Add(new DataColumn("OverlayCard", typeof(int)));
            obj_dt_addUnit.Columns.Add(new DataColumn("Slot"));
            obj_dt_addUnit.Columns.Add(new DataColumn("Goal", typeof(int)));
            obj_dt_addUnit.Columns.Add(new DataColumn("CategoryId"));
            obj_dt_addUnit.Columns.Add(new DataColumn("InputSize"));
            obj_dt_addUnit.Columns.Add(new DataColumn("ItemType", typeof(int)));
            obj_dt_addUnit.Columns.Add(new DataColumn("SafeFrame", typeof(int)));
            obj_dt_addUnit.Columns.Add(new DataColumn("USD", typeof(int)));
            obj_dt_addUnit.Columns.Add(new DataColumn("ExCountry"));
            obj_dt_addUnit.Columns.Add(new DataColumn("AdType", typeof(int)));
            return obj_dt_addUnit;
        }

        /// <summary>
        /// Load dữ liệu từ DataSet vào các biến và DataTable
        /// </summary>
        private void LoadDataFromDataSet(DataSet ds, DataTable obj_dt_addUnit, ref List<string> lstAge,
            ref List<LocationModel> lstLocation, ref string location, ref string slot, ref int goal,
            ref string position_lite, ref string Platform, ref int MngPositionId, ref DataRow row, ref string location_full)
        {
            // Load dữ liệu Age từ Table[3]
            if (ds.Tables.Count > 3 && ds.Tables[3].Rows.Count > 0)
            {
                foreach (DataRow rowAge in ds.Tables[3].Rows)
                {
                    lstAge.Add(rowAge["Age"].ToString());
                }
            }

            // Load dữ liệu Location từ Table[2]
            if (ds.Tables.Count > 2 && ds.Tables[2].Rows.Count > 0)
            {
                foreach (DataRow rowLocal in ds.Tables[2].Rows)
                {
                    if (location != "") location = location + ",";
                    location = location + rowLocal["LocationId"].ToString();
                    lstLocation.Add(new LocationModel(Convert.ToInt32(rowLocal["LocationId"]),
                        rowLocal["LocationName"].ToString(), 0, Convert.ToInt32(rowLocal["Include"])));
                }
            }

            #region Load dữ liệu Booking từ Table[1] vào DataTable
            if (ds.Tables.Count > 1)
            {
                // Đếm số row có CategoryName chứa "gr"
                int grRowCount = ds.Tables[1].AsEnumerable()
                    .Count(r => (r["CategoryName"] ?? string.Empty).ToString().ToLowerInvariant().Contains("gr"));
                
                // Kiểm tra xem các row có "gr" có cùng PositionName không
                bool hasSamePositionName = false;
                if (grRowCount > 1)
                {
                    var grRows = ds.Tables[1].AsEnumerable()
                        .Where(r => (r["CategoryName"] ?? string.Empty).ToString().ToLowerInvariant().Contains("gr"))
                        .Select(r => r["PositionName"]?.ToString() ?? string.Empty)
                        .Distinct()
                        .ToList();
                    hasSamePositionName = grRows.Count == 1; // Tất cả PositionName phải giống nhau
                }
                
                // Chỉ áp dụng logic đặc biệt khi có nhiều hơn 1 row "gr" VÀ có cùng PositionName
                bool hasMultipleGr = grRowCount > 1 && hasSamePositionName;
                bool firstGrAssigned = false;
                foreach (DataRow dr in ds.Tables[1].Rows)
                {
                    DataRow drow = obj_dt_addUnit.NewRow();
                    int bookingId = Convert.ToInt32(dr["BookingId"]);
                    string categoryName = dr["CategoryName"].ToString();
                    string categoryKey = categoryName.ToLowerInvariant();
                    bool categoryHasGr = categoryKey.Contains("gr");

                    drow["Id"] = Convert.ToInt32(dr["Id"]);
                    drow["BookingId"] = bookingId;
                    drow["BookingName"] = dr["BookingName"].ToString();
                    drow["FromDate"] = dr["FromDate"];
                    drow["ToDate"] = dr["ToDate"];
                    drow["SiteName"] = dr["SiteName"].ToString();
                    drow["MngPositionId"] = Convert.ToInt32(dr["MngPositionId"]);
                    drow["PositionName"] = dr["PositionName"].ToString();
                    drow["CategoryName"] = categoryName;
                    drow["Platform"] = dr["Platform"].ToString();
                    drow["PositionLite"] = dr["PositionLite"].ToString();
                    drow["Ads_Size"] = dr["Ads_Size"].ToString();
                    drow["CreativeType"] = dr["CreativeType"].ToString();
                    drow["Size"] = dr["Size"].ToString();
                    drow["Include"] = Convert.ToInt32(dr["Include"]);
                    if (categoryHasGr && hasMultipleGr)
                    {
                        drow["SetLineItem"] = firstGrAssigned ? 0 : 1;
                        if (!firstGrAssigned)
                        {
                            firstGrAssigned = true;
                        }
                    }
                    else
                    {
                        drow["SetLineItem"] = Convert.ToInt32(dr["SetLineItem"]);
                    }
                    //drow["SetLineItem"] = Convert.ToInt32(dr["SetLineItem"]);
                    drow["PercentQuantiy"] = Convert.ToInt32(dr["PercentQuantiy"]);
                    drow["IsPlacement"] = Convert.ToInt32(dr["IsPlacement"]);
                    drow["OverlayCard"] = Convert.ToInt32(dr["OverlayCard"]);
                    drow["Slot"] = dr["Slot"].ToString();
                    drow["Goal"] = Convert.ToInt32(dr["Goal"]);
                    drow["CategoryId"] = dr["CategoryId"].ToString();
                    drow["InputSize"] = dr["InputSize"].ToString();
                    drow["ItemType"] = Convert.ToInt32(dr["ItemType"]);
                    drow["SafeFrame"] = Convert.ToInt32(dr["SafeFrame"]);
                    drow["USD"] = Convert.ToInt32(dr["USD"]);
                    drow["ExCountry"] = dr["ExCountry"].ToString();
                    // quyet dinh viec chon MasterCompose khong ?  = 2 la chon
                    drow["AdType"] = dr.Table.Columns.Contains("AdType") ? Convert.ToInt32(dr["AdType"]) : 2;
                    obj_dt_addUnit.Rows.Add(drow);
                }

                // Lấy thông tin từ row đầu tiên của Table[1]
                if (ds.Tables[1].Rows.Count > 0)
                {
                    MngPositionId = Convert.ToInt32(ds.Tables[1].Rows[0]["MngPositionId"]);
                    Platform = ds.Tables[1].Rows[0]["Platform"].ToString();
                    position_lite = ds.Tables[1].Rows[0]["PositionLite"].ToString();
                    slot = ds.Tables[1].Rows[0]["Slot"].ToString();
                    goal = Convert.ToInt32(ds.Tables[1].Rows[0]["Goal"]);
                }
            }
            #endregion

            // Load dữ liệu chính từ Table[0]
            if (ds.Tables[0].Rows.Count > 0)
            {
                row = ds.Tables[0].Rows[0];
                location_full = row["LocationFull"].ToString();
            }
        }

        /// <summary>
        /// Parse danh sách landing pages từ JSON
        /// </summary>
        private List<string> ParseLandingPages(JArray data_json, string defaultLandingPage)
        {
            List<string> lstLandingpage = new List<string>();
            string json_langding_page = data_json[0]["url_landing_page"].ToString();
            var data_langding_page = JArray.Parse(json_langding_page);

            if (data_langding_page.Count > 0)
            {
                for (int n = 0; n <= data_langding_page.Count - 1; n++)
                {
                    lstLandingpage.Add(data_langding_page[n]["TrackingUrl"].ToString().Trim());
                }
            }
            else
            {
                lstLandingpage.Add(defaultLandingPage);
            }
            return lstLandingpage;
        }

        /// <summary>
        /// Xây dựng danh sách Creatives từ JSON data
        /// </summary>
        private List<CreativeModel> BuildCreativesFromJson(JArray data_json, string index_industrial,
            string index_brand, string campaign, List<string> lstLandingpage)
        {
            List<CreativeModel> lstCreative = new List<CreativeModel>();
            string json_html_banner = data_json[0]["ket_qua_san_xuat"].ToString();
            var data_html_banner = JArray.Parse(json_html_banner);

            ErrorWriter.WriteLog(LogPath, " data_html_banner: " + JsonConvert.SerializeObject(data_html_banner));

            int creative_id = 0;
            foreach (var banner in data_html_banner)
            {
                CreativeModel creative = new CreativeModel();
                creative_id++;
                creative.third_party_tracking = new List<string>();

                // Parse third party tracking URLs
                string json_third_party_tracking = data_json[0]["third_party_tracking"].ToString().Trim();
                var data_third_party_tracking = JArray.Parse(json_third_party_tracking);
                if (data_third_party_tracking.Count > 0)
                {
                    foreach (var obj_tracking in data_third_party_tracking)
                    {
                        creative.third_party_tracking.Add(obj_tracking["TrackingUrl"].ToString().Trim());
                    }
                }

                // Gán các thuộc tính cho Creative
                creative.Id = creative_id;
                creative.name = "Banner_" + banner["size"].ToString().Trim();
                creative.iframe_url = banner["link"].ToString().Trim();
                creative.ads_size = banner["size"].ToString().Trim();
                creative.index_industrial = index_industrial;
                creative.index_brand = index_brand;
                creative.campaign_name = campaign;
                creative.creative_type = 1;
                creative.creative_template = creative_template(creative.iframe_url);
                creative.creative_url = "";
                creative.overlay_card = 1;
                creative.run_mode = creative_runmode(creative.iframe_url);
                creative.index = 0;
                creative.data_creative_id = 0;
                creative.click_through_url = GetLandingpage(lstLandingpage, creative.ads_size);
                creative.safe_frame = 0;
                lstCreative.Add(creative);
            }

            ErrorWriter.WriteLog(LogPath, " data_html_banner_02: " + JsonConvert.SerializeObject(lstCreative));
            return lstCreative;
        }

        /// <summary>
        /// Lấy danh sách Positions đã được group theo các tiêu chí
        /// </summary>
        private dynamic GetGroupedPositions(DataTable obj_dt_addUnit)
        {
            var lstPosition = (from p in obj_dt_addUnit.AsEnumerable()
                               where p.Field<int>("SetLineItem") == 1
                               group p by new
                               {
                                   Platform = p.Field<string>("Platform"),
                                   PositionName = p.Field<string>("PositionName"),
                                   Ad_Size = p.Field<string>("Ads_Size"),
                                   Creative_Size = p.Field<string>("Size"),
                                   Percent = p.Field<int>("PercentQuantiy"),
                                   CreativeType = p.Field<string>("CreativeType"),
                                   IsPlacement = p.Field<int>("IsPlacement"),
                                   Overlay_Card = p.Field<int>("OverlayCard"),
                                   CategoryId = p.Field<string>("CategoryId"),
                                   CategoryName = p.Field<string>("CategoryName"),
                                   Input_Size = p.Field<string>("InputSize"),
                                   Item_Type = p.Field<int>("ItemType"),
                                   Safe_Frame = p.Field<int>("SafeFrame"),
                                   USD = p.Field<int>("USD")
                               } into g
                               select new
                               {
                                   Platform = g.Key.Platform,
                                   PositionName = g.Key.PositionName,
                                   Ads_Size = g.Key.Ad_Size,
                                   Creative_Size = g.Key.Creative_Size,
                                   Percent = g.Key.Percent,
                                   Creative_Type = g.Key.CreativeType,
                                   IsPlacement = g.Key.IsPlacement,
                                   Overlay_Card = g.Key.Overlay_Card,
                                   CategoryId = g.Key.CategoryId,
                                   CategoryName = g.Key.CategoryName,
                                   Input_Size = g.Key.Input_Size,
                                   Item_Type = g.Key.Item_Type,
                                   Safe_Frame = g.Key.Safe_Frame,
                                   USD = g.Key.USD
                               }).ToList();

            ErrorWriter.WriteLog(LogPath, " data_html_banner_04: " + JsonConvert.SerializeObject(lstPosition));
            return lstPosition;
        }

        /// <summary>
        /// Kiểm tra xem có phải Inread không (dựa vào PositionName và Creative size)
        /// </summary>
        private bool CheckInread(dynamic lstPosition, List<CreativeModel> lstCreative)
        {
            bool Inread = false;
            int Index_LineItem = System.Linq.Enumerable.Count(lstPosition);
            if (Index_LineItem > 0 && (lstPosition[0].PositionName == "IP12" || lstPosition[0].PositionName == "IPHome"))
            {
                foreach (CreativeModel objTemp in lstCreative)
                {
                    if (objTemp.ads_size == "320x640")
                    {
                        Inread = true;
                        break;
                    }
                }
            }
            return Inread;
        }

        /// <summary>
        /// Xây dựng một LineItem cụ thể từ dữ liệu
        /// </summary>
        private LineItemModel BuildSingleLineItem(DataRow row, dynamic position, DataTable obj_dt_addUnit,
            DataSet ds, List<LocationModel> lstLocation, string location_full, string location,
            List<string> lstAge, string id_article, string deliver_time, int ProductType, int AdTypeID,
            int goal, string slot, string position_lite, int ProductId, int request_id, string campaign,
            bool Inread, int order_default, int lineItemIndex)
        {
            LineItemModel line_item = new LineItemModel();
            line_item.creative = new List<CreativeModel>();
            line_item.companion_sizes = new List<string>();

            // Thiết lập thời gian giao hàng
            string start_deliver_time, end_deliver_time;
            GetDeliverTime(deliver_time, out start_deliver_time, out end_deliver_time);
            line_item.start_deliver_time = start_deliver_time;
            line_item.end_deliver_time = end_deliver_time;

            // Thiết lập targeting (age, gender, audience, device, article, tag, brandsafe)
            line_item.age = GetDeliverAge(row["Age"].ToString(), lstAge);
            line_item.gender = GetDeliverGender(row["Gender"].ToString());
            line_item.audience = GetDeliverAudience(row["Audience"].ToString());
            line_item.device = GetDeliverDevice(row["Device"].ToString());
            line_item.article = GetArticle(row["Article"].ToString(), id_article);
            line_item.tag = GetTag(row["Tag"].ToString());
            line_item.brandsafe = GetBrandsafe(row["BrandSafe"].ToString());
            line_item.categoryid = GetCategoryId(position.CategoryId);
            line_item.usd = position.USD;
            line_item.vat = Convert.ToInt32(row["VAT"].ToString());

            // Thiết lập location nếu có
            if (location_full != location && lstLocation.Count > 0)
            {
                line_item.location = GetListLocation(lstLocation);
            }

            // Thiết lập các giá trị cơ bản
            line_item.goal = goal;
            line_item.item_type = 2;
            line_item.percentage = 2;
            line_item.display_creative = 1;
            line_item.category_name = position.CategoryName;
            line_item.position_name = position.PositionName;

            // Thiết lập line_item_type và ad_type dựa vào ProductType và AdTypeID
            SetLineItemTypeAndAdType(line_item, ProductType, AdTypeID, order_default);

            // Thiết lập thời gian bắt đầu và kết thúc 
            if (Environment == "Dev")
            {
                line_item.start_time = Convert.ToDateTime(row["FromDate"]).AddYears(1);
                line_item.end_time = Convert.ToDateTime(row["ToDate"]).AddYears(1);
            }
            else
            {
                line_item.start_time = Convert.ToDateTime(row["FromDate"]);
                line_item.end_time = Convert.ToDateTime(row["ToDate"]);
            }

            // Gán item_type từ position
            line_item.item_type = position.Item_Type;

            // Tính toán quantity
            CalculateLineItemQuantity(line_item, row, position, ProductType, AdTypeID, Inread);

            // Thiết lập unit và frequency
            line_item.unit = Convert.ToInt32(row["Unit"]);
            line_item.frequency = Convert.ToInt32(row["Frequency"]);

            // Tính toán rate và discount
            CalculateLineItemRate(line_item, row, position, ProductType, AdTypeID, lineItemIndex);

            // Chuyển đổi rate sang USD nếu cần
            if (line_item.usd == 2)
                line_item.rate = Math.Round(line_item.rate * 1.1 / 23000, 3);

            // Xử lý discount nếu không phải số nguyên
            if (!Common.IsInteger(line_item.discount))
            {
                line_item.percentage = 1;
                line_item.discount = (Convert.ToInt64(row["Price"]) - Convert.ToInt64(row["Amount"])) * position.Percent / 100;
            }

            // Lấy data_line_item_id
            line_item.data_line_item_id = GetDataId(obj_dt_addUnit, 1, position.PositionName, position.Ads_Size,
                position.Creative_Size, position.Creative_Type, position.IsPlacement, position.Overlay_Card);

            // Thiết lập add_sizes
            string[] separatingStrings = { "#$#" };
            string ads_size_v2 = position.Ads_Size.ToString();
            string[] arrAdSize = ads_size_v2.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);
            List<string> lst_ads_size = new List<string>();
            foreach (string adsize in arrAdSize)
            {
                lst_ads_size.Add(adsize);
            }
            line_item.add_sizes = lst_ads_size.Count > 0 ? lst_ads_size[0] : "";

            // Thiết lập custom_targeting
            line_item.custom_targeting = (line_item.line_item_type == 1) ? slot : "";

            // Thiết lập unit_type
            line_item.unit_type = 1;

            // Xây dựng AdUnits và Placements
            BuildAdUnitsAndPlacements(line_item, position, ds, Inread);

            // Tạo tên cho LineItem
            string contractno = row["ContractNo"].ToString().Trim();
            string customer_lite = row["CustomerLite"].ToString().Trim();
            string positionNameForName = Inread ? position_lite.Replace("IP3 Home", "IP IR").Replace("IP", "IP IR") : position_lite;
            string dateRange = line_item.start_time.ToString("ddMM") + "-" + line_item.end_time.ToString("ddMM");
            line_item.name = GetName(contractno, ProductId, request_id, customer_lite, campaign, positionNameForName, dateRange, line_item.discount);

            return line_item;
        }

        /// <summary>
        /// Thiết lập line_item_type và ad_type dựa vào ProductType và AdTypeID
        /// </summary>
        private void SetLineItemTypeAndAdType(LineItemModel line_item, int ProductType, int AdTypeID, int order_default)
        {
            line_item.ad_type = 1;
            if (ProductType == 17 && AdTypeID == 2) // DFP-CPD
            {
                line_item.line_item_type = 1;
                line_item.item_type = 1;
            }
            else if (ProductType == 17 && AdTypeID == 4) // DFP-CPM
            {                
                line_item.line_item_type = 2;
            }
            else if (ProductType == 11 && AdTypeID == 18) // DFP Preroll
            {             
                line_item.line_item_type = 2;
            }
            else if (ProductType == 11 && AdTypeID == 19) // DFP CPC
            {             
                line_item.line_item_type = 4;
            }
            else if (ProductType == 11 && AdTypeID == 30) // DFP Tối ưu
            {             
                line_item.line_item_type = 5;
                line_item.item_type = order_default == 1 ? 6 : 5;
            }
            else
            {             
                line_item.line_item_type = 6;
            }
        }

        /// <summary>
        /// Tính toán quantity cho LineItem
        /// </summary>
        private void CalculateLineItemQuantity(LineItemModel line_item, DataRow row, dynamic position,
            int ProductType, int AdTypeID, bool Inread)
        {
            if (line_item.item_type == 5) // Tối ưu || Nước ngoài
            {
                double count_date = (line_item.end_time.Date - line_item.start_time.Date).TotalDays + 1;
                if (count_date > 0)
                {
                    line_item.quantity = Convert.ToInt64(row["Quantity"]) * position.Percent / (100 * Convert.ToInt32(count_date));
                }
                else
                {
                    line_item.quantity = Convert.ToInt64(row["Quantity"]) * position.Percent / 100;
                }
            }
            else if (ProductType == 17 && AdTypeID == 4 && Inread == false) // DFP-CPM
            {
                line_item.quantity = Convert.ToInt64(row["Quantity"]) * position.Percent / 100;
            }
            else
            {
                line_item.quantity = Convert.ToInt64(row["Quantity"]);
            }
        }

        /// <summary>
        /// Tính toán rate và discount cho LineItem
        /// </summary>
        private void CalculateLineItemRate(LineItemModel line_item, DataRow row, dynamic position,
            int ProductType, int AdTypeID, int lineItemIndex)
        {
            if (lineItemIndex == 1) // LineItem đầu tiên
            {
                if (line_item.line_item_type == 1) // CPD
                {
                    line_item.rate = Convert.ToInt64(row["Price"]) / ((line_item.end_time.Date - line_item.start_time.Date).TotalDays + 1);
                    line_item.discount = Convert.ToDouble(row["Discount"]);
                }
                else
                {
                    if (line_item.quantity != 0)
                    {
                        if (ProductType == 17 && AdTypeID == 4) // CPM
                        {
                            line_item.rate = Convert.ToInt64(row["Price"]) * 1000 / Convert.ToInt64(row["Quantity"]);
                        }
                        else if (ProductType == 11 && AdTypeID == 30) // Tối ưu
                        {
                            line_item.rate = Convert.ToInt64(row["Price"]) * (line_item.unit == 2 ? 1 : 1000) / Convert.ToInt64(row["Quantity"]);
                        }
                        else
                        {
                            line_item.rate = Convert.ToInt64(row["Price"]) / line_item.quantity;
                        }
                        line_item.discount = Convert.ToDouble(row["Discount"]);
                    }
                    else
                    {
                        line_item.rate = 0;
                        line_item.discount = 0;
                    }
                }
            }
            else // Các LineItem sau
            {
                if (ProductType == 17 && AdTypeID == 4) // CPM
                {
                    line_item.rate = Convert.ToInt64(row["Price"]) * 1000 / Convert.ToInt64(row["Quantity"]);
                    line_item.discount = Convert.ToDouble(row["Discount"]);
                }
                else
                {
                    line_item.rate = 0;
                    line_item.discount = 0;
                }
            }
        }

        /// <summary>
        /// Xây dựng AdUnits và Placements cho LineItem
        /// </summary>
        private void BuildAdUnitsAndPlacements(LineItemModel line_item, dynamic position, DataSet ds, bool Inread)
        {
            if (position.IsPlacement == 0) // Sử dụng AdUnits
            {
                line_item.adunit = new List<AdUnitModel>();
                if (ds.Tables.Count > 1)
                {
                    foreach (DataRow rowBooking in ds.Tables[1].Rows)
                    {
                        if (rowBooking["PositionName"].ToString() == position.PositionName.ToString() &&
                            rowBooking["Platform"].ToString() == position.Platform.ToString())
                        {
                            AdUnitModel objAdUnit = new AdUnitModel();
                            if (rowBooking["PositionName"].ToString().IndexOf("[") >= 0)
                            {
                                objAdUnit.adunit = rowBooking["SiteName"].ToString() + ">" + rowBooking["Platform"].ToString() + ">" + rowBooking["CategoryName"].ToString();
                            }
                            else
                            {
                                objAdUnit.adunit = rowBooking["SiteName"].ToString() + ">" + rowBooking["Platform"].ToString() + ">" + rowBooking["PositionName"].ToString() + ">" + rowBooking["CategoryName"].ToString();
                            }
                            objAdUnit.include = Convert.ToInt32(rowBooking["Include"]);
                            line_item.adunit.Add(objAdUnit);
                        }
                    }
                }
                line_item.palcement = new List<PlacementModel>();
            }
            else // Sử dụng Placements
            {
                line_item.palcement = new List<PlacementModel>();
                if (ds.Tables.Count > 1)
                {
                    foreach (DataRow rowBooking in ds.Tables[1].Rows)
                    {
                        if (rowBooking["PositionName"].ToString() == position.PositionName.ToString() &&
                            rowBooking["Platform"].ToString() == position.Platform.ToString())
                        {
                            PlacementModel objPlacement = new PlacementModel();
                            string categoryName = rowBooking["CategoryName"].ToString();
                            if (Inread)
                            {
                                categoryName = categoryName.Replace("MB: Inpage Gr1: IP, IR, BP, incomment", "MB: Inread Gr1 (MB, App, AMP)")
                                    .Replace("MB: Inpage Gr2: IP, IR, BP, incomment", "MB: Inread Gr2 (MB, App, AMP)");
                            }
                            objPlacement.placement = categoryName;
                            line_item.palcement.Add(objPlacement);
                        }
                    }
                }
                line_item.adunit = new List<AdUnitModel>();
            }
        }

        /// <summary>
        /// Xử lý Creatives cho một LineItem
        /// </summary>
        private void ProcessCreativesForLineItem(LineItemModel line_item, dynamic position, DataTable obj_dt_addUnit,
            List<CreativeModel> lstCreative, bool Inread, int ProductType, int AdTypeID)
        {
            string creative_size_v2 = position.Creative_Size.ToString();
            string ads_size_v2 = position.Ads_Size.ToString();
            int safe_frame = position.Safe_Frame;

            // Tạo danh sách SizeModel cho Creatives
            List<SizeModel> lstSetCreative = new List<SizeModel>();
            lstSetCreative.Add(new SizeModel(line_item.data_line_item_id, position.PositionName, creative_size_v2,
                ads_size_v2, position.Creative_Type, position.IsPlacement, position.Overlay_Card, position.Input_Size));

      

            // Lấy các positions với SetLineItem = 0 (companion creatives)
            var lstPos = (from p in obj_dt_addUnit.AsEnumerable()
                          where p.Field<int>("SetLineItem") == 0
                          group p by new
                          {
                              Platform = p.Field<string>("Platform"),
                              PositionName = p.Field<string>("PositionName"),
                              Ad_Size = p.Field<string>("Ads_Size"),
                              Creative_Size = p.Field<string>("Size"),
                              Percent = p.Field<int>("PercentQuantiy"),
                              CreativeType = p.Field<string>("CreativeType"),
                              IsPlacement = p.Field<int>("IsPlacement"),
                              Overlay_Card = p.Field<int>("OverlayCard"),
                              CategoryId = p.Field<string>("CategoryId"),
                              Input_Size = p.Field<string>("InputSize"),
                              Item_Type = p.Field<int>("ItemType"),
                              Safe_Frame = p.Field<int>("SafeFrame"),
                              USD = p.Field<int>("USD")
                          } into g
                          select new
                          {
                              Platform = g.Key.Platform,
                              PositionName = g.Key.PositionName,
                              Ads_Size = g.Key.Ad_Size,
                              Creative_Size = g.Key.Creative_Size,
                              Percent = g.Key.Percent,
                              Creative_Type = g.Key.CreativeType,
                              IsPlacement = g.Key.IsPlacement,
                              Overlay_Card = g.Key.Overlay_Card,
                              CategoryId = g.Key.CategoryId,
                              Input_Size = g.Key.Input_Size,
                              Item_Type = g.Key.Item_Type,
                              Safe_Frame = g.Key.Safe_Frame,
                              USD = g.Key.USD
                          }).ToArray();


            //// Nếu ZoneId = 53531 hoặc 53532, chỉ lấy 1 creative đầu tiên
            //if (lstPos.Count() > 0 && lstPos[0].PositionName == "IP12_IPHome")
            //{
            //    lstPos = lstPos.Take(1).ToArray();
            //}

            int creative_sync = 0;
            if (lstPos.Count() > 0)
            {
                creative_sync = 1;
                line_item.display_creative = 2;
                line_item.ad_type = Convert.ToInt32(obj_dt_addUnit.Rows[0]["AdType"]);// (ProductType == 17 && AdTypeID == 4) ? 1 : 2;
                foreach (var pos in lstPos)
                {
                    var data_creative_id = GetDataId(obj_dt_addUnit, 0, pos.PositionName, pos.Ads_Size,
                        pos.Creative_Size, pos.Creative_Type, pos.IsPlacement, pos.Overlay_Card);
                    lstSetCreative.Add(new SizeModel(data_creative_id, pos.PositionName, pos.Creative_Size,
                        pos.Ads_Size, pos.Creative_Type, pos.IsPlacement, pos.Overlay_Card, pos.Input_Size));
                  
                }
            }
            else
            {
                // Lấy các positions với SetLineItem = 2
                lstPos = (from p in obj_dt_addUnit.AsEnumerable()
                          where p.Field<int>("SetLineItem") == 2
                          group p by new
                          {
                              Platform = p.Field<string>("Platform"),
                              PositionName = p.Field<string>("PositionName"),
                              Ad_Size = p.Field<string>("Ads_Size"),
                              Creative_Size = p.Field<string>("Size"),
                              Percent = p.Field<int>("PercentQuantiy"),
                              CreativeType = p.Field<string>("CreativeType"),
                              IsPlacement = p.Field<int>("IsPlacement"),
                              Overlay_Card = p.Field<int>("OverlayCard"),
                              CategoryId = p.Field<string>("CategoryId"),
                              Input_Size = p.Field<string>("InputSize"),
                              Item_Type = p.Field<int>("ItemType"),
                              Safe_Frame = p.Field<int>("SafeFrame"),
                              USD = p.Field<int>("USD")
                          } into g
                          select new
                          {
                              Platform = g.Key.Platform,
                              PositionName = g.Key.PositionName,
                              Ads_Size = g.Key.Ad_Size,
                              Creative_Size = g.Key.Creative_Size,
                              Percent = g.Key.Percent,
                              Creative_Type = g.Key.CreativeType,
                              IsPlacement = g.Key.IsPlacement,
                              Overlay_Card = g.Key.Overlay_Card,
                              CategoryId = g.Key.CategoryId,
                              Input_Size = g.Key.Input_Size,
                              Item_Type = g.Key.Item_Type,
                              Safe_Frame = g.Key.Safe_Frame,
                              USD = g.Key.USD
                          }).ToArray();

                if (lstPos.Count() > 0)
                {
                    foreach (var pos2 in lstPos)
                    {
                        var data_creative_id_2 = GetDataId(obj_dt_addUnit, 2, pos2.PositionName, pos2.Ads_Size,
                            pos2.Creative_Size, pos2.Creative_Type, pos2.IsPlacement, pos2.Overlay_Card);
                        lstSetCreative.Add(new SizeModel(data_creative_id_2, pos2.PositionName, pos2.Creative_Size,
                            pos2.Ads_Size, pos2.Creative_Type, pos2.IsPlacement, pos2.Overlay_Card, pos2.Input_Size));
                    }
                }
            }

            // Xử lý từng Creative trong lstSetCreative
            string[] separatingStrings = { "#$#" };
            foreach (var objPos in lstSetCreative)
            {
                List<string> lst_size = new List<string>();
                string[] arrSize = objPos.size.Split(',');
                foreach (string size in arrSize)
                {
                    lst_size.Add(size);
                }

                List<string> lst_ads_size = new List<string>();
                string[] arrAdSize = objPos.ads_size.Split(separatingStrings, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string adsize in arrAdSize)
                {
                    lst_ads_size.Add(adsize);
                }

                int creative_type_temp = Inread ? 1 : CreativeType(objPos.creative_type, 0);
                string input_size_temp = InputSize(objPos.input_size, 0);

                if (lstCreative.Count() > 0)
                {
                    // Tìm Creative phù hợp với size
                    var ltCreative = (from s in lst_size
                                      join c in lstCreative on s equals c.ads_size
                                      group c by c.ads_size into g
                                      select new { Size = g.Key, Id = g.Min(y => y.Id), Index = g.Min(x => x.index) }).OrderBy(x => x.Index);

                    int Creative_Id = -1;
                    if (lstCreative.Count() == 1)
                    {
                        Creative_Id = lstCreative[0].Id;
                    }

                    if (ltCreative.Count() > 0) // Kiem tra xem co kich thuoc phu hop khong
                    {
                        Creative_Id = ltCreative.ToList()[0].Id;
                    }
                    //else
                    //{
                    //    Creative_Id = -1; // cuonglv update 07-1-2026
                    //}

                    if (Creative_Id > 0)
                    {
                        for (int k = 0; k <= lstCreative.Count() - 1; k++)
                        {
                            if (lstCreative[k].Id == Creative_Id)
                            {
                                CreativeModel obj = new CreativeModel();
                                obj.Id = lstCreative[k].Id;
                                obj.click_through_url = lstCreative[k].click_through_url;

                                // Tạo tên Creative
                                if (Inread)
                                {
                                    obj.name = "Banner_Inread_" + lstCreative[k].ads_size + "_" + Common.RandString();
                                }
                                else
                                {
                                    obj.name = "Banner_" + (line_item.line_item_type == 5 ? "" : (objPos.name.Replace("[", "").Replace("]", "") + "_")) +
                                        lstCreative[k].ads_size + "_" + Common.RandString();
                                }

                                obj.iframe_url = lstCreative[k].iframe_url;
                                obj.ads_size = lstCreative[k].ads_size;
                                obj.index_industrial = lstCreative[k].index_industrial;
                                obj.index_brand = lstCreative[k].index_brand;
                                obj.campaign_name = lstCreative[k].campaign_name;
                                obj.overlay_card = objPos.overlay_card;
                                obj.creative_template = lstCreative[k].creative_template;
                                obj.creative_url = lstCreative[k].creative_url;
                                obj.run_mode = lstCreative[k].run_mode;
                                obj.index = lstCreative[k].index;
                                obj.third_party_tracking = lstCreative[k].third_party_tracking;
                                obj.line_item_sync = 0;
                                obj.data_creative_id = objPos.data_id;
                                obj.creative_type = creative_type_temp;
                                obj.input_size = input_size_temp;
                                obj.safe_frame = safe_frame;

                                // Xử lý add_sizes
                                if (lst_ads_size.Count > 0)
                                    obj.add_sizes = creative_adsize(obj.creative_template, lst_ads_size[0]);

                                // Tìm size phù hợp trong lst_size
                                for (int h = 0; h <= lst_size.Count() - 1; h++)
                                {
                                    if (lst_size[h] == lstCreative[k].ads_size)
                                    {
                                        obj.add_sizes = creative_adsize(obj.creative_template, lst_ads_size[h]);
                                        obj.input_size = InputSize(objPos.input_size, h);
                                        obj.creative_type = Inread ? 1 : CreativeType(objPos.creative_type, h);
                                        break;
                                    }
                                }

                                line_item.creative.Add(obj);
                                lstCreative[k].index = lstCreative[k].index + 1;

                                // Xử lý companion_sizes
                                if (line_item.creative.Count() == 1)
                                {
                                    line_item.creative[0].line_item_sync = creative_sync;
                                    if (obj.ads_size != null)
                                    {
                                        line_item.add_sizes = obj.add_sizes;
                                        line_item.companion_sizes.Add(obj.ads_size);
                                    }
                                }
                                else
                                {
                                    if (obj.add_sizes != null)
                                    {
                                        if (!line_item.companion_sizes.Contains(obj.add_sizes))
                                        {
                                            line_item.companion_sizes.Add(obj.add_sizes);
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Thêm các Creatives chưa sử dụng vào các LineItems
        /// </summary>
        private void AddUnusedCreativesToLineItems(List<LineItemModel> lineItems, List<CreativeModel> lstCreative)
        {
            var lstCreativeNotUsed = (from cretive_not_use in lstCreative
                                      where cretive_not_use.index == 0
                                      select cretive_not_use).ToList<CreativeModel>();

            if (lstCreativeNotUsed.Count() > 0)
            {
                foreach (var item in lineItems)
                {
                    int index_temp = 0;
                    if (item.creative.Count > 0)
                    {
                        int creative_type_temp = item.creative[0].creative_type;
                        int overlay_card_temp = item.creative[0].overlay_card;
                        int data_creative_id_temp = item.creative[0].data_creative_id;
                        string add_sizes_temp = item.creative[0].add_sizes;
                        string name_temp = item.creative[0].name;
                        string ads_size_temp = item.creative[0].ads_size;
                        string input_size_temp = item.creative[0].input_size;
                        int safe_frame_temp = item.creative[0].safe_frame;

                        for (int k = 0; k <= lstCreativeNotUsed.Count - 1; k++)
                        {
                            if (ads_size_temp == lstCreativeNotUsed[k].ads_size)
                            {
                                index_temp++;
                                CreativeModel objCreative = new CreativeModel();
                                objCreative.Id = lstCreativeNotUsed[k].Id;
                                objCreative.click_through_url = lstCreativeNotUsed[k].click_through_url;
                                objCreative.name = name_temp + "_" + "0" + index_temp.ToString();
                                objCreative.iframe_url = lstCreativeNotUsed[k].iframe_url;
                                objCreative.ads_size = lstCreativeNotUsed[k].ads_size;
                                objCreative.index_industrial = lstCreativeNotUsed[k].index_industrial;
                                objCreative.index_brand = lstCreativeNotUsed[k].index_brand;
                                objCreative.campaign_name = lstCreativeNotUsed[k].campaign_name;
                                objCreative.creative_type = creative_type_temp;
                                objCreative.overlay_card = overlay_card_temp;
                                objCreative.creative_template = lstCreativeNotUsed[k].creative_template;
                                objCreative.creative_url = lstCreativeNotUsed[k].creative_url;
                                objCreative.run_mode = lstCreativeNotUsed[k].run_mode;
                                objCreative.index = lstCreativeNotUsed[k].index;
                                objCreative.third_party_tracking = lstCreativeNotUsed[k].third_party_tracking;
                                objCreative.line_item_sync = 0;
                                objCreative.data_creative_id = data_creative_id_temp;
                                objCreative.add_sizes = add_sizes_temp;
                                objCreative.input_size = input_size_temp;
                                objCreative.safe_frame = safe_frame_temp;
                                item.creative.Add(objCreative);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        private string GetName(string ContractNo, int ProductId, int RequestId, string Custommer, string Campaign, string Position, string Date, double discount)
        {
            string name = "";
            if (Environment == "Dev")
            {
                name = ContractNo + "-" + ProductId.ToString() + "-" + RequestId.ToString() + "-DEV-TEST-" + Common.RandString();
            }
            else
            {
                name = ContractNo + "-" + ProductId.ToString() + "-" + RequestId.ToString() + "-" + Custommer + "-" + Campaign + "-" + Position + "-" + Date + '-' + Common.RandString();
            }

            if (!Common.IsInteger(discount))
            {
                name = name + "(CK:" + discount.ToString() + "%)";
            }

            return name;
        }

        public string creative_adsize(string format, string adsize)
        {
            if (adsize == "Inpage iFrame (All)" && format == "Image Tracking") return "Inpage Images (All)";
            if (adsize == "Popup iFrame" && format == "Image Tracking") return "Popup Images";
            return adsize;
        }

        public string creative_template(string url)
        {
            if (url.ToLower().IndexOf("salescloud.fptonline.net") >= 0)
            {
                if (url.ToLower().IndexOf(".txt") >= 0)
                {
                    return "Code Tracking";
                }
                else
                {
                    return "Image Tracking";
                }
            }
            return "Iframer Tracking";
        }

        // Quy định điều hướng tạo Creative
        //public string creative_template(string position, string ads_size)
        //{
        //    if (ads_size == "out-of-page")
        //    {
        //        switch (position)
        //        {
        //            case "sticky":
        //                return "Standard banner";
        //            case "Popup":
        //                return "Popup";
        //            case "In Image":
        //                return "Popup";
        //            default:
        //                break;
        //        }             
        //    }           
        //    return "Iframer Tracking";
        //}

        public string creative_runmode(string url)
        {
            if (url.ToLower().IndexOf("salescloud.fptonline.net") >= 0)
            {
                if (url.ToLower().IndexOf(".txt") >= 0)
                {
                    return "script_code";
                }
                else
                {
                    return "image";
                }

            }
            return "iframe";
        }

        public int GetDataId(DataTable dt, int SetLineItem, string PositionName, string Ads_Size, string Creative_Size, string Creative_Type, int IsPlacement, int Overlay_Card)
        {
            var data_id = (from p in dt.AsEnumerable()
                           where p.Field<int>("SetLineItem") == SetLineItem
                           && p.Field<string>("PositionName") == PositionName
                           && p.Field<string>("Ads_Size") == Ads_Size
                           && p.Field<string>("Size") == Creative_Size
                           && p.Field<string>("CreativeType") == Creative_Type
                           && p.Field<int>("IsPlacement") == IsPlacement
                           && p.Field<int>("OverlayCard") == Overlay_Card
                           select new { Id = p.Field<int>("Id") }).Min(x => x.Id);
            return data_id;
        }

        public string GetLandingpage(List<string> lstLandingpage, string size)
        {
            if (lstLandingpage.Count == 1) return lstLandingpage[0];
            foreach (string link in lstLandingpage)
            {
                if (link.ToLower().Contains(size.ToLower()))
                {
                    return link;
                }
                else if (link.ToLower().EndsWith(size.ToLower()))
                {
                    return link;
                }
            }

            if (lstLandingpage.Count > 0)
            {
                return lstLandingpage[0];
            }

            return "";
        }

        private int CreativeType(string creative, int index)
        {
            var arr = creative.Split(',');
            if (arr.Length <= index) return Convert.ToInt32(arr[0]);
            return Convert.ToInt32(arr[index]);
        }

        private string InputSize(string size, int index)
        {
            var arr = size.Split(',');
            if (arr.Length <= index) return arr[0];
            return arr[index];
        }

        public List<LocationModel> GetListLocation(List<LocationModel> lstInput)
        {
            List<LocationModel> lst = new List<LocationModel>();
            foreach (LocationModel obj in lstInput)
            {
                lst.Add(new LocationModel(obj.location_id, obj.location_name, obj.region, obj.include));
            }
            List<LocationModel> lstResult = new List<LocationModel>();
            if (lst.Count == 0) return lstResult;
            LocationModel objRegion = null;
            objRegion = CheckRegion(1, lst); //Miền Bac
            if (objRegion != null) lstResult.Add(new LocationModel(objRegion.location_id, objRegion.location_name, 1, objRegion.include));
            objRegion = CheckRegion(2, lst); //Mien Trung
            if (objRegion != null) lstResult.Add(new LocationModel(objRegion.location_id, objRegion.location_name, 1, objRegion.include));
            objRegion = CheckRegion(3, lst); //Mien Nam
            if (objRegion != null) lstResult.Add(new LocationModel(objRegion.location_id, objRegion.location_name, 1, objRegion.include));
            foreach (LocationModel obj in lst)
            {
                if (obj.location_name != "NOT_RUN")
                {
                    if (obj.location_id == 1000 || obj.location_id == 1002 || obj.location_id == 1003)
                    {
                        lstResult.Add(new LocationModel(obj.location_id, obj.location_name, 1, obj.include));
                    }
                    else
                    {
                        lstResult.Add(new LocationModel(obj.location_id, obj.location_name, 0, obj.include));
                    }
                }
            }
            return lstResult;
        }

        public LocationModel CheckRegion(int RegionId, List<LocationModel> lst)
        {
            if (lst.Count == 0) return null;

            LocationModel objResult = null;
            string[] arrRegion = null;
            switch (RegionId)
            {
                case 1:
                    arrRegion = location_mien_bac.Split(',');
                    break;
                case 2:
                    arrRegion = location_mien_trung.Split(',');
                    break;
                case 3:
                    arrRegion = location_mien_nam.Split(',');
                    break;
                default:
                    arrRegion = null;
                    break;
            }

            if (arrRegion == null) return null;

            var item = from x in lst
                       join mb in arrRegion on x.location_id equals Convert.ToInt32(mb)
                       select new { id = x.location_id, name = x.location_name };

            if (arrRegion.Length == item.Count())
            {
                switch (RegionId)
                {
                    case 1:
                        objResult = new LocationModel(1000, "Miền Bắc", 1, 1);
                        break;
                    case 2:
                        objResult = new LocationModel(1002, "Miền Trung", 1, 1);
                        break;
                    case 3:
                        objResult = new LocationModel(1003, "Miền Nam", 1, 1);
                        break;
                    default:
                        arrRegion = null;
                        break;
                }
                List<LocationModel> lstRemove = new List<LocationModel>();
                foreach (LocationModel obj in lst)
                {
                    foreach (string id in arrRegion)
                    {
                        if (obj.location_id == Convert.ToInt32(id))
                        {
                            lstRemove.Add(obj);
                            break;
                        }
                    }
                }

                foreach (LocationModel remove in lstRemove)
                {
                    lst.Remove(remove);
                }
            }

            return objResult;
        }

        public void GetDeliverTime(string time, out string start, out string end)
        {
            start = "";
            end = "";
            int start_hour = 0;
            int start_minute = 0;
            int end_hour = 0;
            int end_minute = 0;
            if (time.IndexOf("Từ") >= 0 && time.IndexOf("Đến") >= 0)
            {
                string[] timeStrings = time.Split(new string[] { "Từ", "Đến" }, StringSplitOptions.RemoveEmptyEntries);
                if (timeStrings.Count() == 1)
                {
                    start = timeStrings[0].Trim();
                    end = "00:00";
                }
                else if (timeStrings.Count() == 2)
                {
                    start = timeStrings[0].Trim();
                    end = timeStrings[1].Trim();
                }

                if (start.Trim() == "-1") start = "00:00";
                if (end.Trim() == "-1" && end.Trim() == "") end = "24:00";
            }
            else
            {
                string time_temp = Regex.Replace(time, @"\D", " ").TrimStart().TrimEnd();
                if (time_temp == "") return;
                while (time_temp.Contains("  "))
                {
                    time_temp = time_temp.Replace("  ", " ");
                }

                if (time_temp == " " || time_temp == "") return;

                var arrHour = time_temp.Split(' ');
                int count_items = arrHour.Count();

                if (count_items >= 4)
                {
                    start_hour = Convert.ToInt32(arrHour[0]);
                    start_minute = Convert.ToInt32(arrHour[1]);
                    end_hour = Convert.ToInt32(arrHour[2]);
                    end_minute = Convert.ToInt32(arrHour[3]);
                }
                else
                {
                    if (count_items > 0)
                    {
                        start_hour = Convert.ToInt32(arrHour[0]);
                        if (start_hour > 24) start_hour = 0;
                    }

                    if (count_items > 1)
                    {
                        start_minute = Convert.ToInt32(arrHour[1]);
                    }

                    if (count_items > 2)
                    {
                        end_hour = Convert.ToInt32(arrHour[2]);
                    }

                    if (end_hour > 24 && start_minute < 24)
                    {
                        start_minute = 0;
                        end_minute = end_hour;
                        if (end_minute > 59) end_minute = 0;
                        end_hour = start_minute;
                        start_minute = 0;
                    }
                }

                if (start_hour > 24) start_hour = 0;
                if (start_minute > 59) start_minute = 0;
                if (end_hour > 24) end_hour = 0;
                if (end_minute > 59) end_minute = 0;

                if (start_hour > end_hour) end_hour = 0;
                if (start_hour == end_hour && end_hour == 0) return;

                start_minute = (start_minute / 15);
                start_minute = start_minute * 15;

                end_minute = (end_minute / 15);
                end_minute = end_minute * 15;

                start = (start_hour >= 10 ? start_hour.ToString() : "0" + start_hour.ToString()) + ":" + (start_minute >= 10 ? start_minute.ToString() : "0" + start_minute.ToString());
                end = (end_hour >= 10 ? end_hour.ToString() : "0" + end_hour.ToString()) + ":" + (end_minute >= 10 ? end_minute.ToString() : "0" + end_minute.ToString());
            }

            if (end == "24:00") end = "00:00";

            if (start == "00:00" && end == "00:00")
            {
                start = "";
                end = "";
            }
        }

        public List<Gender> GetDeliverGender(string gender)
        {
            List<Gender> lstGender = new List<Gender>();
            if (gender == "" || gender == "-1" || gender == "0,1") return lstGender;
            var arr = gender.Split(',');
            foreach (string item in arr)
            {
                if (item.Trim() == "" || item.Trim() == "-1") continue;
                if (item.Trim() == "0") lstGender.Add(new Gender("Female", 0));
                if (item.Trim() == "1") lstGender.Add(new Gender("Male", 0));
            }
            return lstGender;
        }

        public List<Age> GetDeliverAge(string age, List<string> lstFull)
        {
            List<Age> lstAge = new List<Age>();
            List<string> lstTarget = new List<string>();
            if (age == "" || age == "-1") return lstAge;
            var arr = age.Split(',');
            foreach (string item in arr)
            {
                if (item.Trim() == "" || item.Trim() == "-1") continue;
                lstTarget.Add(item.Trim());
            }

            if (lstTarget.Count > 0)
            {

                var lst = lstFull.Where(p => lstTarget.All(p2 => p2 != p));

                if (lst.Count() > 0)
                {
                    foreach (var range in lst)
                    {
                        lstAge.Add(new Age(range, 0));
                    }
                }
            }

            return lstAge;
        }

        public List<Audience> GetDeliverAudience(string audience)
        {
            List<Audience> lstAudience = new List<Audience>();
            if (audience == "" || audience == "-1") return lstAudience;
            var arr = audience.Split(',');
            foreach (string item in arr)
            {
                if (item.Trim() == "" || item.Trim() == "-1") continue;
                lstAudience.Add(new Audience(item.Trim()));
            }
            return lstAudience;
        }

        public List<Device> GetDeliverDevice(string device)
        {
            List<Device> lstDevice = new List<Device>();
            if (device == "" || device == "-1") return lstDevice;
            var arr = device.Split(',');
            foreach (string item in arr)
            {
                if (item.Trim() == "" || item.Trim() == "-1") continue;
                switch (item.Trim())
                {
                    case "PC":
                        lstDevice.Add(new Device("Desktop"));
                        lstDevice.Add(new Device("Connected TV"));
                        lstDevice.Add(new Device("Tablet"));
                        break;
                    case "Mobile":
                        lstDevice.Add(new Device("Feature Phone"));
                        lstDevice.Add(new Device("Smartphone"));
                        break;
                    default:
                        break;
                }
            }
            return lstDevice;
        }

        public List<Article> GetArticle(string article, string article_scl)
        {
            List<Article> lstArticle = new List<Article>();
            if (article == "" && article_scl == "") return lstArticle;
            string[] arr = null;
            if (article != "")
            {
                arr = article.Split(',');
            }
            else
            {
                arr = article_scl.Split(',');
            }
            foreach (string item in arr)
            {
                if (item.Trim() == "") continue;
                lstArticle.Add(new Article(item.Trim()));
            }

            return lstArticle;
        }

        public List<CategoryId> GetCategoryId(string categoryid)
        {
            List<CategoryId> lstCategoryId = new List<CategoryId>();
            if (categoryid == "") return lstCategoryId;
            var arr = categoryid.Split(',');
            foreach (string item in arr)
            {
                if (item.Trim() == "") continue;
                lstCategoryId.Add(new CategoryId(item.Trim()));
            }

            return lstCategoryId;
        }

        public List<Tag> GetTag(string tag)
        {
            List<Tag> lstTag = new List<Tag>();
            if (tag == "") return lstTag;
            var arr = tag.Split(',');
            foreach (string item in arr)
            {
                if (item.Trim() == "") continue;
                lstTag.Add(new Tag(item.Trim()));
            }

            return lstTag;
        }

        public List<Brandsafe> GetBrandsafe(string brandsafe)
        {
            List<Brandsafe> lstBrandsafe = new List<Brandsafe>();
            if (brandsafe == "") return lstBrandsafe;
            var arr = brandsafe.Split(',');
            foreach (string item in arr)
            {
                if (item.Trim() == "") continue;
                lstBrandsafe.Add(new Brandsafe(item.Trim()));
            }

            return lstBrandsafe;
        }

    }
}
