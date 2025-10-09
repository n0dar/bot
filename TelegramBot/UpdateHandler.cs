#nullable enable
using bot.Core.Entities;
using bot.Core.Exceptions;
using bot.Core.Services.Interfaces;
using bot.TelegramBot;
using bot.TelegramBot.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace bot
{
    internal class UpdateHandler(IUserService UserService, IToDoService ToDoService, IToDoReportService ToDoReportService, IEnumerable<IScenario> Scenarios, IScenarioContextRepository ContextRepository, CancellationToken CT) : IUpdateHandler
    {
        private ITelegramBotClient _botClient;
        private readonly IUserService _userService = UserService;
        private readonly IToDoService _toDoService = ToDoService;
        private readonly IToDoReportService _toDoReportService = ToDoReportService;
        private readonly CancellationToken _ct = CT;

        public delegate void MessageEventHandler(string message);
        private event MessageEventHandler? OnHandleUpdateStarted;
        private event MessageEventHandler? OnHandleUpdateCompleted;
        private IScenario GetScenario(ScenarioType scenario)
        {
            return Scenarios.FirstOrDefault(s => s.CanHandle(scenario)) ?? throw new ScenarioDoesNotFound(scenario);
        }
        private async Task ProcessScenario(ScenarioContext context, Update update, CancellationToken ct)
        {
            IScenario scenario = GetScenario(context.CurrentScenario);
            ScenarioResult ScenarioResult = await scenario.HandleMessageAsync(_botClient, context, update, ct);

            if (ScenarioResult == ScenarioResult.Completed) await ContextRepository.ResetContext(update.Message.From.Id, ct);
            else await ContextRepository.SetContext(update.Message.From.Id, context, ct);
        }
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
        private async Task Start(Update update)
        {
            if (await _userService.GetUserAsync(update.Message.From.Id, _ct) == null)
            {
               await _userService.RegisterUserAsync(update.Message.From.Id, update.Message.From.Username, _ct);
            }
        }
        private async Task Report(Update update)
        {
            ToDoUser user = await  _userService.GetUserAsync(update.Message.From.Id, _ct);
            (int total, int completed, int active, DateTime generatedAt) = await _toDoReportService.GetUserStatsAsync(user.UserId, _ct);
            await _botClient.SendMessage(update.Message.Chat.Id, $"Статистика по задачам на {generatedAt}. Всего: {total}; Завершенных: {completed}; Активных: {active}", Telegram.Bot.Types.Enums.ParseMode.None, cancellationToken: _ct);
        }
        private void Info(Update update)
        {
            _botClient.SendMessage(update.Message.Chat.Id, "Версия — C.C.C, дата создания — DD.MM.YYYY", Telegram.Bot.Types.Enums.ParseMode.None, cancellationToken: _ct);
        }
        async Task IUpdateHandler.HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                RaiseHandleUpdateStarted(update.Message.Text);
                _botClient = botClient;
                string command = update.Message.Text.Trim();
                string? commandParam = null;
                int spaceIndex = command.IndexOf(' ');
                if (spaceIndex >= 0 && spaceIndex < command.Length - 1)
                {
                    commandParam = command[(spaceIndex + 1)..].Trim();
                    command = command[..spaceIndex];
                }
                ScenarioContext? scenarioContext = await ContextRepository.GetContext(update.Message.From.Id, cancellationToken);
                if (scenarioContext != null && command!="/cancel") await ProcessScenario(scenarioContext, update, cancellationToken);
                else
                {
                    ToDoUser? toDoUser = await _userService.GetUserAsync(update.Message.From.Id, _ct);
                    Guid taskId;

                    switch (command)
                    {
                        case "/start" when toDoUser == null:
                            await Start(update);
                            break;
                        case "/addtask" when toDoUser != null:
                            await ProcessScenario(new ScenarioContext(ScenarioType.AddTask), update, cancellationToken);
                        break;
                        case "/showalltasks" when toDoUser != null:
                            await botClient.SendMessage(update.Message.Chat.Id, GetMessageForShowCommands(await _toDoService.GetAllByUserIdAsync(toDoUser.UserId, cancellationToken), command), Telegram.Bot.Types.Enums.ParseMode.None, cancellationToken: cancellationToken);
                            break;
                        case "/showtasks" when toDoUser != null:
                            await botClient.SendMessage(update.Message.Chat.Id, GetMessageForShowCommands(await _toDoService.GetActiveByUserIdAsync(toDoUser.UserId, cancellationToken), command), Telegram.Bot.Types.Enums.ParseMode.None, cancellationToken: cancellationToken);
                            break;
                        case "/find" when toDoUser != null && commandParam != null:
                            await botClient.SendMessage(update.Message.Chat.Id, GetMessageForShowCommands(await _toDoService.FindAsync(toDoUser, commandParam, cancellationToken), "/showtasks"), Telegram.Bot.Types.Enums.ParseMode.None, cancellationToken: cancellationToken);
                            break;
                        case "/removetask" when toDoUser != null && commandParam != null:
                            if (Guid.TryParse(commandParam, out taskId))
                            {
                                await _toDoService.DeleteAsync(taskId, _ct);
                                await botClient.SendMessage(update.Message.Chat.Id, "Задача удалена", Telegram.Bot.Types.Enums.ParseMode.None, cancellationToken: cancellationToken);
                            }
                            else await botClient.SendMessage(update.Message.Chat.Id, "Некорректный идентификатор задачи", Telegram.Bot.Types.Enums.ParseMode.None, cancellationToken: cancellationToken);
                            break;
                        case "/completetask" when toDoUser != null && commandParam != null:
                            if (Guid.TryParse(commandParam, out taskId))
                            {
                                await _toDoService.MarkCompletedAsync(taskId, cancellationToken);
                                await botClient.SendMessage(update.Message.Chat.Id, "Задача завершена", Telegram.Bot.Types.Enums.ParseMode.None, cancellationToken: cancellationToken);
                            }
                            else await botClient.SendMessage(update.Message.Chat.Id, "Некорректный идентификатор задачи", Telegram.Bot.Types.Enums.ParseMode.None, cancellationToken: cancellationToken);
                            break;
                        case "/report" when toDoUser != null:
                            await Report(update);
                            break;
                        case "/info":
                            Info(update);
                            break;
                        case "/cancel":
                            await ContextRepository.ResetContext(update.Message.From.Id, cancellationToken);
                            await botClient.SendMessage(update.Message.Chat.Id, "Команда отменена", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: Keyboards.DefaultKeyboard, cancellationToken: cancellationToken);
                            break;
                        default:
                            await _botClient.SendMessage(update.Message.Chat.Id, "Жду вашу команду...", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: toDoUser == null ? Keyboards.StartKeyboard : Keyboards.DefaultKeyboard, cancellationToken: _ct);
                            break;
                    }
                    RaiseHandleUpdateCompleted(update.Message.Text);
                    //await Help(update);
                }
            }
            catch (Exception ex)
            {
                await ((IUpdateHandler)this).HandleErrorAsync(botClient, ex, HandleErrorSource.HandleUpdateError, cancellationToken);
                RaiseHandleUpdateCompleted(update.Message.Text);
            }
        }
        Task IUpdateHandler.HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
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
