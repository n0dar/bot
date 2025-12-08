#nullable enable
using bot.Core.Entities;
using bot.Core.Exceptions;
using bot.Core.Services.Classes;
using bot.Core.Services.Interfaces;
using bot.TelegramBot;
using bot.TelegramBot.DTO;
using bot.TelegramBot.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using static bot.Core.Entities.ToDoItem;

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
            ToDoUser? toDoUser = await _userService.GetUserAsync(update.CallbackQuery.From.Id, ct);
            if (toDoUser != null)
            {
                CallbackDto callbackDto = CallbackDto.FromString(update.CallbackQuery.Data);
                ScenarioContext? scenarioContext = await ContextRepository.GetContext(update.CallbackQuery.From.Id, ct);

                switch (callbackDto.Action)
                {
                    case "show":
                        if (scenarioContext == null)
                        {
                            PagedListCallbackDto pagedListCallbackDto = PagedListCallbackDto.FromString(update.CallbackQuery.Data);
                            IReadOnlyList<ToDoItem>? toDoItems = await ((ToDoService)_toDoService).GetByUserIdAndListAsync(toDoUser.UserId, pagedListCallbackDto.ToDoListId, ToDoItemState.Active, ct);
                            await _botClient.EditMessageText(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.Id, $"Активные задачи списка '{(await _toDoListService.GetAsync((Guid)pagedListCallbackDto.ToDoListId, ct)).Name}'", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: Keyboards.PagedButtonsKeyboard(toDoItems, pagedListCallbackDto), cancellationToken: ct);
                        }
                        break;
                    case "show_completed":
                        if (scenarioContext == null)
                        {
                            PagedListCallbackDto pagedListCallbackDto = PagedListCallbackDto.FromString(update.CallbackQuery.Data);
                            IReadOnlyList<ToDoItem>? toDoItems = await ((ToDoService)_toDoService).GetByUserIdAndListAsync(toDoUser.UserId, pagedListCallbackDto.ToDoListId, ToDoItemState.Completed, ct);
                            if (toDoItems.Any())
                                await _botClient.EditMessageText(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.Id, $"Выполненные задачи списка '{(await _toDoListService.GetAsync((Guid)pagedListCallbackDto.ToDoListId, ct)).Name}'", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: Keyboards.PagedButtonsKeyboard(toDoItems, pagedListCallbackDto), cancellationToken: ct);
                            else
                                await _botClient.SendMessage(update.CallbackQuery.Message.Chat.Id, $"В списке нет выполненных задач", Telegram.Bot.Types.Enums.ParseMode.None, cancellationToken: ct);
                        }
                        break;
                    case "addlist":
                        if (scenarioContext == null) await ProcessScenario(new ScenarioContext(ScenarioType.AddList), update, ct);
                        break;
                    case "deletelist":
                        if (scenarioContext == null)
                        {
                            //await _botClient.EditMessageReplyMarkup(chatId: update.CallbackQuery.Message.Chat.Id, messageId: update.CallbackQuery.Message.MessageId);
                            scenarioContext = new ScenarioContext(ScenarioType.DeleteList);
                            await ProcessScenario(scenarioContext, update, ct);
                        }
                        break;
                    case "deletelistbyid":
                        if (scenarioContext?.CurrentScenario.ToString() == "DeleteList" && scenarioContext?.CurrentStep == "Approve")
                        {
                            ToDoListCallbackDto toDoListCallbackDto = ToDoListCallbackDto.FromString(update.CallbackQuery.Data);
                            await _botClient.EditMessageReplyMarkup(chatId: update.CallbackQuery.Message.Chat.Id, messageId: update.CallbackQuery.Message.MessageId);
                            scenarioContext.Data["ToDoList"] = await _toDoListService.GetAsync((Guid)toDoListCallbackDto.ToDoListId, ct);
                            await ProcessScenario(scenarioContext, update, ct);
                        }
                        break;
                    case "deletelistbyidyes":
                        if (scenarioContext?.CurrentScenario.ToString() == "DeleteList" && scenarioContext?.CurrentStep == "Delete")
                        {
                            await _botClient.EditMessageReplyMarkup(chatId: update.CallbackQuery.Message.Chat.Id, messageId: update.CallbackQuery.Message.MessageId);
                            scenarioContext.Data["Approve"] = callbackDto.Action;
                            await ProcessScenario(scenarioContext, update, ct);
                        }
                        break;
                    case "deletelistbyidno":
                        if (scenarioContext?.CurrentScenario.ToString() == "DeleteList" && scenarioContext?.CurrentStep == "Delete")
                        {
                            await _botClient.EditMessageReplyMarkup(chatId: update.CallbackQuery.Message.Chat.Id, messageId: update.CallbackQuery.Message.MessageId);
                            scenarioContext.Data["Approve"] = callbackDto.Action;
                            await ProcessScenario(scenarioContext, update, ct);
                        }
                        break;
                    case "addtask":
                        if (scenarioContext?.CurrentScenario.ToString() == "AddTask" && scenarioContext?.CurrentStep == "AddTask")
                        {
                            ToDoListCallbackDto toDoListCallbackDto = ToDoListCallbackDto.FromString(update.CallbackQuery.Data);
                            await _botClient.EditMessageReplyMarkup(chatId: update.CallbackQuery.Message.Chat.Id, messageId: update.CallbackQuery.Message.MessageId);
                            scenarioContext.Data["ToDoList"] = await _toDoListService.GetAsync((Guid)toDoListCallbackDto.ToDoListId, ct);
                            await ProcessScenario(scenarioContext, update, ct);
                        }
                        break;
                    case "showtask":
                        if (scenarioContext == null)
                        {
                            ToDoItemCallbackDto toDoItemCallbackDto = ToDoItemCallbackDto.FromString(update.CallbackQuery.Data);
                            await _botClient.EditMessageReplyMarkup(chatId: update.CallbackQuery.Message.Chat.Id, messageId: update.CallbackQuery.Message.MessageId);
                            ToDoItem? toDoItem = await _toDoService.GetAsync((Guid)toDoItemCallbackDto.ToDoItemId, ct);
                            if (toDoItem != null)  await _botClient.SendMessage(update.CallbackQuery.Message.Chat.Id, $"Задача '{toDoItem.Name}'\n\nСрок выполнения: {toDoItem.Deadline}\nВремя создания: {toDoItem.CreatedAt}" +  (toDoItem.StateChangedAt == null ? "" : "\nВремя выполнения: " + toDoItem.StateChangedAt?.ToString()), Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: Keyboards.CompleteDeleteTaskKeyboard(toDoItem), cancellationToken: ct);
                        }
                        break;
                    case "completetask":
                        if (scenarioContext == null)
                        {
                            ToDoItemCallbackDto toDoItemCallbackDto = ToDoItemCallbackDto.FromString(update.CallbackQuery.Data);
                            await _botClient.EditMessageReplyMarkup(chatId: update.CallbackQuery.Message.Chat.Id, messageId: update.CallbackQuery.Message.MessageId);
                            await _toDoService.MarkCompletedAsync((Guid)toDoItemCallbackDto.ToDoItemId, ct);
                            await _botClient.SendMessage(update.CallbackQuery.Message.Chat.Id, "Задача завершена", Telegram.Bot.Types.Enums.ParseMode.None, cancellationToken: ct);
                        }
                        break;
                    case "deletetask":
                        if (scenarioContext == null)
                        {
                            ToDoItemCallbackDto toDoItemCallbackDto = ToDoItemCallbackDto.FromString(update.CallbackQuery.Data);
                            await _botClient.EditMessageReplyMarkup(chatId: update.CallbackQuery.Message.Chat.Id, messageId: update.CallbackQuery.Message.MessageId);
                            ToDoItem? toDoItem = await _toDoService.GetAsync((Guid)toDoItemCallbackDto.ToDoItemId, ct);
                            if (toDoItem != null) await _botClient.SendMessage(update.CallbackQuery.Message.Chat.Id, $"Подтверждаете удаление задачи {toDoItem.Name}?", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: Keyboards.YesNoKeyboard("deletetask", toDoItem.Id), cancellationToken: ct);
                        }
                        break;
                    case "deletetaskyes":
                        if (scenarioContext == null)
                        {
                            ToDoItemCallbackDto toDoItemCallbackDto = ToDoItemCallbackDto.FromString(update.CallbackQuery.Data);
                            await _botClient.EditMessageReplyMarkup(chatId: update.CallbackQuery.Message.Chat.Id, messageId: update.CallbackQuery.Message.MessageId);
                            await _toDoService.DeleteAsync((Guid)toDoItemCallbackDto.ToDoItemId, ct);
                            await _botClient.SendMessage(update.CallbackQuery.Message.Chat.Id, $"Задача удалена", Telegram.Bot.Types.Enums.ParseMode.None, cancellationToken: ct);
                        }
                        break;
                    case "deletetaskno":
                        if (scenarioContext == null)
                        {
                            await _botClient.EditMessageReplyMarkup(chatId: update.CallbackQuery.Message.Chat.Id, messageId: update.CallbackQuery.Message.MessageId);
                            await _botClient.SendMessage(update.CallbackQuery.Message.Chat.Id, "Удаление отменено", Telegram.Bot.Types.Enums.ParseMode.None, cancellationToken: ct);
                        }
                        break;
                }
            }
            await ((TelegramBotClient)_botClient).AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
        }
        async Task OnMessage(Update update, CancellationToken cancellationToken)
        {
            RaiseHandleUpdateStarted(update.Message.Text);
            string command = update.Message.Text.Trim();
            ScenarioContext? scenarioContext = await ContextRepository.GetContext(update.Message.From.Id, cancellationToken);
            if (scenarioContext != null && command != "/cancel" && command != "/show" && command != "/addtask" && command != "/report") await ProcessScenario(scenarioContext, update, cancellationToken);
            else
            {
                ToDoUser? toDoUser = await _userService.GetUserAsync(update.Message.From.Id, _ct);

                switch (command)
                {
                    case "/start" when toDoUser == null:
                        await Start(update);
                        break;
                    case "/addtask" when toDoUser != null:
                        ScenarioContext addTaskScenarioContext = new(ScenarioType.AddTask);
                        await ProcessScenario(addTaskScenarioContext, update, cancellationToken);
                        break;
                    case "/show" when toDoUser != null:
                        await _botClient.SendMessage(update.Message.Chat.Id, "Выберите список", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: Keyboards.ShowToDoListKeyboard(await _toDoListService.GetUserListsAsync(toDoUser.UserId, cancellationToken)), cancellationToken: cancellationToken);
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
