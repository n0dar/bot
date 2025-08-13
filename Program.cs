using bot.Core.DataAccess;
using bot.Core.Services;
using bot.Core.Services.Classes;
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
                botClient.StartReceiving(new UpdateHandler(new UserService(new InMemoryUserRepository()), new ToDoService(new InMemoryToDoRepository())));
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

