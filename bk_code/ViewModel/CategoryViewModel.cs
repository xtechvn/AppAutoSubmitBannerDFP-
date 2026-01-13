using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppAutoSubmitBannerDFP.ViewModel
{
  public  class CategoryViewModel
    {
        public string category_name { get; set; }
        public List<CategoryChildViewModel> category_child { get; set; }
    }
    public class CategoryChildViewModel
    {
        public string category_child_name { get; set; }
    }
}
