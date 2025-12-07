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

namespace bot.Infrastructure.DataAccess
{
    internal class SqlToDoListRepository(IDataContextFactory<ToDoDataContext> dataContextFactory) : IToDoListRepository
    {
        async Task IToDoListRepository.AddAsync(ToDoList list, CancellationToken ct)
        {
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();
            await dbContext.InsertAsync<ToDoListModel>(ModelMapper.MapToModel(list), token: ct);
        }
        async Task IToDoListRepository.DeleteAsync(Guid id, CancellationToken ct)
        {
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();
            await dbContext.ToDoList
                .Where(i => i.Id == id)
                .DeleteAsync(ct);
        }
        async Task<bool> IToDoListRepository.ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
        {
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();
            return await dbContext.ToDoList.AnyAsync(m => m.IdToDoUser == userId && m.Name == name, ct);
        }
        async Task<ToDoList> IToDoListRepository.GetAsync(Guid id, CancellationToken ct)
        {
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();
            ToDoListModel model = await dbContext.ToDoList.FirstOrDefaultAsync(m => m.Id == id, ct);
            return ModelMapper.MapFromModel(model);
        }
        async Task<IReadOnlyList<ToDoList>> IToDoListRepository.GetByUserIdAsync(Guid userId, CancellationToken ct)
        {
            List<ToDoList> res = [];
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();
            List<ToDoListModel> models = [.. dbContext.ToDoList.Where(m => m.IdToDoUser == userId)];
            foreach (ToDoListModel model in models)
            {
                res.Add(ModelMapper.MapFromModel(model));
            }
            return (IReadOnlyList<ToDoList>)res;
        }
    }
}