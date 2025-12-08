#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace bot.TelegramBot.Scenarios
{
    public interface IScenarioContextRepository
    {
        //Получить контекст пользователя
        Task<ScenarioContext?> GetContext(long userId, CancellationToken ct);
        //Задать контекст пользователя
        Task SetContext(long userId, ScenarioContext context, CancellationToken ct);
        //Сбросить (очистить) контекст пользователя
        Task ResetContext(long userId, CancellationToken ct);
        Task<IReadOnlyList<ScenarioContext>> GetContexts(CancellationToken ct);
    }
}
