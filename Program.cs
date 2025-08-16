using bot.Core.DataAccess;
using bot.Core.Services;
using bot.Core.Services.Classes;
using bot.Core.Services.Interfaces;
using bot.Infrastructure.DataAccess;
using Otus.ToDoList.ConsoleBot;
using System;

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
                UserService userService = new UserService(inMemoryUserRepository);

                InMemoryToDoRepository inMemoryToDoRepository = new();
                ToDoService toDoService = new(inMemoryToDoRepository);

                ToDoReportService toDoReportService = new(inMemoryToDoRepository);

                UpdateHandler updateHandler = new UpdateHandler(userService, toDoService, toDoReportService);

                botClient.StartReceiving(updateHandler);
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

