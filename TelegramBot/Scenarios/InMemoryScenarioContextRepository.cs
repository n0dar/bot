using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace bot.TelegramBot.Scenarios
{
    public class InMemoryScenarioContextRepository : IScenarioContextRepository
    {
        private readonly ConcurrentDictionary<long, ScenarioContext> _contextRepository = [];
        Task<IReadOnlyList<ScenarioContext>> IScenarioContextRepository.GetContexts(CancellationToken ct)
        {
            return Task.FromResult((IReadOnlyList<ScenarioContext>)_contextRepository.Select(s => s.Value).ToList());
        }
        Task<ScenarioContext> IScenarioContextRepository.GetContext(long userId, CancellationToken ct)
        {
            _contextRepository.TryGetValue(userId, out ScenarioContext context);
            return Task.FromResult(context);
        }
        Task IScenarioContextRepository.ResetContext(long userId, CancellationToken ct)
        {
            _contextRepository.TryRemove(userId, out ScenarioContext value);
            return Task.CompletedTask;
        }
        Task IScenarioContextRepository.SetContext(long userId, ScenarioContext context, CancellationToken ct)
        {
            _contextRepository.TryAdd(userId, context);
            return Task.CompletedTask;
        }
    }
}
