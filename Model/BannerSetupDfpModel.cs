namespace AppAutoSubmitBannerDFP.Model
{
    //string param = "?parent_id=" + parent_id + "&link=" + link + "&type=" + type + "&product_Id="
    //   + product_Id + "&request_Id=" + request_id + "&keyName=" + keyname + "&slot=" + Slot + "&data_id=" + data_id;
    class BannerSetupDfpModel
    {
        public int parent_id { get; set; }
        public string link { get; set; }
        public int type { get; set; }
        public int product_Id { get; set; }
        public int request_Id { get; set; }
        public string keyName { get; set; }
        public int slot { get; set; }
        public int data_id { get; set; }
    }
}
