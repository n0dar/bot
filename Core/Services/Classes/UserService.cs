using bot.Core.DataAccess;
using bot.Core.Entities;
using bot.Core.Services.Interfaces;
using System.Collections.Generic;

namespace bot.Core.Services.Classes
{
    internal class UserService(IUserRepository UserRepository) : IUserService
    {
        private readonly IUserRepository _userRepository = UserRepository;

        ToDoUser IUserService.GetUser(long telegramUserId)
        {
            return _userRepository.GetUserByTelegramUserId(telegramUserId);
        }
        ToDoUser IUserService.RegisterUser(long telegramUserId, string telegramUserName)
        {
            ToDoUser toDoUser = new(telegramUserId, telegramUserName);
            _userRepository.Add(toDoUser);
            return toDoUser;
        }
    }
}
