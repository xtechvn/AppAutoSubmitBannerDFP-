
using System.Collections.Generic;

namespace AppAutoSubmitBannerDFP.ViewModel
{
    public class BannerEntities
    {

        public int ProductId { get; set; }
        public int RequestId { get; set; }
        public OrderViewModel order { get; set; }
        //public LineItemViewModel line_item { get; set; }       
        //public CreativeViewModel creative { get; set; }
    }
}
