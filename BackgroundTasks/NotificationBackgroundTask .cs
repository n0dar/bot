using bot.Core.Entities;
using bot.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace bot.BackgroundTasks
{
    internal class NotificationBackgroundTask(INotificationService notificationService, ITelegramBotClient bot) : BackgroundTask(TimeSpan.FromMinutes(1), nameof(NotificationBackgroundTask))
    {
        protected async override Task Execute(CancellationToken ct)
        {
            IReadOnlyList<Notification> notifications = await notificationService.GetScheduledNotification(DateTime.Now, ct);

            foreach (Notification n in notifications)
            {
                await bot.SendMessage(n.User.TelegramUserId, n.Text, Telegram.Bot.Types.Enums.ParseMode.None, cancellationToken: ct);
                await notificationService.MarkNotified(n.Id, ct);
            }
        }
    }
}
