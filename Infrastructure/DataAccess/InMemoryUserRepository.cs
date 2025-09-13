using bot.Core.DataAccess;
using bot.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace bot.Infrastructure.DataAccess
{
    internal class InMemoryUserRepository : IUserRepository
    {
        private readonly List<ToDoUser> toDoUserList = [];
        public async Task AddAsync(ToDoUser user, CancellationToken ct)
        {
            await Task.Run(()=> toDoUserList.Add(user), ct);
        }
        public Task<ToDoUser> GetUserAsync(Guid userId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
        public async Task<ToDoUser> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct)
        {
            return await Task.Run(()=> toDoUserList.Find(x => x.TelegramUserId == telegramUserId), ct);
        }
    }
}
