using System;

namespace bot.Core.Services.Interfaces
{
    internal interface IToDoReportService
    {
        (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId);
    }
}
