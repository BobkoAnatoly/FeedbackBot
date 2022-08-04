using BusinesLogic.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BusinesLogic.Services.Implementations
{
    public class HandleUpdateService : IHandleUpdateService
    {
        private ITelegramBotClient _botClient;
        public HandleUpdateService()
        {
        }
        public async Task EchoAsync(Update update,ITelegramBotClient client)
        {
            _botClient = client;
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(update.Message!),
                UpdateType.EditedMessage => BotOnMessageReceived(update.EditedMessage!),
                UpdateType.InlineQuery => BotOnInlineQueryReceived(update.InlineQuery!),
                UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(update.ChosenInlineResult!),
                _ => UnknownUpdateHandlerAsync(update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(exception);
            }
        }

        
        private async Task BotOnMessageReceived(Message message)
        {
            if (message.Type != MessageType.Text)
                return;
            var action = message.Text!.Split(' ')[0] switch
            {
                "/start"=>_botClient.SendTextMessageAsync(message.Chat.Id,"Привет"),
                _=> _botClient.SendTextMessageAsync(message.Chat.Id,"123")
                //"/inline" => SendInlineKeyboard(_botClient, message),
                //"/keyboard" => SendReplyKeyboard(_botClient, message),
                //"/remove" => RemoveKeyboard(_botClient, message),
                //"/photo" => SendFile(_botClient, message),
                //"/request" => RequestContactAndLocation(_botClient, message),
                //_ => Usage(_botClient, message)
            };
            Message sentMessage = await action;
        }




        private Task HandleErrorAsync(Exception exception)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            return Task.CompletedTask;
        }
        private Task UnknownUpdateHandlerAsync(Update update)
        {
            throw new NotImplementedException();
        }

        private Task BotOnChosenInlineResultReceived(ChosenInlineResult chosenInlineResult)
        {
            throw new NotImplementedException();
        }

        private Task BotOnInlineQueryReceived(InlineQuery inlineQuery)
        {
            throw new NotImplementedException();
        }

    }
}
