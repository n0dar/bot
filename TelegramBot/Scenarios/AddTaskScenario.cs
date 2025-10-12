using bot.Core.Entities;
using bot.Core.Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace bot.TelegramBot.Scenarios
{
    internal class AddTaskScenario(IUserService UserService, IToDoService ToDoService) : IScenario
    {
        private readonly IUserService _userService  = UserService;
        private readonly IToDoService _toDoService = ToDoService;
        bool IScenario.CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.AddTask;
        }
        async Task<ScenarioResult> IScenario.HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            switch (context.CurrentStep)
            {
                case null:
                    context.Data["ToDoUser"] = await _userService.GetUserAsync(update.Message.From.Id, ct);
                    context.CurrentStep = "Name";
                    await bot.SendMessage(update.Message.Chat.Id, "Введите название задачи:", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: Keyboards.CancelKeyboard, cancellationToken: ct);
                    return ScenarioResult.Transition;
                case "Name":
                    context.Data["ToDoName"] = update.Message.Text;
                    context.CurrentStep = "Deadline";
                    await bot.SendMessage(update.Message.Chat.Id, "Введите дэдлайн задачи (в формате dd.MM.yyyy):", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: Keyboards.CancelKeyboard, cancellationToken: ct);
                    return ScenarioResult.Transition;
                default:
                    if (DateOnly.TryParseExact(update.Message.Text, "dd.MM.yyyy", out DateOnly deadline))
                    {
                        await _toDoService.AddAsync((ToDoUser)context.Data["ToDoUser"], context.Data["ToDoName"].ToString(), deadline, null, ct);
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
