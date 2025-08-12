using System.Collections.Generic;

namespace bot
{
    internal class UserService : IUserService
    {
        private readonly List<ToDoUser> _toDoUserList = [];
        ToDoUser IUserService.GetUser(long telegramUserId)
        {
            return _toDoUserList.Find(x => x.TelegramUserId == telegramUserId);
        }
        ToDoUser IUserService.RegisterUser(long telegramUserId, string telegramUserName)
        {
            ToDoUser toDoUser = new(telegramUserId, telegramUserName);
            _toDoUserList.Add (toDoUser);
            return toDoUser;
        }
    }
}
