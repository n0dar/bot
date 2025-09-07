using bot.Core.DataAccess;
using bot.Core.Services.Interfaces;
using System;

namespace bot.Core.Services.Classes
{
    internal class ToDoReportService (IToDoRepository toDoRepository) : IToDoReportService
    {
        public (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId)
        {
            int total = toDoRepository.GetAllByUserId(userId).Count;
            int active = toDoRepository.GetActiveByUserId(userId).Count;

            return (total, total - active, active, DateTime.Now);
        }
    }
}
