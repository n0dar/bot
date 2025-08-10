namespace bot
{
    internal class UserService : IUserService
    {
        private ToDoUser? _toDoUser;

        ToDoUser IUserService.GetUser(long telegramUserId)
        {
            if (_toDoUser is not null && _toDoUser.TelegramUserId== telegramUserId)  return _toDoUser;
            return null;
        }
        ToDoUser IUserService.RegisterUser(long telegramUserId, string telegramUserName)
        {
            _toDoUser = new ToDoUser(telegramUserId, telegramUserName);
            return _toDoUser;
        }
    }
}
