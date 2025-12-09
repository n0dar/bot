using bot.Core.DataAccess;
using bot.Core.Entities;
using bot.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace bot.BackgroundTasks
{
    internal class DeadlineBackgroundTask(INotificationService notificationService, IUserRepository userRepository, IToDoRepository toDoRepository) : BackgroundTask(TimeSpan.FromMinutes(1), nameof(DeadlineBackgroundTask))
    {
        protected async override Task Execute(CancellationToken ct)
        {
            IReadOnlyList<ToDoUser>  users = await userRepository.GetUsers(ct);

            foreach (ToDoUser u in users)
            {
                IReadOnlyList<ToDoItem> deadToDoItems = await toDoRepository.GetActiveWithDeadline(u.UserId, DateTime.Now.AddDays(-1).Date, DateTime.Now.Date, ct);
                foreach (ToDoItem i in deadToDoItems)
                {
                    await notificationService.ScheduleNotification(u.UserId, $"Dealine_{i.Id}", $"Ой! Вы пропустили дедлайн по задаче {i.Name}", DateTime.Now, ct);
                }
            }
        }
    }
}
