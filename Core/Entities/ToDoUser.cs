using System;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace bot.Core.Entities
{
    public class ToDoUser(long telegramUserId, string telegramUserName)
    {
        [JsonConstructor]
        public ToDoUser(long telegramUserId, string telegramUserName, DateTime registeredAt, Guid userId) : this(telegramUserId, telegramUserName)
        {
            this.TelegramUserId = telegramUserId;
            this.TelegramUserName = telegramUserName;
            this.RegisteredAt = registeredAt;
            this.UserId = userId;
        }

        public Guid UserId { get;} = Guid.NewGuid();
        public long TelegramUserId { get; } = telegramUserId;
        public string TelegramUserName { get; } = telegramUserName;
        public DateTime RegisteredAt { get; } = DateTime.UtcNow;
    }
}
