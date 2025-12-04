using bot.Core.Entities;
using bot.Core.Services.Classes;
using bot.Core.Services.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace bot.TelegramBot.Scenarios
{
    internal class AddListScenario(IUserService UserService, IToDoListService ToDoListService) : IScenario
    {
        bool IScenario.CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.AddList;
        }
        async Task<ScenarioResult> IScenario.HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            switch (context.CurrentStep)
            {
                case null:
                    context.Data["MessageId"] = update.CallbackQuery.Message.Id;
                    context.Data["ToDoUser"] = await UserService.GetUserAsync(update.CallbackQuery.From.Id, ct);
                    await bot.SendMessage(update.CallbackQuery.Message.Chat.Id, "Введите название списка:", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: Keyboards.CancelKeyboard, cancellationToken: ct);
                    context.CurrentStep = "Name";
                    return ScenarioResult.Transition;
                default:
                    await ToDoListService.AddAsync((ToDoUser)context.Data["ToDoUser"], update.Message.Text, ct);
                    await bot.SendMessage(update.Message.Chat.Id, "Название списка добавлено", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: Keyboards.DefaultKeyboard, cancellationToken: ct);
                    await bot.EditMessageReplyMarkup(update.Message.Chat.Id, (int)context.Data["MessageId"], Keyboards.ShowToDoListKeyboard(await ToDoListService.GetUserListsAsync(((ToDoUser)context.Data["ToDoUser"]).UserId, ct)), cancellationToken: ct);
                    return ScenarioResult.Completed;

            }
        }
    }
}
