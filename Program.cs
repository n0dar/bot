using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

//using Otus.ToDoList.ConsoleBot.Types;
using System;
//using System.Collections.Generic;
//using static bot.ToDoItem;

namespace bot
{
    class Program
    {
        private static void Main()
        {
            ConsoleBotClient botClient = new();
            UserService UserService = new();
            ToDoService ToDoService = new();
            try
            {
                botClient.StartReceiving(new UpdateHandler(UserService, ToDoService));
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

