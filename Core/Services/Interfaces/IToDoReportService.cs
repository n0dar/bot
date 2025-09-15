using System;
using System.Threading;
using System.Threading.Tasks;

namespace bot.Core.Services.Interfaces
{
    internal interface IToDoReportService
    {
        Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStatsAsync(Guid userId, CancellationToken ct);
    }
}
