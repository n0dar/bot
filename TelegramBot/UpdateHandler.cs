#nullable enable
using bot.Core.Entities;
using bot.Core.Exceptions;
using bot.Core.Services.Interfaces;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace bot
{
    internal class UpdateHandler(IUserService UserService, IToDoService ToDoService, IToDoReportService ToDoReportService, CancellationToken CT) : IUpdateHandler
    {
        private readonly IUserService _userService = UserService;
        private readonly IToDoService _toDoService = ToDoService;
        private readonly IToDoReportService _toDoReportService = ToDoReportService;
        private readonly CancellationToken _ct = CT;

        public delegate void MessageEventHandler(string message);
        private event MessageEventHandler? OnHandleUpdateStarted;
        private event MessageEventHandler? OnHandleUpdateCompleted;
        public void SubscribeUpdateStarted(MessageEventHandler handler)
        {
            OnHandleUpdateStarted += handler;
        }
        public void UnsubscribeUpdateStarted(MessageEventHandler handler)
        {
            OnHandleUpdateStarted -= handler;
        }
        public void SubscribeUpdateCompleted(MessageEventHandler handler)
        {
            OnHandleUpdateCompleted += handler;
        }
        public void UnsubscribeUpdateCompleted(MessageEventHandler handler)
        {
            OnHandleUpdateCompleted -= handler;
        }
        private void RaiseHandleUpdateStarted(string message)
        {
            OnHandleUpdateStarted?.Invoke(message);
        }
        private void RaiseHandleUpdateCompleted(string message)
        {
            OnHandleUpdateCompleted?.Invoke(message);
        }
        private string GetMessageForShowCommands(IReadOnlyList<ToDoItem> toDoItemList, string command)
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
            botClient.SendMessage(update.Message.Chat, $"Статистика по задачам на {generatedAt}. Всего: {total}; Завершенных: {completed}; Активных: {active}", _ct);
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
                                                                                "/find         — покажу список актичных задач, начинающихся с префиса (укажите префикс через пробел);\r\n" +
                                                                                "/removetask   — удалю задачу из списка (укажите ее GUID через пробел);\r\n" +
                                                                                "/completetask — изменю статус задачи с \"Активна\" на \"Выполнена\" (укажите ее GUID через пробел);\r\n"  +
                                                                                "/report       — покажу статистику по задачам;\r\n"
                ) +
                                                                                "/help         — покажу справочную информацию;\r\n" +
                                                                                "/info         — покажу свои версию и дату создания;\r\n" +
                                                                                "Завершайте ввод нажатием на Enter.\r\n" +
                                                                                "Ctrl + C      — завершу работу",
                _ct
            );
        }
        private void Info(ITelegramBotClient botClient, Update update)
        {
            botClient.SendMessage(update.Message.Chat, "Версия — C.C.C, дата создания — DD.MM.YYYY", _ct);
        }
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            try
            {
                RaiseHandleUpdateStarted(update.Message.Text);
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
                        await botClient.SendMessage(update.Message.Chat, "Задача добавлена", ct);
                        break;
                    case "/showalltasks" when toDoUser != null:
                        await botClient.SendMessage(update.Message.Chat, GetMessageForShowCommands(_toDoService.GetAllByUserId(toDoUser.UserId), command), ct);
                        break;
                    case "/showtasks" when toDoUser != null:
                        await botClient.SendMessage(update.Message.Chat, GetMessageForShowCommands(_toDoService.GetActiveByUserId(toDoUser.UserId), command), ct);
                        break;
                    case "/find" when toDoUser != null && commandParam != null:
                        await botClient.SendMessage(update.Message.Chat, GetMessageForShowCommands(_toDoService.Find(toDoUser, commandParam), "/showtasks"), ct);
                        break;
                    case "/removetask" when toDoUser != null && commandParam != null:
                        if (Guid.TryParse(commandParam, out taskId))
                        {
                            _toDoService.Delete(taskId);
                            await botClient.SendMessage(update.Message.Chat, "Задача удалена", ct);
                        }
                        else await botClient.SendMessage(update.Message.Chat, "Некорректный идентификатор задачи", ct);
                        break;
                    case "/completetask" when toDoUser != null && commandParam != null:
                        if (Guid.TryParse(commandParam, out taskId))
                        {
                            _toDoService.MarkCompleted(taskId);
                            await botClient.SendMessage(update.Message.Chat, "Задача завершена", ct);
                        }
                        else await botClient.SendMessage(update.Message.Chat, "Некорректный идентификатор задачи", ct);
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
                await botClient.SendMessage(update.Message.Chat, "Жду вашу команду...", ct);
                RaiseHandleUpdateCompleted(update.Message.Text);
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(botClient, ex, ct);
                await botClient.SendMessage(update.Message.Chat, "Жду вашу команду...", ct);
                RaiseHandleUpdateCompleted(update.Message.Text);
            }
        }
        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
        {
            ConsoleColor prevColor = Console.ForegroundColor;
            if
            (
                exception is ArgumentException ||
                exception is TaskCountLimitException ||
                exception is TaskLengthLimitException ||
                exception is DuplicateTaskException
            )
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{exception.Message}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine
                (
                    $"Произошла непредвиденная ошибка:\r\n" +
                    $"{exception.GetType().FullName}\r\n" +
                    $"{exception.Message}\r\n" +
                    $"{exception.StackTrace}\r\n" +
                    $"{exception.InnerException}\r\n"
                );
            }
            Console.ForegroundColor = prevColor;
            return Task.CompletedTask;
        }
    }
}
