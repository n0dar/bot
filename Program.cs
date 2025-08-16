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
                ToDoService toDoService = new(new InMemoryToDoRepository());
                botClient.StartReceiving(new UpdateHandler(new UserService(new InMemoryUserRepository()), toDoService, new ToDoReportService(toDoService)));
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

