using System;
using System.Collections.Generic;
using System.Text;

namespace AppAutoSubmitBannerDFP.ViewModel
{
    public class ErrorModel
    {
        public string name { get; set; }
        public string function { get; set; }
        public string detail { get; set; }
        public int type { get; set; }
        public ErrorModel(string sname, string sfunction, string sdetail, int itype)
        {
            name = sname;
            function = sfunction;
            detail = sdetail;
            type = itype;
        }
    }
}
