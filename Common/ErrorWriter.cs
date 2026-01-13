using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppAutoSubmitBannerDFP.Common
{
    public class ErrorWriter
    {
        public static void WriteLog(string AppPath, string sFunction, string sAction)
        {
            StreamWriter sLogFile = null;
            try
            {
                //Ghi lại hành động của người sử dụng vào log file
                string sDay = string.Format("{0:dd}", DateTime.Now);
                string sMonth = string.Format("{0:MM}", DateTime.Now);
                string strLogFileName = sDay + "-" + sMonth + "-" + DateTime.Now.Year + ".log";
                string strFolderName = AppPath + @"\Logs\" + DateTime.Now.Year + "-" + sMonth;
                //Application.StartupPath
                //Tạo thư mục nếu chưa có
                if (!Directory.Exists(strFolderName + @"\"))
                {
                    Directory.CreateDirectory(strFolderName + @"\");
                }
                strLogFileName = strFolderName + @"\" + strLogFileName;

                if (File.Exists(strLogFileName))
                {
                    //Nếu đã tồn tại file thì tiếp tục ghi thêm
                    sLogFile = File.AppendText(strLogFileName);
                    sLogFile.WriteLine(string.Format("Thời điểm xảy ra lỗi: {0:hh:mm:ss tt}", DateTime.Now));
                    if (sFunction != string.Empty)
                        sLogFile.WriteLine(string.Format("Hàm/Phương thức sinh lỗi: {0}", sFunction));
                    sLogFile.WriteLine(string.Format("Chi tiết lỗi: {0}", sAction));
                    sLogFile.WriteLine("-------------------------------------------");
                    sLogFile.Flush();
                }
                else
                {
                    //Nếu file chưa tồn tại thì có thể tạo mới và ghi log
                    sLogFile = new StreamWriter(strLogFileName);
                    sLogFile.WriteLine(string.Format("Thời điểm xảy ra lỗi: {0:hh:mm:ss tt}", DateTime.Now));
                    if (sFunction != string.Empty)
                        sLogFile.WriteLine(string.Format("Hàm/Phương thức sinh lỗi: {0}", sFunction));
                    sLogFile.WriteLine(string.Format("Chi tiết lỗi: {0}", sAction));
                    sLogFile.WriteLine("-------------------------------------------");
                }
                sLogFile.Close();
            }
            catch (Exception)
            {
                if (sLogFile != null)
                {
                    sLogFile.Close();
                }
            }
        }

        public static void WriteLog(string AppPath, string sAction)
        {
            WriteLog(AppPath, string.Empty, sAction);
        }

        public static void WirteFile(string AppPath, string sContent)
        {
            StreamWriter sLogFile = null;
            try
            {
                sLogFile = new StreamWriter(AppPath);
                sLogFile.WriteLine(sContent);
                sLogFile.Close();
            }
            catch (Exception)
            {
                if (sLogFile != null)
                {
                    sLogFile.Close();
                }
            }
        }
    }
}
