using bot.Core.DataAccess;
using bot.Core.Entities;
using bot.Core.Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace bot.Core.Services.Classes
{
    internal class UserService(IUserRepository userRepository) : IUserService
    {
        public async Task<ToDoUser> GetUserAsync(long telegramUserId, CancellationToken ct)
        {
            return await userRepository.GetUserByTelegramUserIdAsync(telegramUserId, ct);
        }
        public async Task<ToDoUser> RegisterUserAsync(long telegramUserId, string telegramUserName, CancellationToken ct)
        {
            ToDoUser toDoUser = new ToDoUser
            {
                UserId = Guid.NewGuid(),
                TelegramUserId = telegramUserId,
                TelegramUserName = telegramUserName,
                RegisteredAt = DateTime.Now
            };
            await userRepository.AddAsync(toDoUser, ct);
            return toDoUser;
        }
    }
}
