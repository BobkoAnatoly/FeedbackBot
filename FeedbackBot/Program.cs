using BusinesLogic.Services.Implementations;
using BusinesLogic.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

namespace FeedbackBot
{
    class Program
    {
        static IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<IHandleUpdateService, HandleUpdateService>();
                    services.AddTransient<Update>();
                })
                .Build();
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            Configure();


            Console.ReadLine();
        }
        public static void Configure()
        {
            string _token = "5457154768:AAG6-8UKoLCUbA83G-c7H16iDlqW-my3c6I";
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