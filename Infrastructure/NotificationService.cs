
using bot.Core.Entities;
using bot.Core.Services.Interfaces;
using bot.Infrastructure.DataAccess;
using bot.Infrastructure.DataAccess.Models;
using LinqToDB;
using LinqToDB.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace bot.Infrastructure
{
    internal class NotificationService(IDataContextFactory<ToDoDataContext> dataContextFactory) : INotificationService
    {
        public async Task<IReadOnlyList<Notification>> GetScheduledNotification(DateTime scheduledBefore, CancellationToken ct)
        {
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();

            List<NotificationModel> notificationModels = await dbContext.Notifications
                 .Where(n => !n.IsNotified && n.ScheduledAt <= scheduledBefore)
                 .ToListAsync(ct);

            IReadOnlyList<Notification> notifications = notificationModels
                  .Select(ModelMapper.MapFromModel)
                  .ToList();
    
            return notifications;
        }

        public async Task MarkNotified(Guid notificationId, CancellationToken ct)
        {
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();

            NotificationModel notificationModel = await dbContext.Notifications
                .FirstOrDefaultAsync(n=>n.Id == notificationId, ct);

            Notification notification = ModelMapper.MapFromModel(notificationModel);

            notification.IsNotified = true;
            notification.NotifiedAt = DateTime.Now;

            await dbContext.UpdateAsync(notification, token: ct);
        }
        public async Task<bool> ScheduleNotification(Guid userId, string type, string text, DateTime scheduledAt, CancellationToken ct)
        {
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();

            if (!await dbContext.Notifications.AnyAsync(n => n.IdToDoUser == userId && n.Type == type, ct))
            {
                Notification notification = new()
                {
                    Id = Guid.NewGuid(),
                    IdToDoUser = userId,
                    Type = type,
                    Text = text,
                    ScheduledAt = scheduledAt,
                    IsNotified = false,
                };

                await dbContext.InsertAsync(notification, token: ct);

                return true;
            }
            else return false;
        }
    }
}
