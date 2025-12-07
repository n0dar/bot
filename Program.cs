using bot.Core.Services.Classes;
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
                    new() { Command = "addtask", Description = "Добавить задачу" },
                    new() { Command = "report", Description = "Статистика" },
                    new() { Command = "info", Description = "Версия и дата создания" }
                ];

                await botClient.SetMyCommands(commands);

                //DataContextFactory dataContextFactory = new("Host=localhost;Port=5432;Database=ToDoList;Username=postgres;Password=81828516;Pooling=true");
                DataContextFactory dataContextFactory = new("");

                SqlUserRepository sqlUserRepository = new(dataContextFactory);
                UserService userService = new(sqlUserRepository);

                SqlToDoRepository sqlToDoRepository = new(dataContextFactory);
                ToDoService toDoService = new(sqlToDoRepository);
                ToDoReportService toDoReportService = new(sqlToDoRepository);

                SqlToDoListRepository sqlToListDoRepository = new(dataContextFactory);
                ToDoListService toDoListService = new(sqlToListDoRepository);

                IEnumerable<IScenario> scenerios =
                [
                    new AddTaskScenario(userService, toDoListService, toDoService),
                    new AddListScenario(userService, toDoListService),
                    new DeleteListScenario(userService, toDoListService, toDoService)
                ];
                InMemoryScenarioContextRepository contextRepository = new();

                using CancellationTokenSource cts = new();

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

