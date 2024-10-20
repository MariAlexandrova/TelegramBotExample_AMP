using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using TelegramBotExample_AMP.Service;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;
using TelegramBotExample_AMP.Entity;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotExample_AMP.TgBotApi
{
    internal class Bot
    {
        // клиент-бот
        private ITelegramBotClient client;
        private CategoryService categoryService;
        private ProductService productService;
        // создание бота
        public Bot(string token, string connectionString)
        {
            client = new TelegramBotClient(token);
            categoryService = new CategoryService(connectionString);
            productService = new ProductService(connectionString);
        }
        // запуск бота (ожидание сообщений от пользователя)
        public void StartReceiving()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // получать все типы обновлений
            };

            client.StartReceiving(
                HandleUpdateAsync,
                HandlePollingErrorAsync,
                receiverOptions,
                cancellationToken: cancellationTokenSource.Token
            );
            Console.WriteLine($"Бот {client.GetMeAsync().Result.Username} запущен!");
            Console.ReadKey();
            cancellationTokenSource.Cancel();
        }
        private async Task DeleteMessageSafeAsync(long chatId, int messageId)
        {
            try
            {

                await client.DeleteMessageAsync(chatId, messageId);
            }
            catch (ApiRequestException ex) when (ex.ErrorCode == 400 && ex.Message.Contains("message to delete not found")) { }
            catch (Exception ex)
            {
                Console.WriteLine($"произошла ошибка при удалении сообщения: {ex.Message}");
            }
        }
        // главный метод - точка входа каждого обращения к боту
        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            //Запуск
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                var message = update.Message;
                // Получаем ник пользователя и выводим в консоль
                string username = message.From.Username ?? "Неизвестный пользователь";
                Console.WriteLine($"Пользователь {username} отправил сообщение: {message.Text}");
                if (message.Text == "/start")
                {
                    var startKeyboard = CreateStartKeyboard();


                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "👋 Приветствуем Вас в нашем магазине цифровых версий игр для Play Station - Play Sphere! 💫" +
                        "\n🚀 Нажмите /start для начала",
                        replyMarkup: startKeyboard,
                        cancellationToken: cancellationToken
                    );
                }
                if (message.Text.StartsWith("/search"))
                {
                    if (message.Text.Length > 8)
                    {
                        var nameOfProduct = message.Text.Substring(8);
                        var products = productService.SearchProductsByName(nameOfProduct);
                        if (products.Any())
                        {
                            var keyboardButtons = products.Select(product => new[] { InlineKeyboardButton.WithUrl($"🎮 {product.Name}", product.Url) }).ToList();
                            keyboardButtons.Add(new[] { InlineKeyboardButton.WithCallbackData("↩️ Вернуться", "back_to_buy") });
                            var productKeyboard = new InlineKeyboardMarkup(keyboardButtons);

                            await client.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: $"✨ Результаты поиска по запросу: {nameOfProduct}",
                                replyMarkup: productKeyboard,
                                cancellationToken: cancellationToken
                            );
                        }
                        else
                        {
                            var keyboardButton = InlineKeyboardButton.WithCallbackData("↩️ Вернуться", "back_to_buy");
                            var backKeyboard = new InlineKeyboardMarkup(keyboardButton);

                            await client.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: $"По запросу \"{nameOfProduct}\" ничего не найдено 🤕\nПопробуйте другой запрос.",
                                replyMarkup: backKeyboard,
                                cancellationToken: cancellationToken
                            );
                        }
                    }
                    else
                    {
                        var keyboardButton = InlineKeyboardButton.WithCallbackData("↩️ Вернуться", "back_to_buy");
                        var backKeyboard = new InlineKeyboardMarkup(keyboardButton);

                        await client.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Неверный формат запроса 🤕\nПопробуйте другой запрос.",
                            replyMarkup: backKeyboard,
                            cancellationToken: cancellationToken
                        );
                        Console.WriteLine();
                    }
                    
                }
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                var callbackQuery = update.CallbackQuery;
                await HandleCallbackQuery(callbackQuery, cancellationToken);
            }
        }
        
        private InlineKeyboardMarkup CreateStartKeyboard()
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("/start", "start")
                }
            });
        }
        //Начальное меню
        private async Task HandleMainMenuCommand(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var mainMenuKeyboard=CreateMainMenuKeyboard();
            await client.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: "🔥 Play Sphere - бесконечные преключения в цифровом формате!",
                replyMarkup: mainMenuKeyboard,
                cancellationToken: cancellationToken
            );
        }
        private InlineKeyboardMarkup CreateMainMenuKeyboard()
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithUrl("📣 Наш новостной канал", "https://t.me/PlaySphereGames"),
                },
                new[]
                {
                    InlineKeyboardButton.WithUrl("❓ Задать вопрос", "https://t.me/plSphere_bot"),
                    InlineKeyboardButton.WithCallbackData("🛍️ Приобрести", "buy")
                },
                new[]
                {
                    InlineKeyboardButton.WithUrl("☎️ Связаться с оператором", "https://t.me/PlaySphereSupport")
                }
            });
        }
        //Меню кнопки Приобрести (выбор категории)
        private async Task HandleBuyCommand(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            try
            {
                var categories = categoryService.GetAllCategories();
                var keyboardButtons = categories.Select(category => new[] { InlineKeyboardButton.WithCallbackData($"⭐️ {category.Name}", $"category_{category.Id}") }).ToList();
                keyboardButtons.Add(new[] { InlineKeyboardButton.WithCallbackData("🔎 Поиск", "search") });
                keyboardButtons.Add(new[] { InlineKeyboardButton.WithCallbackData("↩️ Вернуться", "back_to_main") });
                var categoryKeyboard = new InlineKeyboardMarkup(keyboardButtons);

                await client.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: "⚡️ Выберите категорию:",
                    replyMarkup: categoryKeyboard,
                    cancellationToken: cancellationToken
                );
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.ToString());
                throw ex;
            }
            
        }
        //После выбора категории  
        private async Task HandleProductCommand(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var categoryId = int.Parse(callbackQuery.Data.Split('_')[1]);
            var products = productService.GetProductsByCategory(categoryId);
            var keyboardButtons = products.Select(product => new[] { InlineKeyboardButton.WithUrl($"🎮 {product.Name}", product.Url) }).ToList();
            keyboardButtons.Add(new[] { InlineKeyboardButton.WithCallbackData("↩️ Вернуться", "back_to_buy") });
            var productKeyboard = new InlineKeyboardMarkup(keyboardButtons);

            await client.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: "🔎 Выберите товар из предложенного списка\n\n" +
                    "💡 Если в данном списке нет желаемого товара, то напишите нашему оператору " +
                    "@PlaySphereSupport",
                    replyMarkup: productKeyboard,
                    cancellationToken: cancellationToken
            );
        }
        private async Task HandleSearchCommand(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if(callbackQuery.Data=="search") 
            {
                var keyboardButton= InlineKeyboardButton.WithCallbackData("↩️ Вернуться", "back_to_buy");
                var backKeyboard = new InlineKeyboardMarkup(keyboardButton);

                await client.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: "✏️ Пожалуйста, укажите название товара для поиска. \n✅ Пример: /search UFC 4",
                    replyMarkup: backKeyboard,
                    cancellationToken: cancellationToken
                );

                return;
            }
            

        }
        
        private async Task HandleCallbackQuery(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Data.StartsWith("category_"))
            {
                await DeleteMessageSafeAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);

                //await client.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, cancellationToken);
                HandleProductCommand(callbackQuery, cancellationToken);
            }
            
            switch (callbackQuery.Data)
            {
                case "start":
                    await DeleteMessageSafeAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    //await client.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, cancellationToken);
                    HandleMainMenuCommand(callbackQuery, cancellationToken);

                    break;
                //Приобрести ->
                case "buy":
                    await DeleteMessageSafeAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    //await client.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, cancellationToken);
                    HandleBuyCommand(callbackQuery, cancellationToken);
                    break;
                //Вернуться ->
                case "back_to_main":
                    await DeleteMessageSafeAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    //await client.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, cancellationToken);
                    HandleMainMenuCommand(callbackQuery, cancellationToken);    
                    break;
                //Вернуться на меню с покупкой ->
                case "back_to_buy":
                    await DeleteMessageSafeAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    //await client.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, cancellationToken);
                    HandleBuyCommand(callbackQuery, cancellationToken);
                    break;
                case "search":
                    await DeleteMessageSafeAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    //await client.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, cancellationToken);

                    HandleSearchCommand(callbackQuery, cancellationToken);
                    break;
            } 
        }
        //Ошибки
        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            string errorMessage;

            if (exception is ApiRequestException apiRequestException)
            {
                errorMessage = $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}";
            }
            else
            {
                errorMessage = exception.ToString();
            }
            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }
    }
}
