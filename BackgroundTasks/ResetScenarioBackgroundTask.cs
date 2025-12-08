using bot.Core.Entities;
using bot.TelegramBot;
using bot.TelegramBot.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace bot.BackgroundTasks
{
    internal class ResetScenarioBackgroundTask
    (
        TimeSpan resetScenarioTimeout,
        IScenarioContextRepository scenarioRepository,
        ITelegramBotClient bot
    ) : BackgroundTask(TimeSpan.FromMinutes(1), nameof(ResetScenarioBackgroundTask))
    {
        protected async override Task Execute(CancellationToken ct)
        {
            IReadOnlyList<ScenarioContext> scenarioContexts = await scenarioRepository.GetContexts(ct);

            ScenarioContext? expiredScenarioContext = scenarioContexts.FirstOrDefault(c => DateTime.Now - c.CreatedAt > resetScenarioTimeout);

            if (expiredScenarioContext != null)
            {
                await scenarioRepository.ResetContext(expiredScenarioContext.UserId, ct);

                await bot.SendMessage
                (
                    ((ToDoUser)expiredScenarioContext.Data["ToDoUser"]).TelegramUserId,
                    $"Сценарий отменен, так как не поступил ответ в течение {resetScenarioTimeout}",
                    Telegram.Bot.Types.Enums.ParseMode.None,
                    replyMarkup: Keyboards.DefaultKeyboard,
                    cancellationToken: ct
                );
            }
        }
    }
}
