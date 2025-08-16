#nullable enable
using bot;
using bot.Core.Entities;

namespace bot.Core.Services.Interfaces
{
    internal interface IUserService
    {
        ToDoUser RegisterUser(long telegramUserId, string telegramUserName);
        ToDoUser? GetUser(long telegramUserId);
    }
}
