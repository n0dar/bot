using bot.Core.DataAccess;
using bot.Core.Entities;
using bot.Core.Services.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace bot.Core.Services.Classes
{
    internal class UserService(IUserRepository userRepository) : IUserService
    {
        public async Task<ToDoUser> GetUserAsync(long telegramUserId, CancellationToken ct)
        {
            return await Task.Run(() => userRepository.GetUserByTelegramUserIdAsync(telegramUserId, ct));
        }
        public async Task<ToDoUser> RegisterUserAsync(long telegramUserId, string telegramUserName, CancellationToken ct)
        {
            ToDoUser toDoUser = new(telegramUserId, telegramUserName);
            await Task.Run(() => userRepository.AddAsync(toDoUser, ct),ct);
            return toDoUser;
        }
    }
}
