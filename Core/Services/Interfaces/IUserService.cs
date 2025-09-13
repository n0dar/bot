#nullable enable
using bot.Core.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace bot.Core.Services.Interfaces
{
    internal interface IUserService
    {
        Task<ToDoUser> RegisterUserAsync(long telegramUserId, string telegramUserName, CancellationToken ct);
        Task<ToDoUser> GetUserAsync(long telegramUserId, CancellationToken ct);
    }
}
