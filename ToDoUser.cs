using System;

namespace bot
{
    internal class ToDoUser
    {
        public Guid UserId { get; }
        public string TelegramUserName { get; }
        public DateTime RegisteredAt { get; }
        public ToDoUser(string telegramUserName)
        {
            UserId = Guid.NewGuid();
            TelegramUserName = telegramUserName;
            RegisteredAt = DateTime.UtcNow;
    }
}
}
