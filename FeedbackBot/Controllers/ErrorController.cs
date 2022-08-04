using Telegram.Bot;

namespace FeedbackBot.Controllers
{
    public class ErrorController
    {
        public static async Task HandleErrorAsync(ITelegramBotClient botClient,
            Exception exception,
            CancellationToken cancellationToken)
        {

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }
    }
}
