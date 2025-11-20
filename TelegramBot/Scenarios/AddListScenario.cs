using bot.Core.Entities;
using bot.Core.Services.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

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
                    context.Data["ToDoUser"] = await UserService.GetUserAsync(update.CallbackQuery.From.Id, ct);
                    await bot.SendMessage(update.CallbackQuery.Message.Chat.Id, "Введите название списка:", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: Keyboards.CancelKeyboard, cancellationToken: ct);
                    context.CurrentStep = "Name";
                    return ScenarioResult.Transition;
                default:
                    //context.Data["ToDoListName"] = update.Message.Text;
                    await ToDoListService.AddAsync((ToDoUser)context.Data["ToDoUser"], update.Message.Text, ct);
                    await bot.SendMessage(update.Message.Chat.Id, "Название списка добавлено", Telegram.Bot.Types.Enums.ParseMode.None, replyMarkup: Keyboards.DefaultKeyboard, cancellationToken: ct);
                    return ScenarioResult.Completed;

            }
        }
    }
}
