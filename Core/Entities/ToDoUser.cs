using System;

namespace bot.Core.Entities
{
    public class ToDoUser(long telegramUserId, string telegramUserName)
    {
        public Guid UserId { get; set; } = Guid.NewGuid();
        public long TelegramUserId { get; set; } = telegramUserId;
        public string TelegramUserName { get; set; } = telegramUserName;
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }
}
