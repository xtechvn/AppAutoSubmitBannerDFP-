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
