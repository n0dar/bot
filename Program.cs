using bot.Core.DataAccess;
using bot.Core.Services;
using bot.Core.Services.Classes;
using bot.Core.Services.Interfaces;
using bot.Infrastructure.DataAccess;
using Otus.ToDoList.ConsoleBot;
using System;
using System.Threading;

namespace bot
{
    class Program
    {
        private static void Main()
        {
            try
            {
                ConsoleBotClient botClient = new();

                InMemoryUserRepository inMemoryUserRepository = new();
                UserService userService = new(inMemoryUserRepository);

                InMemoryToDoRepository inMemoryToDoRepository = new();
                ToDoService toDoService = new(inMemoryToDoRepository);

                ToDoReportService toDoReportService = new(inMemoryToDoRepository);

                using CancellationTokenSource cts = new();

                UpdateHandler updateHandler = new(userService, toDoService, toDoReportService, cts.Token);

                botClient.StartReceiving(updateHandler, cts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine
                (
                    $"Произошла непредвиденная ошибка:\r\n" +
                    $"{ex.GetType().FullName}\r\n" +
                    $"{ex.Message}\r\n" +
                    $"{ex.StackTrace}\r\n" +
                    $"{ex.InnerException}\r\n"
                );
            }
        }
    }
}

