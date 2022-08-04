using Telegram.Bot;
using Telegram.Bot.Types;

namespace BusinesLogic.Services.Interfaces
{
    public interface IHandleUpdateService
    {
        Task EchoAsync(Update update,ITelegramBotClient client);
    }
}
