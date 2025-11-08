using bot.Core.Entities;
using bot.Core.Services.Classes;
using bot.Core.Services.Interfaces;
using bot.Infrastructure.DataAccess;
using bot.TelegramBot.Scenarios;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
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

                BotCommand[] commands =
                [
                    new() { Command = "start", Description = "Старт" },
                    new() { Command = "cancel", Description = "Отменить команду" },
                    new() { Command = "addtask", Description = "Добавить (укажите имя через пробел)" },
                    new() { Command = "show", Description = "Активные" },
                    new() { Command = "find", Description = "Активные по префиксу (укажите через пробел)" },
                    new() { Command = "removetask", Description = "Удалить по GUID (укажите через пробел)" },
                    new() { Command = "completetask", Description = "Завершить по GUID (укажите через пробел)" },
                    new() { Command = "report", Description = "Статистика" },
                    new() { Command = "info", Description = "Версия и дата создания" }
                ];

                await botClient.SetMyCommands(commands);

                FileUserRepository fileUserRepository = new("UserRepository");
                UserService userService = new(fileUserRepository);

                FileToDoRepository fileToDoRepository = new("ToDoRepository");

                ToDoService toDoService = new(fileToDoRepository);

                ToDoReportService toDoReportService = new(fileToDoRepository);

                using CancellationTokenSource cts = new();

                IEnumerable<IScenario> scenerios =
                [
                    new AddTaskScenario(userService, toDoService)
                ];

                InMemoryScenarioContextRepository contextRepository = new();


                FileToDoListRepository fileToDoListRepository = new("ToDoListRepository");
                ToDoListService toDoListService = new(fileToDoListRepository);


                UpdateHandler updateHandler = new(userService, toDoService, toDoReportService, scenerios, contextRepository, toDoListService, cts.Token);

                static void startedHandler(string msg) => Console.WriteLine($"Началась обработка сообщения '{msg}'");
                static void completedHandler(string msg) => Console.WriteLine($"Закончилась обработка сообщения '{msg}'");

                try
                {
                    updateHandler.SubscribeUpdateStarted(startedHandler);
                    updateHandler.SubscribeUpdateCompleted(completedHandler);
                    
                    var receiverOptions = new ReceiverOptions
                    {
                        AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery],
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

