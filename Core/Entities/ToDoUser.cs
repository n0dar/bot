using System;
using System.Text.Json.Serialization;

namespace bot.Core.Entities
{
    internal class ToDoUser(long telegramUserId, string telegramUserName)
    {
        [JsonConstructor]
        public ToDoUser(long telegramUserId, string telegramUserName, DateTime registeredAt) : this(telegramUserId, telegramUserName)
        {
            this.TelegramUserId = telegramUserId;
            this.TelegramUserName = telegramUserName;
            this.RegisteredAt = registeredAt;
        }
        public Guid UserId { get; set; } = Guid.NewGuid();
        public long TelegramUserId { get; } = telegramUserId;
        public string TelegramUserName { get; } = telegramUserName;
        public DateTime RegisteredAt { get; } = DateTime.UtcNow;
    }
}
