using System;

namespace bot.Core.Entities
{
    internal class ToDoUser(long telegramUserId, string telegramUserName)
    {
        public long TelegramUserId { get; } = telegramUserId;
        public string TelegramUserName { get; } = telegramUserName;
        public DateTime RegisteredAt { get; } = DateTime.UtcNow;
    }
}
