using bot.Core.DataAccess;
using bot.Core.Entities;
using bot.Core.Services.Interfaces;
using System.Collections.Generic;

namespace bot.Core.Services.Classes
{
    internal class UserService(IUserRepository userRepository) : IUserService
    {
        ToDoUser IUserService.GetUser(long telegramUserId)
        {
            return userRepository.GetUserByTelegramUserId(telegramUserId);
        }
        ToDoUser IUserService.RegisterUser(long telegramUserId, string telegramUserName)
        {
            ToDoUser toDoUser = new(telegramUserId, telegramUserName);
            userRepository.Add(toDoUser);
            return toDoUser;
        }
    }
}
