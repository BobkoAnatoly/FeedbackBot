using BusinesLogic.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FeedbackBot.Controllers
{
    public class UpdateController
    {
        private readonly IHandleUpdateService _handleUpdateService;

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            //if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            //{
            //    var message = update.Message;
            //    if (message.Text.ToLower() == "/start")
            //    {
            //        await botClient.SendTextMessageAsync(message.Chat, "Добро пожаловать на борт, добрый путник!");
            //        return;
            //    }
            //    await botClient.SendTextMessageAsync(message.Chat, "Привет-привет!!");
            //}
        }
    }
}
