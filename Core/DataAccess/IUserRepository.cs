#nullable enable
using bot.Core.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace bot.Core.DataAccess
{
    internal interface IUserRepository
    {
        Task<ToDoUser> GetUserAsync(Guid userId, CancellationToken ct);
        Task<ToDoUser> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct);
        Task AddAsync(ToDoUser user, CancellationToken ct);
    }
}
