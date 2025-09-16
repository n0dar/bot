using bot.Core.Services.Classes;
using bot.Infrastructure.DataAccess;

using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace bot
{
    class Program
    {
        static async Task Main()
        {
            try
            {
                TelegramBotClient botClient = new(Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN", EnvironmentVariableTarget.User));

                InMemoryUserRepository inMemoryUserRepository = new();
                UserService userService = new(inMemoryUserRepository);

                InMemoryToDoRepository inMemoryToDoRepository = new();
                ToDoService toDoService = new(inMemoryToDoRepository);

                ToDoReportService toDoReportService = new(inMemoryToDoRepository);

                using CancellationTokenSource cts = new();

                UpdateHandler updateHandler = new(userService, toDoService, toDoReportService, cts.Token);

                static void startedHandler(string msg) => Console.WriteLine($"Началась обработка сообщения '{msg}'");
                static void completedHandler(string msg) => Console.WriteLine($"Закончилась обработка сообщения '{msg}'");

                try
                {
                    updateHandler.SubscribeUpdateStarted(startedHandler);
                    updateHandler.SubscribeUpdateCompleted(completedHandler);
                    
                    var receiverOptions = new ReceiverOptions
                    {
                        AllowedUpdates = [UpdateType.Message],
                        DropPendingUpdates = true
                    };
                    botClient.StartReceiving(updateHandler, receiverOptions);
                    var me = await botClient.GetMe();
                    Console.WriteLine($"{me.FirstName} запущен!");
                    Console.WriteLine("Нажмите клавишу A для выхода");

                    ConsoleKeyInfo keyInfo;
                    do
                    {
                        keyInfo = Console.ReadKey(intercept: true);
                        if (keyInfo.Key == ConsoleKey.A)
                        {
                            cts.Cancel();
                            break;
                        }   
                        else Console.WriteLine(me);
                    }
                    while (true);
                }
                finally
                {
                    updateHandler.UnsubscribeUpdateStarted(startedHandler);
                    updateHandler.UnsubscribeUpdateCompleted(completedHandler);
                }
            }
            catch (Exception ex)
            {
                ConsoleColor prevColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine
                (
                    $"Произошла непредвиденная ошибка:\r\n" +
                    $"{ex.GetType().FullName}\r\n" +
                    $"{ex.Message}\r\n" +
                    $"{ex.StackTrace}\r\n" +
                    $"{ex.InnerException}\r\n"
                );
                Console.ForegroundColor = prevColor;
            }
        }
    }
}

