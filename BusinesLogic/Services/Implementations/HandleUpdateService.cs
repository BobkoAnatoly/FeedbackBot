using BusinesLogic.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Model;
using Model.Models;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.ReplyMarkups;

namespace BusinesLogic.Services.Implementations
{
    public class HandleUpdateService : IHandleUpdateService
    {
        int count = 1;
        private ApplicationDatabaseContext context;
        private ITelegramBotClient _botClient;
        private CancellationToken cancellationToken;
        public HandleUpdateService()
        {
            cancellationToken = new();
            context = new ApplicationDatabaseContext();
        }
        public async Task EchoAsync(Update update, ITelegramBotClient client)
        {
            _botClient = client;
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(update.Message!),
                UpdateType.EditedMessage => BotOnMessageReceived(update.EditedMessage!),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery!),
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

        private async Task<Message> BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            count+=1;
            List<Professor> professors = context.Professors.AsNoTracking().ToList();
            string s = "";
            if (int.Parse(callbackQuery.Data) is int)
            {
                for (int i = int.Parse(callbackQuery.Data); i < 50; i++)
                {

                    s += i + 1 + " " + professors[i].FirstName + professors[i].LastName + professors[i].Patronymic + "\n";
                }
                await _botClient.EditMessageTextAsync(callbackQuery.InlineMessageId, s);
            }
            return await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "fewfew");
        }

        private async Task BotOnMessageReceived(Message message)
        {
            if (message.Type != MessageType.Text)
                return;
            var action = message.Text switch
            {
                "/start" => SendStartButtonsAsync(_botClient, message),
                "Читать отзывы" => ShowProfessorList(_botClient, message,1),
                //"/keyboard" => SendReplyKeyboard(_botClient, message),
                //"/remove" => RemoveKeyboard(_botClient, message),
                //"/photo" => SendFile(_botClient, message),
                //"/request" => RequestContactAndLocation(_botClient, message),
                _ => Usage(_botClient, message)
            };
            Message sentMessage = await action;
        }

        private async Task<Message> ShowProfessorList(ITelegramBotClient botClient, Message message,int? count)
        {
            InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("<<", "0"),
                        InlineKeyboardButton.WithCallbackData(">>", $"{20*count}"),
                    }
                });
            List<Professor> professors = context.Professors.AsNoTracking().ToList();
            string s = "";
            for (int i = 0; i < 20; i++)
            {

                s += i+1 +" "+ professors[i].FirstName + professors[i].LastName + professors[i].Patronymic + "\n";
            } 
            return await _botClient.SendTextMessageAsync(message.Chat.Id, s,replyMarkup:inlineKeyboard);
            
        }

        private async Task<Message> Usage(ITelegramBotClient bot, Message message)
        {
            //InlineKeyboardMarkup inlineKeyboard = new(
            //    new[]
            //    {
            //        // first row
            //        new []
            //        {
            //            InlineKeyboardButton.WithCallbackData("1.1", "11"),
            //            InlineKeyboardButton.WithCallbackData("1.2", "12"),
            //        },
            //        // second row
            //        new []
            //        {
            //            InlineKeyboardButton.WithCallbackData("2.1", "21"),
            //            InlineKeyboardButton.WithCallbackData("2.2", "22"),
            //        },
            //    });

            //return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
            //                                      text: "Choose",
            //                                      replyMarkup: inlineKeyboard);
            //ReplyKeyboardMarkup keyboardMarkup = new ReplyKeyboardMarkup(new[]
            //{
            //    new KeyboardButton[]{"Читать отзывы","Оставить отзыв"}
            //})
            //{ ResizeKeyboard = true};
            return await bot.SendTextMessageAsync(message.Chat.Id,"123");
        }

        private async Task<Message> SendStartButtonsAsync(ITelegramBotClient bot, Message message)
        {
            ReplyKeyboardMarkup keyboardMarkup = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[]{"Читать отзывы","Оставить отзыв"}
            })
            { ResizeKeyboard = true};
            return await bot.SendTextMessageAsync(message.Chat.Id, text: "Выберите", replyMarkup: keyboardMarkup);
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

        private async Task BotOnInlineQueryReceived(InlineQuery inlineQuery)
        {
            InlineQueryResult[] results = {
            // displayed result
            new InlineQueryResultArticle(
                id: "3",
                title: "TgBots",
                inputMessageContent: new InputTextMessageContent(
                    "hello"
                ))};
            await _botClient.AnswerInlineQueryAsync(inlineQueryId: inlineQuery.Id,
                                                results: results,
                                                isPersonal: true,
                                                cacheTime: 0);

        }

    }
}
