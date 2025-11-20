using bot.Core.Entities;
using bot.Core.Services.Classes;
using bot.Core.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace bot.TelegramBot.Scenarios
{
    internal class DeleteListScenario(IUserService UserService, IToDoListService ToDoListService, IToDoService ToDoService) : IScenario
    {
        bool IScenario.CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.DeleteList;
        }
        async Task<ScenarioResult> IScenario.HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            switch (context.CurrentStep)
            {
                case null:
                    context.Data["ToDoUser"] = await UserService.GetUserAsync(update.CallbackQuery.From.Id, ct);
                    IReadOnlyList<ToDoList> toDoLists = await ToDoListService.GetUserListsAsync(((ToDoUser)context.Data["ToDoUser"]).UserId, ct);
                    if (toDoLists.Any())
                    {
                        Message sentMessage  = await bot.SendMessage(update.CallbackQuery.Message.Chat.Id, "Выберете список для удаления:", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: Keyboards.DeleteToDoListKeyboard(toDoLists), cancellationToken: ct);
                        context.CurrentStep = "Approve";
                        return ScenarioResult.Transition;
                    }
                    else return ScenarioResult.Completed;
                case "Approve":
                    await bot.SendMessage(update.CallbackQuery.Message.Chat.Id, $"Подтверждаете удаление списка {((ToDoList)context.Data["ToDoList"]).Name} и всех его задач?", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: Keyboards.YesNoKeyboard(), cancellationToken: ct);
                    context.CurrentStep = "Delete";
                    return ScenarioResult.Transition;
                default:
                    if (context.Data["Approve"].ToString() == "yes")
                    {
                        int deltededToDoCount = await ((ToDoService)ToDoService).DeleteByUserIdAndListAsync(((ToDoUser)context.Data["ToDoUser"]).UserId, ((ToDoList)context.Data["ToDoList"]).Id, ct);
                        await ToDoListService.DeleteAsync(((ToDoList)context.Data["ToDoList"]).Id, ct);
                        await bot.SendMessage(update.CallbackQuery.Message.Chat.Id, $"Список и его задачи в количестве {deltededToDoCount} шт. удалены.)  ", Telegram.Bot.Types.Enums.ParseMode.None, cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }
                    else
                    {
                        await bot.SendMessage(update.CallbackQuery.Message.Chat.Id, "Удаление отменено", Telegram.Bot.Types.Enums.ParseMode.None, cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }
            }
        }
    }
}
