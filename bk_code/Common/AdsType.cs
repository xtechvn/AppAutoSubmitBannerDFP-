using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppAutoSubmitBannerDFP.Common
{
    public enum AdsType
    {
        standard = 1,
        master_companion,
        video_or_audio
    }
    public enum UnitType
    {
        absolute_value = 1,
        percentage
    }
    public enum standard_creative
    {
        SITE_KHONG_CO_DINH = 0,
        SITE_CO_DINH = 1
    }

    public enum line_item_type
    {
        DFP_CPD = 1, // Thể loại DFP CPD: Sponsorship (4)
        DFP_CPM = 2, // Thể loại DFP CPM: Standard (6,8,10)
        DFP_CPM_TOI_UU = 5 // Thể loại DFP CPM tối ưu, CPC: Price priority (12)        
    }

    public enum item_type
    {        
        DFP_CPM_DEFAULT = 6 // Default: House (16)
    }

    public enum dfp_setup_type
    {
        order = 1,
        line_item = 2,
        creative = 3
    }
    public static class ADD_SIZE
    {
        public const string SIZE_300x600 = "300x600";
        public const string SIZE_BACKGROUND_U_3x3 = "Background_U_3x3";
        public const string SIZE_1920x270 = "1920x270";
        public const string SIZE_300x500 = "300x500";
        public const string SIZE_300x250_MEDIUM_RECTANGLE = "300x250 (Medium Rectangle)";        
        public const string SIZE_970x250 = "970x250";
        public const string SIZE_Masthead_Mobile = "Masthead Mobile";
        public const string SIZE_300x250 = "300x250";
        public const string SIZE_BREAKS_PAGE_MOBILE = "Breaks Page Mobile";
        public const string SIZE_480x270 = "480x270";
        public const string SIZE_IN_IMAGE_ADS = "In Images | Ads";
        public const string SIZE_INPAGE_IFRAME = "Inpage iFrame (All)";
        public const string SIZE_INPAGE_IMAGES = "Inpage Images (All)";
    }
}
