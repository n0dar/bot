#nullable enable
using bot.Core.DataAccess;
using bot.Core.Entities;
using bot.Core.Exceptions;
using bot.Core.Services.Interfaces;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace bot
{
    internal class UpdateHandler(IUserService UserService, IToDoService ToDoService, IToDoReportService ToDoReportService) : IUpdateHandler
    {
        private readonly IUserService _userService = UserService;
        private readonly IToDoService _toDoService = ToDoService;
        private readonly IToDoReportService _toDoReportService = ToDoReportService;
        
        private static string GetMessageForShowCommands(IReadOnlyList<ToDoItem> toDoItemList, string command)
        {
            StringBuilder message = new();
            if (toDoItemList.Count > 0)
            {
                message.AppendLine($"Список{(command == "/showtasks" ? " активных " : " ")}задач:");
                foreach (ToDoItem item in toDoItemList)
                {
                    message.AppendLine(item.ToString());
                }
            }
            else message.AppendLine($"Список{(command == "/showtasks" ? " активных " : " ")}задач пуст"); 
            return message.ToString();
        }
        private void Start(ITelegramBotClient botClient, Update update)
        {
            if (_userService.GetUser(update.Message.From.Id) is null)
            {
                _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username);
            }
        }
        private void Report(ITelegramBotClient botClient, Update update)
        {
            (int total, int completed, int active, DateTime generatedAt) = _toDoReportService.GetUserStats(_userService.GetUser(update.Message.From.Id).UserId);
            botClient.SendMessage(update.Message.Chat, $"Статистика по задачам на {generatedAt}. Всего: {total}; Завершенных: {completed}; Активных: {active}");
        }
        private void Help(ITelegramBotClient botClient, Update update)
        {
            botClient.SendMessage
            (
                update.Message.Chat,
                                                                                "Для взаимодействия со мной вам доступен следующий список команд:\r\n" +
                ((_userService.GetUser(update.Message.From.Id) == null) ?       "/start        — начните работу с этой команды;\r\n"
                                                                                :
                                                                                "/addtask      — добавлю задачу в список (укажите ее имя через пробел);\r\n" +
                                                                                "/showalltasks — покажу список всех задач;\r\n" +
                                                                                "/showtasks    — покажу список активных задач;\r\n" +
                                                                                "/removetask   — удалю задачу из списка (укажите ее GUID через пробел);\r\n" +
                                                                                "/completetask — изменю статус задачи с \"Активна\" на \"Выполнена\" (укажите ее GUID через пробел);\r\n"  +
                                                                                "/report       — покажу статистику по задачам;\r\n"
                ) +
                                                                                "/help         — покажу справочную информацию;\r\n" +
                                                                                "/info         — покажу свои версию и дату создания;\r\n" +
                                                                                "Завершайте ввод нажатием на Enter.\r\n" +
                                                                                "Ctrl + C      — завершу работу"
            );
        }
        private static void Info(ITelegramBotClient botClient, Update update)
        {
            botClient.SendMessage(update.Message.Chat, "Версия — 0.0.5, дата создания — 14.08.2025");
        }
        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            try
            {
                string command = update.Message.Text.Trim();
                string? commandParam = null;
                int spaceIndex = command.IndexOf(' ');
                if (spaceIndex >= 0 && spaceIndex < command.Length - 1)
                {
                    commandParam = command.Substring(spaceIndex + 1).Trim();
                    command = command.Substring(0, spaceIndex);
                }

                ToDoUser? toDoUser = _userService.GetUser(update.Message.From.Id);
                Guid taskId;
                
                switch (command)
                {
                    case "/start" when toDoUser == null:
                        Start(botClient, update);
                        Help(botClient, update);
                        break;
                    case "/addtask" when toDoUser != null && commandParam != null:
                        _toDoService.Add(toDoUser, commandParam);
                        botClient.SendMessage(update.Message.Chat, "Задача добавлена");
                        break;
                    case "/showalltasks" when toDoUser != null:
                        botClient.SendMessage(update.Message.Chat, GetMessageForShowCommands(_toDoService.GetAllByUserId(toDoUser.UserId), command));
                        break;
                    case "/showtasks" when toDoUser != null:
                        botClient.SendMessage(update.Message.Chat, GetMessageForShowCommands(_toDoService.GetActiveByUserId(toDoUser.UserId), command));
                        break;
                    case "/removetask" when toDoUser != null && commandParam != null:
                        if (Guid.TryParse(commandParam, out taskId))
                        {
                            _toDoService.Delete(taskId);
                            botClient.SendMessage(update.Message.Chat, "Задача удалена");
                        }
                        else botClient.SendMessage(update.Message.Chat, "Некорректный идентификатор задачи");
                        break;
                    case "/completetask" when toDoUser != null && commandParam != null:
                        if (Guid.TryParse(commandParam, out taskId))
                        {
                            _toDoService.MarkCompleted(taskId);
                            botClient.SendMessage(update.Message.Chat, "Задача завершена");
                        }
                        else botClient.SendMessage(update.Message.Chat, "Некорректный идентификатор задачи");
                        break;
                    case "/report":
                        Report(botClient, update);
                        break;
                    case "/help":
                        Help(botClient, update);
                        break;
                    case "/info":
                        Info(botClient, update);
                        break;
                    default:
                        Help(botClient, update);
                        break;
                }
                botClient.SendMessage(update.Message.Chat, "Жду вашу команду...");
            }
            catch (Exception ex)
            when
            (
                ex is ArgumentException ||
                ex is TaskCountLimitException ||
                ex is TaskLengthLimitException ||
                ex is DuplicateTaskException
            )
            {
                botClient.SendMessage(update.Message.Chat, $"{ex.Message}");
            }
            catch (Exception ex)
            {
                botClient.SendMessage(update.Message.Chat,
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
