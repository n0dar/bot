using bot.Core.DataAccess;
using bot.Core.Entities;
using bot.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace bot.BackgroundTasks
{
    internal class TodayBackgroundTask(INotificationService notificationService, IUserRepository userRepository, IToDoRepository toDoRepository) : BackgroundTask(TimeSpan.FromMinutes(1), nameof(TodayBackgroundTask))
    {
        protected async override Task Execute(CancellationToken ct)
        {
            IReadOnlyList<ToDoUser> users = await userRepository.GetUsers(ct);

            foreach (ToDoUser u in users)
            {
                IReadOnlyList<ToDoItem> todayToDoItems = await toDoRepository.GetActiveWithTodayDeadline(u.UserId,  ct);
                if (todayToDoItems.Any()) await notificationService.ScheduleNotification(u.UserId, $"Today_{DateOnly.FromDateTime(DateTime.Now)}", $"Запланированные на сегодня задачи {String.Join(",", todayToDoItems.Select(t => t.Name).ToList())}", DateTime.Now, ct);
            }
        }
    }
}
