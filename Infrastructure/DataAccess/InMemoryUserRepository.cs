using bot.Core.DataAccess;
using bot.Core.Entities;
using System;
using System.Collections.Generic;

namespace bot.Infrastructure.DataAccess
{
    internal class InMemoryUserRepository : IUserRepository
    {
        private readonly List<ToDoUser> toDoUserList = [];
        public void Add(ToDoUser user)
        {
            toDoUserList.Add(user);
        }

        public ToDoUser GetUser(Guid userId)
        {
            throw new NotImplementedException();
        }

        public ToDoUser GetUserByTelegramUserId(long telegramUserId)
        {
            return toDoUserList.Find(x => x.TelegramUserId == telegramUserId);
        }
    }
}
