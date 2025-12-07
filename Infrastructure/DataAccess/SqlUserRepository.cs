using bot.Core.DataAccess;
using bot.Core.DataAccess.Models;
using bot.Core.Entities;
using LinqToDB;
using LinqToDB.Async;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace bot.Infrastructure.DataAccess
{
    internal class SqlUserRepository(IDataContextFactory<ToDoDataContext> dataContextFactory) : IUserRepository
    {
        async Task IUserRepository.AddAsync(ToDoUser user, CancellationToken ct)
        {
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();
            await dbContext.InsertAsync<ToDoUserModel>(ModelMapper.MapToModel(user), token: ct);
        }
        async Task<ToDoUser> IUserRepository.GetUserAsync(Guid userId, CancellationToken ct)
        {
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();
            ToDoUserModel model = await dbContext.ToDoUser.FirstOrDefaultAsync(m => m.Id == userId, ct);
            return ModelMapper.MapFromModel(model);
        }
        async Task<ToDoUser> IUserRepository.GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct)
        {
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();
            ToDoUserModel model = await dbContext.ToDoUser.FirstOrDefaultAsync(m => m.TelegramUserId == telegramUserId, ct);
            return ModelMapper.MapFromModel(model);
        }
    }
}
