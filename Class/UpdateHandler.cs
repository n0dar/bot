#nullable enable
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace bot
{
    internal class UpdateHandler: IUpdateHandler
    {
        private readonly IUserService _IUserService;
        private readonly IToDoService _IToDoService;

        private static string ReadLine()
        {
            string? res = Console.ReadLine();
            if ((res ?? "").Trim() == "") throw new ArgumentException("Значение не может быть пустым");
            return res;
        }
        private void Start(ITelegramBotClient botClient, Update update)
        {
            if (_IUserService.GetUser(update.Message.From.Id) is null)
            {
                _IUserService.RegisterUser(update.Message.From.Id, update.Message.From.Username);
            }
        }
        private void Help(ITelegramBotClient botClient, Update update)
        {
            botClient.SendMessage
            (
                update.Message.Chat,
                                                                                "Для взаимодействия со мной вам доступен следующий список команд:\r\n" +
                ((_IUserService.GetUser(update.Message.From.Id) != null) ? "" : "/start        — начните работу с этой команды;\r\n") +
                ((_IUserService.GetUser(update.Message.From.Id) == null) ? "" : "/addtask      — добавлю задачу в список (укажите ее имя через пробел);\r\n" +
                                                                                "/showalltasks — покажу список всех задач;\r\n"  +
                                                                                "/showtasks    — покажу список активных задач;\r\n"  +
                                                                                "/removetask   — удалю задачу из списка (укажите ее GUID через пробел);\r\n" +
                                                                                "/completetask — изменю статус задачи с \"Активна\" на \"Выполнена\" (укажите ее GUID через пробел);\r\n"
                ) +
                                                                                "/help         — покажу справочную информацию;\r\n" +
                                                                                "/info         — покажу свои версию и дату создания;\r\n" +
                                                                                "Завершайте ввод нажатием на Enter.\r\n" +
                                                                                "Ctrl + C      — завершу работу"
            );
        }
        private void Info(ITelegramBotClient botClient, Update update)
        {
            botClient.SendMessage(update.Message.Chat, "Версия — 0.0.4, дата создания — 11.08.2025");
        }
        public UpdateHandler(IUserService UserService, IToDoService ToDoService)
        {
            _IUserService = UserService;
            _IToDoService = ToDoService;
        }
        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            try
            {
                if (_IToDoService.TaskCountLimit == 0)
                {
                    botClient.SendMessage(update.Message.Chat, "Привет! Я — бот.\r\nВведите максимально допустимое количество задач...");
                    _ = int.TryParse(ReadLine(), out int res);
                    _IToDoService.TaskCountLimit = res;
                }

                if (_IToDoService.TaskLengthLimit == 0)
                {
                    botClient.SendMessage(update.Message.Chat, "Введите максимально допустимую длину наименования задачи...");
                    _ = int.TryParse(ReadLine(), out int res);
                    _IToDoService.TaskLengthLimit = res;
                }

                string command = update.Message.Text.Trim();
                string? commandParam = null;
                int spaceIndex = command.IndexOf(' ');
                if (spaceIndex >= 0 && spaceIndex < command.Length - 1)
                {
                    commandParam = command.Substring(spaceIndex + 1).Trim();
                    command = command.Substring(0, spaceIndex);
                }

                ToDoUser? toDoUser = _IUserService.GetUser(update.Message.From.Id);
                IReadOnlyList<ToDoItem> toDoItemList;
                Guid taskId;
                StringBuilder message = new();

                switch (command)
                {
                    case "/start" when toDoUser == null:
                        Start(botClient, update);
                        Help(botClient, update);
                        break;
                    case "/addtask" when toDoUser != null && commandParam != null:
                        _IToDoService.Add(toDoUser, commandParam);
                        botClient.SendMessage(update.Message.Chat, "Задача добавлена");
                        break;
                    case "/showalltasks" when toDoUser != null:
                        toDoItemList = _IToDoService.GetAllByUserId(toDoUser.TelegramUserId);
                        if (toDoItemList.Count > 0)
                        {
                            message.Clear();
                            message.AppendLine("Список задач:");
                            foreach (ToDoItem item in toDoItemList)
                            {
                                message.AppendLine($"({item.State}) {item.Name} - {item.CreatedAt} - {item.Id}");
                            }
                            botClient.SendMessage(update.Message.Chat, message.ToString());
                        }
                        else botClient.SendMessage(update.Message.Chat, "Список задач пуст");
                        break;
                    case "/showtasks" when toDoUser != null:
                        toDoItemList = _IToDoService.GetActiveByUserId(toDoUser.TelegramUserId);
                        if (toDoItemList.Count > 0)
                        {
                            message.Clear();
                            message.AppendLine("Список активных задач:"); 
                            foreach (ToDoItem item in toDoItemList)
                            {
                                message.AppendLine($"({item.State}) {item.Name} - {item.CreatedAt} - {item.Id}");
                            }
                            botClient.SendMessage(update.Message.Chat, message.ToString());
                        }
                        else botClient.SendMessage(update.Message.Chat, "Список активных задач пуст");
                        break;
                    case "/removetask" when toDoUser != null && commandParam != null:
                        if (Guid.TryParse(commandParam, out taskId))
                        {
                            _IToDoService.Delete(taskId);
                            botClient.SendMessage(update.Message.Chat, "Задача удалена");
                        }
                        else botClient.SendMessage(update.Message.Chat, "Некорректный идентификатор задачи");
                        break;
                    case "/completetask" when toDoUser != null && commandParam != null:
                        if (Guid.TryParse(commandParam, out taskId))
                        {
                            _IToDoService.MarkCompleted(taskId);
                            botClient.SendMessage(update.Message.Chat, "Задача завершена");
                        }
                        else botClient.SendMessage(update.Message.Chat, "Некорректный идентификатор задачи");
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
