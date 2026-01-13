using AppAutoSubmitBannerDFP.Behaviors;
using AppAutoSubmitBannerDFP.Common;
using AppAutoSubmitBannerDFP.Engines;
using AppAutoSubmitBannerDFP.Engines.Creative;
using AppAutoSubmitBannerDFP.Engines.LineItem;
using AppAutoSubmitBannerDFP.Engines.Order;
using AppAutoSubmitBannerDFP.Engines.Proposal;
using AppAutoSubmitBannerDFP.Interfaces;
using AppAutoSubmitBannerDFP.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Configuration;
using System.Linq;
using System.Text;
//https://googlechromelabs.github.io/chrome-for-testing/
namespace AppAutoSubmitBannerDFP
{
    class Program
    {
        public static string QUEUE_HOST = ConfigurationManager.AppSettings["QUEUE_HOST"];
        public static string QUEUE_V_HOST = ConfigurationManager.AppSettings["QUEUE_V_HOST"];
        public static string QUEUE_USERNAME = ConfigurationManager.AppSettings["QUEUE_USERNAME"];
        public static string QUEUE_PASSWORD = ConfigurationManager.AppSettings["QUEUE_PASSWORD"];
        public static string QUEUE_PORT = ConfigurationManager.AppSettings["QUEUE_PORT"];
        public static string QUEUE_KEY_API = ConfigurationManager.AppSettings["QUEUE_KEY_API"];        
        private static string delay_start = ConfigurationManager.AppSettings["delay_start"];
        private static string task_queue_crawl_realtime = ConfigurationManager.AppSettings["queue_name"];
        private static string startupPath = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\bin\Debug\", @"\");
        private static string user_data_dir = ConfigurationManager.AppSettings["user_data_dir"];
        private static int task_job_type = Convert.ToInt32(ConfigurationManager.AppSettings["task_job_type"]);
        private static string tele_group_id = ConfigurationManager.AppSettings["tele_group_id"];
        private static string tele_token = ConfigurationManager.AppSettings["tele_token"];
        private static string DOMAIN_WEBSITE_CRAWLER = ConfigurationManager.AppSettings["DOMAIN_WEBSITE_CRAWLER"];
        private static string is_production = ConfigurationManager.AppSettings["is_production"];

        static void Main(string[] args)
        {
            try
            {
                //// setting SE
                var chrome_option = new ChromeOptions();
                chrome_option.AddArgument("--start-maximized"); // set full man hinh
                chrome_option.AddArgument(@"user-data-dir=" + user_data_dir);
                // Tắt tất cả extensions
                chrome_option.AddArgument("--disable-extensions");

                if (is_production == "0") // Môi trường local test
                {
                    #region TEST
                    var serviceProvider = new ServiceCollection()
                        .AddSingleton<ICrawlerFactory, CrawlerFactory>()
                        .AddSingleton<IOderService, OrderService>()
                        .AddSingleton<ILineItemCPCService, LineItemCPCService>()
                        .AddSingleton<ICreativeService, CreativeService>()
                        .AddSingleton<IProposalService, ProposalService>()
                        .BuildServiceProvider();
                    var page_crawler = serviceProvider.GetService<ICrawlerFactory>();


                    int request_id = 216297; //216083;// 215925;// 215978;
                    var objProcess = new dfpProcess();
                    var obj_banner_detail = objProcess.GetDataByRequestId(request_id);
                    var string_json = JsonConvert.SerializeObject(obj_banner_detail);


                    //string_json = "{\"ProductId\":323942,\"RequestId\":216046,\"order\":{\"name\":\"SG016762 / 20 - 323942 - 216046 - THỜI ĐẠI -TEST - Sun Bộ 2_Charmora_Inpage HCM#08.01_25.01-IP-0901-2501-24113\",\"advertiser\":\"CÔNG TY CỔ PHẦN THƯƠNG MẠI VÀ TRUYỀN THÔNG THỜI ĐẠI\",\"trafficker\":\"anhhtt10\",\"secondary_trafficker\":\"thulk;sonlh39;huongnt128\",\"sales_person\":\"LanTTH3\",\"secondary_salespeople\":\"\",\"order_url\":\"\",\"sync\":0,\"order_default\":0,\"line_item\":[{\"ad_type\":1,\"name\":\"SG016762/20-323942-216046-THỜI ĐẠI-Sun Bộ 2_Charmora_Inpage HCM#08.01_25.01-IP-0901-2501-24113(IP12)\",\"line_item_type\":2,\"add_sizes\":\"Inpage iFrame (All)\",\"companion_sizes\":[\"480x960\"],\"start_time\":\"2026-01-09T00:00:00\",\"end_time\":\"2026-01-25T23:59:00\",\"rate\":72000.0,\"vat\":8,\"discount\":25.0,\"unit_type\":1,\"adunit\":[],\"palcement\":[{\"placement\":\"MB: Inpage Gr1: IP, IR, BP App\"},{\"placement\":\"MB: Inpage Gr2: IP, IR, BP App\"}],\"location\":[{\"location_id\":62,\"location_name\":\"Ho Chi Minh City V\",\"region\":0,\"include\":1}],\"targeting_type\":null,\"custom_targeting\":\"\",\"lineitem_url\":null,\"quantity\":134300,\"percentage\":2,\"display_creative\":1,\"unit\":1,\"usd\":1,\"data_line_item_id\":56,\"item_type\":2,\"frequency\":0,\"goal\":-1,\"start_deliver_time\":\"\",\"end_deliver_time\":\"\",\"create_cretive\":0,\"age\":[],\"audience\":[],\"gender\":[],\"device\":[{\"device\":\"Desktop\"},{\"device\":\"Connected TV\"},{\"device\":\"Tablet\"}],\"article\":[],\"tag\":[],\"brandsafe\":[],\"categoryid\":[],\"category_name\":\"MB: Inpage Gr1: IP, IR, BP App\",\"position_name\":\"IP12\",\"creative\":[{\"name\":\"Banner_IP12_480x960_24113\",\"click_through_url\":\"https://charmoracity.vn/\",\"iframe_url\":\"https://static.eclick.vn/html5/vs_002/ads/s/sun/2026/01/08/215990/480x960/dfp/mobile/rmd/resp/inpage/index.html\",\"ads_size\":\"480x960\",\"input_size\":\"\",\"index_industrial\":\"L1001_Bất động sản \",\"index_brand\":\"LB1001.13_Sun group\",\"campaign_name\":\"Sun Bộ 2_Charmora_Inpage HCM#08.01_25.01\",\"creative_type\":0,\"creative_template\":\"Iframer Tracking\",\"creative_url\":\"\",\"run_mode\":\"iframe\",\"thirparty\":null,\"Id\":1,\"index\":0,\"line_item_sync\":0,\"add_sizes\":\"Inpage iFrame (All)\",\"third_party_tracking\":[],\"overlay_card\":1,\"data_creative_id\":56,\"safe_frame\":0}]},{\"ad_type\":1,\"name\":\"SG016762/20-323942-216046-THỜI ĐẠI-Sun Bộ 2_Charmora_Inpage HCM#08.01_25.01-IP-0901-2501-24113(IP12)\",\"line_item_type\":2,\"add_sizes\":\"Inpage iFrame (All)\",\"companion_sizes\":[\"480x960\"],\"start_time\":\"2026-01-09T00:00:00\",\"end_time\":\"2026-01-25T23:59:00\",\"rate\":72000.0,\"vat\":8,\"discount\":25.0,\"unit_type\":1,\"adunit\":[],\"palcement\":[{\"placement\":\"MB: Inpage Gr1: IP, IR, BP App\"},{\"placement\":\"MB: Inpage Gr2: IP, IR, BP App\"}],\"location\":[{\"location_id\":62,\"location_name\":\"Ho Chi Minh City V\",\"region\":0,\"include\":1}],\"targeting_type\":null,\"custom_targeting\":\"\",\"lineitem_url\":null,\"quantity\":134300,\"percentage\":2,\"display_creative\":1,\"unit\":1,\"usd\":1,\"data_line_item_id\":56,\"item_type\":2,\"frequency\":0,\"goal\":-1,\"start_deliver_time\":\"\",\"end_deliver_time\":\"\",\"create_cretive\":0,\"age\":[],\"audience\":[],\"gender\":[],\"device\":[{\"device\":\"Desktop\"},{\"device\":\"Connected TV\"},{\"device\":\"Tablet\"}],\"article\":[],\"tag\":[],\"brandsafe\":[],\"categoryid\":[],\"category_name\":\"MB: Inpage Gr2: IP, IR, BP App\",\"position_name\":\"IP12\",\"creative\":[{\"name\":\"Banner_IP12_480x960_24113\",\"click_through_url\":\"https://charmoracity.vn/\",\"iframe_url\":\"https://static.eclick.vn/html5/vs_002/ads/s/sun/2026/01/08/215990/480x960/dfp/mobile/rmd/resp/inpage/index.html\",\"ads_size\":\"480x960\",\"input_size\":\"\",\"index_industrial\":\"L1001_Bất động sản \",\"index_brand\":\"LB1001.13_Sun group\",\"campaign_name\":\"Sun Bộ 2_Charmora_Inpage HCM#08.01_25.01\",\"creative_type\":0,\"creative_template\":\"Iframer Tracking\",\"creative_url\":\"\",\"run_mode\":\"iframe\",\"thirparty\":null,\"Id\":1,\"index\":1,\"line_item_sync\":0,\"add_sizes\":\"Inpage iFrame (All)\",\"third_party_tracking\":[],\"overlay_card\":1,\"data_creative_id\":56,\"safe_frame\":0}]},{\"ad_type\":1,\"name\":\"SG016762/20-323942-216046-THỜI ĐẠI-Sun Bộ 2_Charmora_Inpage HCM#08.01_25.01-IP-0901-2501-24113(IPHome)\",\"line_item_type\":2,\"add_sizes\":\"Inpage iFrame (All)\",\"companion_sizes\":[\"480x960\"],\"start_time\":\"2026-01-09T00:00:00\",\"end_time\":\"2026-01-25T23:59:00\",\"rate\":72000.0,\"vat\":8,\"discount\":25.0,\"unit_type\":1,\"adunit\":[],\"palcement\":[{\"placement\":\"MB: Inpage Gr1: IPHome\"},{\"placement\":\"MB: Inpage G2: IPHome\"}],\"location\":[{\"location_id\":62,\"location_name\":\"Ho Chi Minh City V\",\"region\":0,\"include\":1}],\"targeting_type\":null,\"custom_targeting\":\"\",\"lineitem_url\":null,\"quantity\":23700,\"percentage\":2,\"display_creative\":1,\"unit\":1,\"usd\":1,\"data_line_item_id\":140,\"item_type\":2,\"frequency\":0,\"goal\":-1,\"start_deliver_time\":\"\",\"end_deliver_time\":\"\",\"create_cretive\":0,\"age\":[],\"audience\":[],\"gender\":[],\"device\":[{\"device\":\"Desktop\"},{\"device\":\"Connected TV\"},{\"device\":\"Tablet\"}],\"article\":[],\"tag\":[],\"brandsafe\":[],\"categoryid\":[],\"category_name\":\"MB: Inpage Gr1: IPHome\",\"position_name\":\"IPHome\",\"creative\":[{\"name\":\"Banner_IPHome_480x960_24113\",\"click_through_url\":\"https://charmoracity.vn/\",\"iframe_url\":\"https://static.eclick.vn/html5/vs_002/ads/s/sun/2026/01/08/215990/480x960/dfp/mobile/rmd/resp/inpage/index.html\",\"ads_size\":\"480x960\",\"input_size\":\"\",\"index_industrial\":\"L1001_Bất động sản \",\"index_brand\":\"LB1001.13_Sun group\",\"campaign_name\":\"Sun Bộ 2_Charmora_Inpage HCM#08.01_25.01\",\"creative_type\":0,\"creative_template\":\"Iframer Tracking\",\"creative_url\":\"\",\"run_mode\":\"iframe\",\"thirparty\":null,\"Id\":1,\"index\":2,\"line_item_sync\":0,\"add_sizes\":\"Inpage iFrame (All)\",\"third_party_tracking\":[],\"overlay_card\":1,\"data_creative_id\":140,\"safe_frame\":0}]},{\"ad_type\":1,\"name\":\"SG016762/20-323942-216046-THỜI ĐẠI-Sun Bộ 2_Charmora_Inpage HCM#08.01_25.01-IP-0901-2501-56405(IPHome)\",\"line_item_type\":2,\"add_sizes\":\"Inpage iFrame (All)\",\"companion_sizes\":[\"480x960\"],\"start_time\":\"2026-01-09T00:00:00\",\"end_time\":\"2026-01-25T23:59:00\",\"rate\":72000.0,\"vat\":8,\"discount\":25.0,\"unit_type\":1,\"adunit\":[],\"palcement\":[{\"placement\":\"MB: Inpage Gr1: IPHome\"},{\"placement\":\"MB: Inpage G2: IPHome\"}],\"location\":[{\"location_id\":62,\"location_name\":\"Ho Chi Minh City V\",\"region\":0,\"include\":1}],\"targeting_type\":null,\"custom_targeting\":\"\",\"lineitem_url\":null,\"quantity\":23700,\"percentage\":2,\"display_creative\":1,\"unit\":1,\"usd\":1,\"data_line_item_id\":140,\"item_type\":2,\"frequency\":0,\"goal\":-1,\"start_deliver_time\":\"\",\"end_deliver_time\":\"\",\"create_cretive\":0,\"age\":[],\"audience\":[],\"gender\":[],\"device\":[{\"device\":\"Desktop\"},{\"device\":\"Connected TV\"},{\"device\":\"Tablet\"}],\"article\":[],\"tag\":[],\"brandsafe\":[],\"categoryid\":[],\"category_name\":\"MB: Inpage G2: IPHome\",\"position_name\":\"IPHome\",\"creative\":[{\"name\":\"Banner_IPHome_480x960_56405\",\"click_through_url\":\"https://charmoracity.vn/\",\"iframe_url\":\"https://static.eclick.vn/html5/vs_002/ads/s/sun/2026/01/08/215990/480x960/dfp/mobile/rmd/resp/inpage/index.html\",\"ads_size\":\"480x960\",\"input_size\":\"\",\"index_industrial\":\"L1001_Bất động sản \",\"index_brand\":\"LB1001.13_Sun group\",\"campaign_name\":\"Sun Bộ 2_Charmora_Inpage HCM#08.01_25.01\",\"creative_type\":0,\"creative_template\":\"Iframer Tracking\",\"creative_url\":\"\",\"run_mode\":\"iframe\",\"thirparty\":null,\"Id\":1,\"index\":3,\"line_item_sync\":0,\"add_sizes\":\"Inpage iFrame (All)\",\"third_party_tracking\":[],\"overlay_card\":1,\"data_creative_id\":140,\"safe_frame\":0}]}]},\"line_item\":null,\"creative\":null}";


                    var model = JsonConvert.DeserializeObject<BannerEntities>(string_json);

                    // SE READY...
                    using (var browers = new ChromeDriver(startupPath, chrome_option, TimeSpan.FromMinutes(3)))
                    {
                        try
                        {
                            browers.Manage().Timeouts().PageLoad.Add(TimeSpan.FromSeconds(30));

                            Console.WriteLine("=============================== BEGIN " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + model.ProductId + "***REQUEST_ID: {0}", model.RequestId + "====================================");


                            
                            page_crawler.MainProcess(DOMAIN_WEBSITE_CRAWLER, browers, model);

                            Console.WriteLine("=============================== END " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + model.ProductId + "***REQUEST_ID: {0}", model.RequestId + "====================================");

                            browers.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("browers error  = " + ex.ToString());
                            Ultities.Telegram.pushNotify("browers error = " + ex.ToString(), tele_group_id, tele_token);
                            browers.Dispose();
                        }
                    }
                    #endregion
                }
                else
                {
                    #region READ QUEUE

                    var factory = new ConnectionFactory()
                    {
                        HostName = QUEUE_HOST,
                        UserName = QUEUE_USERNAME,
                        Password = QUEUE_PASSWORD,
                        VirtualHost = QUEUE_V_HOST,
                        Port = Protocols.DefaultProtocol.DefaultPort
                    };
                    using (var connection = factory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        try
                        {
                            channel.QueueDeclare(queue: task_queue_crawl_realtime,
                                                 durable: true,
                                                 exclusive: false,
                                                 autoDelete: false,
                                                 arguments: null);

                            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                            Console.WriteLine(" [*] Waiting for messages.");

                            var consumer = new EventingBasicConsumer(channel);
                            consumer.Received += (sender, ea) =>
                            {
                                try
                                {

                                    var body = ea.Body.ToArray();
                                    var message = Encoding.UTF8.GetString(body);

                                    Console.WriteLine("Received banner data Queue: {0}", message + "----------");

                                    var serviceProvider = new ServiceCollection()
                                                                         .AddSingleton<ICrawlerFactory, CrawlerFactory>()
                                                                         .AddSingleton<IOderService, OrderService>()
                                                                         .AddSingleton<ILineItemCPCService, LineItemCPCService>()
                                                                         .AddSingleton<ICreativeService, CreativeService>()
                                                                         .AddSingleton<IProposalService, ProposalService>()
                                                                        .BuildServiceProvider();
                                    var page_crawler = serviceProvider.GetService<ICrawlerFactory>();

                                    var model = JsonConvert.DeserializeObject<BannerEntities>(message);

                                // SE READY...
                                using (var browers = new ChromeDriver(startupPath, chrome_option, TimeSpan.FromMinutes(3)))
                                    {
                                        try
                                        {
                                            browers.Manage().Timeouts().PageLoad.Add(TimeSpan.FromSeconds(30));

                                            Console.WriteLine("=============================== BEGIN " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + model.ProductId + "***REQUEST_ID: {0}", model.RequestId + "====================================");

                                            page_crawler.MainProcess(DOMAIN_WEBSITE_CRAWLER, browers, model);

                                            Console.WriteLine("=============================== END " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " : PRODUCT_ID: " + model.ProductId + "***REQUEST_ID: {0}", model.RequestId + "====================================");

                                            browers.Dispose();
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine("browers error  = " + ex.ToString());
                                            Ultities.Telegram.pushNotify("browers error = " + ex.ToString(), tele_group_id, tele_token);
                                            browers.Dispose();
                                        }
                                    }

                                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("error queue: " + ex.ToString());
                                    Ultities.Telegram.pushNotify("error queue = " + ex.ToString(), tele_group_id, tele_token);
                                }
                            };

                            channel.BasicConsume(queue: task_queue_crawl_realtime, autoAck: false, consumer: consumer);

                            Console.ReadLine();

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            throw;
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Ultities.Telegram.pushNotify("Error job task queue crawl realtime = " + task_queue_crawl_realtime + "-- error: " + ex.ToString(), tele_group_id, tele_token);
                Console.WriteLine(" [x] Received message: {0}", ex.ToString());
            }

        }
    }
}
