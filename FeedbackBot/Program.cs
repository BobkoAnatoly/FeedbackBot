using BusinesLogic.Services.Implementations;
using BusinesLogic.Services.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Model.Models;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using System.Web;
using Microsoft.AspNetCore.Http;
using System.Net;
using Model;

namespace FeedbackBot
{
    class Program
    {
        public static string GetFinalRedirect(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            int maxRedirCount = 8;  // prevent infinite loops
            string newUrl = url;
            do
            {
                HttpWebRequest req = null;
                HttpWebResponse resp = null;
                try
                {
                    req = (HttpWebRequest)HttpWebRequest.Create(url);
                    req.Method = "HEAD";
                    req.AllowAutoRedirect = false;
                    resp = (HttpWebResponse)req.GetResponse();
                    switch (resp.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            return newUrl;
                        case HttpStatusCode.Redirect:
                        case HttpStatusCode.MovedPermanently:
                        case HttpStatusCode.RedirectKeepVerb:
                        case HttpStatusCode.RedirectMethod:
                            newUrl = resp.Headers["Location"];
                            if (newUrl == null)
                                return url;

                            if (newUrl.IndexOf("://", System.StringComparison.Ordinal) == -1)
                            {
                                // Doesn't have a URL Schema, meaning it's a relative or absolute URL
                                Uri u = new Uri(new Uri(url), newUrl);
                                newUrl = u.ToString();
                            }
                            break;
                        default:
                            return newUrl;
                    }
                    url = newUrl;
                }
                catch (WebException)
                {
                    // Return the last known good URL
                    return newUrl;
                }
                catch (Exception ex)
                {
                    return null;
                }
                finally
                {
                    if (resp != null)
                        resp.Close();
                }
            } while (maxRedirCount-- > 0);

            return newUrl;
        }
        static IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<IHandleUpdateService, HandleUpdateService>();
                    services.AddTransient<Update>();
                })
                .Build();
        static void Main(string[] args)
        {
            #region MyRegion

            /*ApplicationDatabaseContext contet = new ApplicationDatabaseContext();
            string path = "https://www.polessu.by/%D0%BF%D0%B5%D1%80%D1%81%D0%BE%D0%BD%D0%B0%D0%BB%D1%8C%D0%BD%D0%B0%D1%8F-%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B8%D1%86%D0%B0-%D0%BF%D1%80%D0%B5%D0%BF%D0%BE%D0%B4%D0%B0%D0%B2%D0%B0%D1%82%D0%B5%D0%BB%D1%8F";
            try
            {
                using (HttpClientHandler hdl = new HttpClientHandler
                {
                    AllowAutoRedirect = false,
                    AutomaticDecompression = System.Net.DecompressionMethods.Deflate |
                    System.Net.DecompressionMethods.GZip |
                    System.Net.DecompressionMethods.None
                })
                {
                    using (var clnt = new HttpClient(hdl))
                    {
                        using (var response = clnt.GetAsync(path).Result)
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                var html = response.Content.ReadAsStringAsync().Result;
                                if (!string.IsNullOrEmpty(html))
                                {
                                    HtmlDocument doc = new HtmlDocument();
                                    doc.LoadHtml(html);

                                    var professorLinks = doc.DocumentNode.SelectNodes(".//div[@class='field-items']//div//table//tbody//tr[1]//td//p//a");
                                    foreach (var item in professorLinks)
                                    {
                                        string[] name = item.InnerText.Split(new string[] {" ","&nbsp;"},StringSplitOptions.None);
                                        var professor = new Professor();
                                        try
                                        {
                                            professor.FirstName = name[1];
                                            professor.LastName = name[0];
                                            professor.Patronymic = name[2];
                                        }
                                        catch (Exception)
                                        {
                                            continue;
                                        }
                                        string spath = path+item.GetAttributeValue("href",null);
                                        try
                                        {
                                            using (HttpClientHandler hdl1 = new HttpClientHandler
                                            {
                                                AllowAutoRedirect = false,
                                                AutomaticDecompression = System.Net.DecompressionMethods.Deflate |
                                                System.Net.DecompressionMethods.GZip |
                                                System.Net.DecompressionMethods.None
                                            })
                                            {
                                                using (var clnt1 = new HttpClient(hdl1))
                                                {
                                                    string redirectpath = GetFinalRedirect(spath);
                                                    using (var response1 = clnt1.GetAsync(redirectpath).Result)
                                                    {
                                                        
                                                        if (response1.IsSuccessStatusCode)
                                                        {
                                                            var html1 = response1.Content.ReadAsStringAsync().Result;
                                                            if (!string.IsNullOrEmpty(html1))
                                                            {
                                                                HtmlDocument doc1 = new HtmlDocument();
                                                                doc1.LoadHtml(html1);

                                                                var professorLinks1 = doc1.DocumentNode.SelectNodes(".//div[@class='field-items']//div//table//tbody//tr[1]//td[1]//img");
                                                                foreach (var item1 in professorLinks1)
                                                                {
                                                                    professor.PhotoPath = "https://www.polessu.by/"+item1.GetAttributeValue("src", null);
                                                                    contet.Professors.Add(professor);
                                                                }
                                                                contet.SaveChanges();

                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex.Message);
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }*/
            #endregion

            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            Configure();


            Console.ReadLine();
        }
        public static void Configure()
        {
            string _token = "5457154768:AAEiqsRnu2V1IaJTmuomD7cbQQkm9r0ZGy8";
            ITelegramBotClient Bot;
            Bot = new TelegramBotClient(_token);
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            Bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.WriteLine("Запущен бот " + Bot.GetMeAsync().Result.FirstName);


            

        }

        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables();
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var svc = ActivatorUtilities.CreateInstance<HandleUpdateService>(host.Services);
            await svc.EchoAsync(update,botClient);
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));


        }
        public static async Task HandleErrorAsync(ITelegramBotClient botClient,
            Exception exception,
            CancellationToken cancellationToken)
        {

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }
    }
}