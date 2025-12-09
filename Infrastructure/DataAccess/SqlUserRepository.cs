using bot.Core.DataAccess;
using bot.Core.DataAccess.Models;
using bot.Core.Entities;
using LinqToDB;
using LinqToDB.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace bot.Infrastructure.DataAccess
{
    internal class SqlUserRepository(IDataContextFactory<ToDoDataContext> dataContextFactory) : IUserRepository
    {
        public async Task<IReadOnlyList<ToDoUser>> GetUsers(CancellationToken ct)
        {
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();
            List<ToDoUserModel> models = await dbContext.ToDoUser.ToListAsync(ct);

            return models.Select(ModelMapper.MapFromModel).ToList();
        }
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
        async Task<ToDoUser?> IUserRepository.GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct)
        {
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();
            ToDoUserModel model = await dbContext.ToDoUser.FirstOrDefaultAsync(m => m.TelegramUserId == telegramUserId, ct);
            if (model == null) return null;
            else return ModelMapper.MapFromModel(model);
        }
    }
}
