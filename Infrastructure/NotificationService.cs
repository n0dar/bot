
using bot.Core.DataAccess.Models;
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
                .LoadWith(m => m.ToDoUser)
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

            notificationModel.IsNotified = true;
            notificationModel.NotifiedAt = DateTime.Now;

            await dbContext.UpdateAsync<NotificationModel>(notificationModel, token: ct);
        }
        public async Task<bool> ScheduleNotification(Guid userId, string type, string text, DateTime scheduledAt, CancellationToken ct)
        {
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();

            ToDoUserModel model = await dbContext.ToDoUser.FirstOrDefaultAsync(m => m.Id == userId, ct);

            if (!await dbContext.Notifications.AnyAsync(n => n.IdToDoUser == userId && n.Type == type, ct))
            {
                NotificationModel notificationModel = new()
                {
                    Id = Guid.NewGuid(),
                    IdToDoUser = userId,//ModelMapper.MapFromModel(model),
                    Type = type,
                    Text = text,
                    ScheduledAt = scheduledAt,
                    IsNotified = false,
                };

                await dbContext.InsertAsync<NotificationModel>(notificationModel, token: ct);

                return true;
            }
            else return false;
        }
    }
}
