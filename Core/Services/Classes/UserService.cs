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
            ToDoUser toDoUser = new ();
            toDoUser.TelegramUserId = telegramUserId;
            toDoUser.TelegramUserName = telegramUserName;
            await userRepository.AddAsync(toDoUser, ct);
            return toDoUser;
        }
    }
}
