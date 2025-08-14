using bot.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot.Core.Services.Classes
{
    internal class ToDoReportService (IToDoService ToDoService) : IToDoReportService
    {
        private readonly IToDoService _toDoService = ToDoService;
        public (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId)
        {
            int total = _toDoService.GetAllByUserId(userId).Count();
            int active = _toDoService.GetActiveByUserId(userId).Count();

            return (total, total - active, active, DateTime.Now);
        }
    }
}
