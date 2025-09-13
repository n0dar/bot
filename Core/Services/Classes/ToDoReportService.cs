using bot.Core.DataAccess;
using bot.Core.Entities;
using bot.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace bot.Core.Services.Classes
{
    internal class ToDoReportService (IToDoRepository toDoRepository) : IToDoReportService
    {
        public async Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStatsAsync(Guid userId, CancellationToken ct)
        {
            var total = ((IReadOnlyList<ToDoItem>)(await toDoRepository.GetAllByUserIdAsync(userId, ct))).Count;
            var active = ((IReadOnlyList<ToDoItem>)(await toDoRepository.GetActiveByUserIdAsync(userId, ct))).Count;
            return (total, total - active, active, DateTime.Now);
        }
    }
}
