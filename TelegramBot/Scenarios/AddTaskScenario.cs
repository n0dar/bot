using bot.Core.Entities;
using bot.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace bot.TelegramBot.Scenarios
{
    internal class AddTaskScenario(IUserService UserService, IToDoListService ToDoListService, IToDoService ToDoService) : IScenario
    {
        bool IScenario.CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.AddTask;
        }
        async Task<ScenarioResult> IScenario.HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            IReadOnlyList<ToDoList> toDoLists;
            switch (context.CurrentStep)
            {
                case null:
                    context.Data["ToDoUser"] = await UserService.GetUserAsync(update.Message.From.Id, ct);
                    toDoLists = await ToDoListService.GetUserListsAsync(((ToDoUser)context.Data["ToDoUser"]).UserId, ct);
                    if (toDoLists.Any())
                    {
                        await bot.SendMessage(update.Message.Chat.Id, "Выберете список:", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: Keyboards.AddTaskToDoListKeyboard(toDoLists), cancellationToken: ct);
                    }
                    else
                    {
                        await bot.SendMessage(update.Message.Chat.Id, "У вас нет ни одного списка задач. Введите название нового списка:", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: Keyboards.CancelKeyboard, cancellationToken: ct);
                    }
                    context.CurrentStep = "AddTask";
                    return ScenarioResult.Transition;
                case "AddTask":
                    toDoLists = await ToDoListService.GetUserListsAsync(((ToDoUser)context.Data["ToDoUser"]).UserId, ct);
                    if (toDoLists.Any()) { }
                    else
                    {
                        await ToDoListService.AddAsync((ToDoUser)context.Data["ToDoUser"], update.Message.Text, ct);
                        toDoLists = await ToDoListService.GetUserListsAsync(((ToDoUser)context.Data["ToDoUser"]).UserId, ct);
                        context.Data["ToDoList"] = toDoLists[0];
                    }
                    await bot.SendMessage((update.Message ?? update.CallbackQuery.Message).Chat.Id, "Введите название задачи:", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: Keyboards.CancelKeyboard, cancellationToken: ct);
                    context.CurrentStep = "TaskName";
                    return ScenarioResult.Transition;
                case "TaskName":
                    context.Data["ToDoName"] = update.Message.Text;
                    await bot.SendMessage(update.Message.Chat.Id, "Введите дэдлайн задачи (в формате dd.MM.yyyy)", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: Keyboards.CancelKeyboard, cancellationToken: ct);
                    context.CurrentStep = "Deadline";
                    return ScenarioResult.Transition;
                default:
                    if (DateOnly.TryParseExact(update.Message.Text, "dd.MM.yyyy", out DateOnly deadline))
                    {
                        context.Data["ToDoDeadline"] = update.Message.Text;
                        await ToDoService.AddAsync((ToDoUser)context.Data["ToDoUser"], context.Data["ToDoName"].ToString(), DateOnly.Parse(context.Data["ToDoDeadline"].ToString()), (ToDoList)context.Data["ToDoList"], ct);
                        await bot.SendMessage(update.Message.Chat.Id, "Задача добавлена", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: Keyboards.DefaultKeyboard, cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }
                    else
                    {
                        await bot.SendMessage(update.Message.Chat.Id, "Введите дэдлайн задачи (в формате dd.MM.yyyy):", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: Keyboards.CancelKeyboard, cancellationToken: ct);
                        return ScenarioResult.Transition;
                    }
            }
        }
    }
}
