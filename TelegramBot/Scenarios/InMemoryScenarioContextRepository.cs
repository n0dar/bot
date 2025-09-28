using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace bot.TelegramBot.Scenarios
{
    public class InMemoryScenarioContextRepository : IScenarioContextRepository
    {
        private readonly Dictionary<long, ScenarioContext> _contextRepository = [];
        Task<ScenarioContext> IScenarioContextRepository.GetContext(long userId, CancellationToken ct)
        {
            _contextRepository.TryGetValue(userId, out var context);
            return Task.FromResult(context);
        }
        Task IScenarioContextRepository.ResetContext(long userId, CancellationToken ct)
        {
            _contextRepository.Remove(userId);
            return Task.CompletedTask;
        }
        Task IScenarioContextRepository.SetContext(long userId, ScenarioContext context, CancellationToken ct)
        {
            _contextRepository[userId] = context;
            return Task.CompletedTask;
        }
    }
}
