#nullable enable
using bot.Core.Entities;
using bot.Core.Exceptions;
using bot.Core.Services.Interfaces;
using bot.TelegramBot;
using bot.TelegramBot.DTO;
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
using Telegram.Bot.Extensions;

namespace bot
{
    internal class UpdateHandler(IUserService UserService, IToDoService ToDoService, IToDoReportService ToDoReportService, IEnumerable<IScenario> Scenarios, IScenarioContextRepository ContextRepository, IToDoListService ToDoListService, CancellationToken CT) : IUpdateHandler
    {
        private ITelegramBotClient _botClient;
        private readonly IUserService _userService = UserService;
        private readonly IToDoService _toDoService = ToDoService;
        private readonly IToDoReportService _toDoReportService = ToDoReportService;
        private readonly IToDoListService _toDoListService = ToDoListService;

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

            if (ScenarioResult == ScenarioResult.Completed) await ContextRepository.ResetContext(((ToDoUser)context.Data["ToDoUser"]).TelegramUserId, ct);
            else await ContextRepository.SetContext(((ToDoUser)context.Data["ToDoUser"]).TelegramUserId, context, ct);
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
                message.AppendLine($"Список{(command == "/show" ? " активных " : " ")}задач:");
                foreach (ToDoItem item in toDoItemList)
                {
                    message.AppendLine(item.ToString());
                }
            }
            else message.AppendLine($"Список{(command == "/show" ? " активных " : " ")}задач пуст"); 
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

        async Task OnCallbackQuery(Update update, CancellationToken ct)
        {
            //CallbackQuery callbackQuery = update.CallbackQuery;

            ToDoUser? toDoUser = await _userService.GetUserAsync(update.CallbackQuery.From.Id, ct);
            if (toDoUser != null)
            {
                ToDoListCallbackDto toDoListCallbackDto = ToDoListCallbackDto.FromString(update.CallbackQuery.Data);
                ScenarioContext? scenarioContext = await ContextRepository.GetContext(update.CallbackQuery.From.Id, ct);

                switch (toDoListCallbackDto.Action)
                {
                    case "show":
                        if (scenarioContext == null)
                        {
                            await ProcessScenario(new ScenarioContext(ScenarioType.ProcessTask), update, ct); 
                            await _botClient.SendMessage(update.CallbackQuery.Message.Chat.Id, $"Активные задачи", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: Keyboards.ShowToDoItemsKeyboard(await _toDoService.GetByUserIdAndListAsync(toDoUser.UserId, toDoListCallbackDto.ToDoListId,ct)), cancellationToken: ct);
                        }                            
                        break;
                    case "addlist":
                        if (scenarioContext == null) await ProcessScenario(new ScenarioContext(ScenarioType.AddList), update, ct);
                        break;
                    case "deletelist":
                        if (scenarioContext == null)
                        { 
                            scenarioContext = new ScenarioContext(ScenarioType.DeleteList);
                            await ProcessScenario(scenarioContext, update, ct);
                        }
                        break;
                    case "deletelistbyid":
                        if (scenarioContext?.CurrentScenario.ToString() == "DeleteList" && scenarioContext?.CurrentStep == "Approve")
                        { 
                            scenarioContext.Data["ToDoList"] = await _toDoListService.GetAsync((Guid)toDoListCallbackDto.ToDoListId, ct);
                            await ProcessScenario(scenarioContext, update, ct);
                        }
                        break;
                    case "yes":
                        if (scenarioContext?.CurrentScenario.ToString() == "DeleteList" && scenarioContext?.CurrentStep == "Delete")
                        {
                            scenarioContext.Data["Approve"] = toDoListCallbackDto.Action;
                            await ProcessScenario(scenarioContext, update, ct);
                        }
                        break;
                    case "no":
                        if (scenarioContext?.CurrentScenario.ToString() == "DeleteList" && scenarioContext?.CurrentStep == "Delete")
                        {
                            scenarioContext.Data["Approve"] = toDoListCallbackDto.Action;
                            await ProcessScenario(scenarioContext, update, ct);
                        }
                        break;
                    case "addtask":
                        if (scenarioContext?.CurrentScenario.ToString() == "AddTask" && scenarioContext?.CurrentStep == "AddTask")
                        {
                            scenarioContext.Data["ToDoList"] = await _toDoListService.GetAsync((Guid)toDoListCallbackDto.ToDoListId, ct);
                            await ProcessScenario(scenarioContext, update, ct);
                        }
                        break;
                    case "showtask":
                        if (scenarioContext?.CurrentScenario.ToString() == "ProcessTask")
                        {
                            ToDoItemCallbackDto toDoItemCallbackDto = ToDoItemCallbackDto.FromString(update.CallbackQuery.Data);
                            ToDoItem? toDoItem = await _toDoService.GetAsync((Guid)toDoItemCallbackDto.ToDoItemId, ct);
                            if (toDoItem != null)
                            {
                                scenarioContext.Data["messageId"] = (await _botClient.SendMessage(update.CallbackQuery.Message.Chat.Id, $"{toDoItem.Name}:\n\nСрок выполнения: {toDoItem.Deadline}\nВремя создания: {toDoItem.CreatedAt}", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: Keyboards.CompleteDeleteTaskKeyboard(toDoItem.Id), cancellationToken: ct)).Id;
                            }
                        }
                        break;
                    case "completetask":
                        if (scenarioContext?.CurrentScenario.ToString() == "ProcessTask")
                        {
                            ToDoItemCallbackDto toDoItemCallbackDto = ToDoItemCallbackDto.FromString(update.CallbackQuery.Data);
                            await _toDoService.MarkCompletedAsync((Guid)toDoItemCallbackDto.ToDoItemId, ct);
                            await _botClient.SendMessage(update.CallbackQuery.Message.Chat.Id, "Задача завершена", Telegram.Bot.Types.Enums.ParseMode.None, cancellationToken: ct);
                            await _botClient.EditMessageReplyMarkup(chatId: update.CallbackQuery.Message.Chat.Id, messageId: (int)scenarioContext.Data["messageId"], replyMarkup: null);
                            //scenarioContext?.CurrentScenario. . ScenarioResult.Completed
                        }
                        break;
                    case "deletetask":
                        break;
                }
            }
            await ((TelegramBotClient)_botClient).AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
        }
        async Task OnMessage(Update update, CancellationToken cancellationToken)
        {
            RaiseHandleUpdateStarted(update.Message.Text);
            string command = update.Message.Text.Trim();
            string? commandParam = null;
            int spaceIndex = command.IndexOf(' ');
            if (spaceIndex >= 0 && spaceIndex < command.Length - 1)
            {
                commandParam = command[(spaceIndex + 1)..].Trim();
                command = command[..spaceIndex];
            }
            ScenarioContext? scenarioContext = await ContextRepository.GetContext(update.Message.From.Id, cancellationToken);
            if (scenarioContext != null && command != "/cancel") await ProcessScenario(scenarioContext, update, cancellationToken);
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
                    case "/show" when toDoUser != null:
                        await _botClient.SendMessage(update.Message.Chat.Id, "Выберите список", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: Keyboards.ShowToDoListKeyboard(await _toDoListService.GetUserListsAsync(toDoUser.UserId, cancellationToken)), cancellationToken: cancellationToken);
                        break;
                    case "/find" when toDoUser != null && commandParam != null:
                        await _botClient.SendMessage(update.Message.Chat.Id, GetMessageForShowCommands(await _toDoService.FindAsync(toDoUser, commandParam, cancellationToken), "/show"), Telegram.Bot.Types.Enums.ParseMode.None, cancellationToken: cancellationToken);
                        break;
                    case "/removetask" when toDoUser != null && commandParam != null:
                        if (Guid.TryParse(commandParam, out taskId))
                        {
                            await _toDoService.DeleteAsync(taskId, _ct);
                            await _botClient.SendMessage(update.Message.Chat.Id, "Задача удалена", Telegram.Bot.Types.Enums.ParseMode.None, cancellationToken: cancellationToken);
                        }
                        else await _botClient.SendMessage(update.Message.Chat.Id, "Некорректный идентификатор задачи", Telegram.Bot.Types.Enums.ParseMode.None, cancellationToken: cancellationToken);
                        break;
                    case "/completetask" when toDoUser != null && commandParam != null:
                        if (Guid.TryParse(commandParam, out taskId))
                        {
                            await _toDoService.MarkCompletedAsync(taskId, cancellationToken);
                            await _botClient.SendMessage(update.Message.Chat.Id, "Задача завершена", Telegram.Bot.Types.Enums.ParseMode.None, cancellationToken: cancellationToken);
                        }
                        else await _botClient.SendMessage(update.Message.Chat.Id, "Некорректный идентификатор задачи", Telegram.Bot.Types.Enums.ParseMode.None, cancellationToken: cancellationToken);
                        break;
                    case "/report" when toDoUser != null:
                        await Report(update);
                        break;
                    case "/info":
                        Info(update);
                        break;
                    case "/cancel":
                        await ContextRepository.ResetContext(update.Message.From.Id, cancellationToken);
                        await _botClient.SendMessage(update.Message.Chat.Id, "Команда отменена", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: Keyboards.DefaultKeyboard, cancellationToken: cancellationToken);
                        break;
                    default:
                        await _botClient.SendMessage(update.Message.Chat.Id, "Жду вашу команду...", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: toDoUser == null ? Keyboards.StartKeyboard : Keyboards.DefaultKeyboard, cancellationToken: _ct);
                        break;
                }
                RaiseHandleUpdateCompleted(update.Message.Text);
            }
        }
        async Task IUpdateHandler.HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                _botClient = botClient;
                await (update switch
                {
                    { Message: { } message } => OnMessage(update, cancellationToken), 
                    { CallbackQuery: { } callbackQuery } => OnCallbackQuery(update, cancellationToken) ,
                    _ => Task.CompletedTask
                });
            }
            catch (Exception ex)
            {
                await ((IUpdateHandler)this).HandleErrorAsync(botClient, ex, HandleErrorSource.HandleUpdateError, cancellationToken);
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
