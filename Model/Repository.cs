
using AppAutoSubmitBannerDFP.Common;
using AppAutoSubmitBannerDFP.Engines;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppAutoSubmitBannerDFP.Model
{
    public class Repository
    {
        public static DataTable getBannerDfpDetail(int product_id, int request_id)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[2];
                objParam[0] = new SqlParameter("@product_id", product_id);
                objParam[1] = new SqlParameter("@request_id", request_id);
                DataTable tb = new DataTable();
                DBWorker.Fill(tb, "sp_getDetailBannerDfp", objParam);
                return tb;
            }
            catch (Exception ex)
            {
                ErrorWriter.WriteLog("D://", "getBannerDfpDetail Error" + ex.ToString());
                return null;
            }
        }

        public static DataTable getMailSetupDfp(int ProductId, int RequestId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[2];
                objParam[0] = new SqlParameter("@RequestId", RequestId);
                objParam[1] = new SqlParameter("@ProductId", ProductId);
                DataTable tb = new DataTable();
                DBWorker.Fill(tb, "sp_SendMailSetupDfp", objParam);
                return tb;
            }
            catch (Exception ex)
            {
                ErrorWriter.WriteLog("D://", "sp_SendMailSetupDfp Error" + ex.ToString());
                return null;
            }
        }

        public static int updateStatusFilter(int parent_id, string link, int type, int product_Id, int request_id, string keyname, int Slot, int data_id)
        {
            int IdRs = -1;
            try
            {
                //SqlParameter[] objParam = new SqlParameter[8];
                //objParam[0] = new SqlParameter("@parent_id", parent_id);
                //objParam[1] = new SqlParameter("@link", link);
                //objParam[2] = new SqlParameter("@type", type);
                //objParam[3] = new SqlParameter("@product_Id", product_Id);
                //objParam[4] = new SqlParameter("@request_id", request_id);
                //objParam[5] = new SqlParameter("@keyName", keyname);
                //objParam[6] = new SqlParameter("@Slot", Slot);
                //objParam[7] = new SqlParameter("@data_id", data_id);
                //IdRs = DBWorker.ExecuteNonQuery("InsertDfptSetup", objParam);
                //return IdRs;

                // updateStatusFilter

                //string param = "?parent_id=" + parent_id + "&link=" + link + "&type=" + type + "&product_Id="
                //   + product_Id + "&request_Id=" + request_id + "&keyName=" + keyname + "&slot=" + Slot + "&data_id=" + data_id;


                var requestData = new BannerSetupDfpModel
                {
                    parent_id = parent_id,
                    link = link,
                    type = type,
                    product_Id = product_Id,
                    request_Id = request_id,
                    keyName = keyname,
                    slot = Slot,
                    data_id = data_id
                };

                string jsonString = JsonConvert.SerializeObject(requestData);
                string base64EncodedJson = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonString));



                var res = ConnectApiSc.getDataSalesCloudMethodGet("/api/dfp-services/insert-dfp-setup", "token=" + base64EncodedJson);

                var data = JObject.Parse(res);
                if (data["error"].ToString() == "0")
                {
                    int id = Convert.ToInt32(data["data"].ToString());
                    return id;
                }
                return -1;
            }
            catch (Exception ex)
            {
                ErrorWriter.WriteLog("D://", "[updateStatusFilter] Error" + ex.ToString());
                return -1;
            }
        }

        public static int updateSlotSyncBannerDFP(int key_id, int slot)
        {
            int IdRs = -1;
            try
            {
                SqlParameter[] objParam = new SqlParameter[2];
                objParam[0] = new SqlParameter("@key_id", key_id);
                objParam[1] = new SqlParameter("@slot", slot);

                IdRs = DBWorker.ExecuteNonQuery("sp_updateSlotSyncBannerDFP", objParam);
                return IdRs;
            }
            catch (Exception ex)
            {
                ErrorWriter.WriteLog("D://", "[updateSlotSyncBannerDFP] Error" + ex.ToString());
                return -1;
            }
        }
    }
}
