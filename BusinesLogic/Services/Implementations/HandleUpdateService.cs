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

namespace BusinesLogic.Services.Implementations
{
    public class HandleUpdateService : IHandleUpdateService
    {
        int PageNumber = 1;
        bool readBtnIsChecked = false;
        bool writeBtnIsChecked = false;
        bool DataIsFilled = false;
        bool IsReadyForWritingFeedback = false;
        Professor SelectedProfessor;
        private readonly ApplicationDatabaseContext _context;
        private ITelegramBotClient _botClient;
        private readonly IProfessorsService _professorsSrvice;
        private readonly IFeedbackService _feedbackService;
        public HandleUpdateService(ApplicationDatabaseContext context,
            IProfessorsService professorsSrvice,
            IFeedbackService feedbackService)
        {
            _context = context;
            _professorsSrvice = professorsSrvice;
            _feedbackService = feedbackService;
        }
        public async Task EchoAsync(Update update, ITelegramBotClient client)
        {
            _botClient = client;
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(update.Message!),
                UpdateType.EditedMessage => BotOnMessageReceived(update.EditedMessage!),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery!),
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

            if (callbackQuery.Data == "<<")
            {
                var action = BackPage(_botClient, callbackQuery);
                Message sentMessage = await action;
                return sentMessage;

            }
            if (callbackQuery.Data == ">>")
            {
                var action = NextPage(_botClient, callbackQuery);
                Message sentMessage = await action;
                return sentMessage;
            }
            return null;

            async Task<Message> BackPage(ITelegramBotClient bot, CallbackQuery callbackQuery)
            {
                readBtnIsChecked = true;
                string text;
                if (PageNumber > 1)
                {
                    InlineKeyboardMarkup inlineKeyboard = new(
                    new[]
                    {
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("<<", "<<"),
                            InlineKeyboardButton.WithCallbackData(">>", ">>"),
                        }
                    });
                    if (DataIsFilled)
                    {
                        PageNumber -= 1;
                        text = _feedbackService.Get(SelectedProfessor.Id, PageNumber);
                        return await _botClient.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, text, replyMarkup: inlineKeyboard);

                    }

                    PageNumber -= 1;
                    text = _professorsSrvice.GetProfessors(PageNumber);
                    return await _botClient.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, text, replyMarkup: inlineKeyboard);
                }
                return null;
            }
            async Task<Message> NextPage(ITelegramBotClient bot, CallbackQuery callbackQuery)
            {
                readBtnIsChecked = true;
                string text;
                InlineKeyboardMarkup inlineKeyboard = new(
                    new[]
                    {
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("<<", "<<"),
                            InlineKeyboardButton.WithCallbackData(">>", ">>"),
                        }
                    });
                if (DataIsFilled && PageNumber < _feedbackService.GetCountOfPages())
                {
                    PageNumber += 1;
                    text = _feedbackService.Get(SelectedProfessor.Id, PageNumber);
                    return await _botClient.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, text, replyMarkup: inlineKeyboard);
                }

                if (PageNumber < CheckNumOfPages())
                {
                    PageNumber += 1;
                    text = _professorsSrvice.GetProfessors(PageNumber);
                    return await _botClient.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, text, replyMarkup: inlineKeyboard);
                }
                return null;
            }
            int CheckNumOfPages()
            {
                return Convert.ToInt32(Math.Round((double)_professorsSrvice.GetCountOfProfessors() / 20));
            }
        }


        private async Task BotOnMessageReceived(Message message)
        {
            if (message.Type != MessageType.Text)
                return;
            var action = message.Text switch
            {
                "/start" => SendStartButtonsAsync(_botClient, message),
                "Читать отзывы" => ShowProfessorList(_botClient, message),
                "Оставить отзыв" => ShowProfessorList(_botClient, message),
                "Меню" => SendStartButtonsAsync(_botClient, message),
                //"Назад" => RemoveKeyboard(_botClient, message),
                //"/photo" => SendFile(_botClient, message),
                //"/request" => RequestContactAndLocation(_botClient, message),
                _ => Usage(_botClient, message)
            };
            Message sentMessage = await action;

            async Task<Message> SendStartButtonsAsync(ITelegramBotClient bot, Message message)
            {
                DataIsFilled = false;
                readBtnIsChecked = false;
                writeBtnIsChecked = false;
                ReplyKeyboardMarkup keyboardMarkup = new ReplyKeyboardMarkup(new[]
                {
                new KeyboardButton[]{"Читать отзывы","Оставить отзыв"}
                })
                { ResizeKeyboard = true };
                return await bot.SendTextMessageAsync(message.Chat.Id, text: "Выберите", replyMarkup: keyboardMarkup);
            }

            async Task<Message> Usage(ITelegramBotClient bot, Message message)
            {
                if (writeBtnIsChecked)
                {
                    if (IsReadyForWritingFeedback)
                    {
                        var IsCreated = _feedbackService.Create(message, SelectedProfessor.Id);
                    }
                    var professor = _professorsSrvice.Get(message.Text);
                    SelectedProfessor = professor;
                    if (professor == null)
                    {
                        return await bot.SendTextMessageAsync(message.Chat.Id, "Преподавалель не найден.");
                    }
                    else
                    {
                        string caption = $"{professor.LastName} {professor.FirstName} {professor.Patronymic} \n" +
                            $"Рейтинг: {_feedbackService.GetRaiting(professor.Id)}";
                        await bot.SendPhotoAsync(message.Chat.Id, photo: professor.PhotoPath, caption: caption);
                        string feedbackExample = "--Шаблон отзыва--\n" +
                            "Лекции: \n" +
                            "Практические занятия:\n" +
                            "Конспект:\n" +
                            "Зачёт\\экзамен:\n" +
                            "Общее впечатление:\n";
                        return await bot.SendTextMessageAsync(message.Chat.Id, feedbackExample);

                    }

                }
                if (readBtnIsChecked)
                {
                    var professor = _professorsSrvice.Get(message.Text);
                    if (professor == null)
                    {
                        return await bot.SendTextMessageAsync(message.Chat.Id, "Преподавалель не найден.");
                    }
                    else
                    {
                        if (professor.Feedbacks.Count == 0)
                        {
                            return await bot.SendTextMessageAsync(message.Chat.Id, "Не найдено отзывов о преподавателе.");
                        }
                        DataIsFilled = true;
                        SelectedProfessor = professor;
                        ReplyKeyboardMarkup keyboardMarkup = new ReplyKeyboardMarkup(new[]
                        {
                        new KeyboardButton[]{"Меню"}
                        })
                        { ResizeKeyboard = true };
                        string caption = $"{professor.LastName} {professor.FirstName} {professor.Patronymic} \n" +
                            $"Рейтинг: {_feedbackService.GetRaiting(professor.Id)}";
                        await bot.SendPhotoAsync(message.Chat.Id, photo: professor.PhotoPath, caption: caption, replyMarkup: keyboardMarkup);

                        return await ShowFeedbackList(bot, message, professor.Id);
                    }
                }
                return await bot.SendTextMessageAsync(message.Chat.Id, "123");
            }
        }
        private async Task<Message> ShowFeedbackList(ITelegramBotClient botClient, Message message, int professorId)
        {
            PageNumber = 1;
            InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("<<", "<<"),
                        InlineKeyboardButton.WithCallbackData(">>", ">>"),
                    }
                });

            string text = _feedbackService.Get(professorId, PageNumber);
            return await _botClient.SendTextMessageAsync(message.Chat.Id, text, replyMarkup: inlineKeyboard);

        }
        private async Task<Message> ShowProfessorList(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "Читать отзывы")
                readBtnIsChecked = true;
            else
                writeBtnIsChecked = true;
            PageNumber = 1;
            InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("<<", "<<"),
                        InlineKeyboardButton.WithCallbackData(">>", ">>"),
                    }
                });
            string text = _professorsSrvice.GetProfessors(PageNumber);
            await _botClient.SendTextMessageAsync(message.Chat.Id, text, replyMarkup: inlineKeyboard);
            return await _botClient.SendTextMessageAsync(message.Chat.Id, "Введите фимилию и имя преподавателя");

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

    }
}
