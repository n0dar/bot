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
using static bot.Core.Entities.ToDoItem;

namespace bot.Infrastructure.DataAccess
{
    internal class SqlToDoRepository(IDataContextFactory<ToDoDataContext> dataContextFactory) : IToDoRepository
    {
        async Task IToDoRepository.AddAsync(ToDoItem item, CancellationToken ct)
        {
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();
            await dbContext.InsertAsync<ToDoItemModel>(ModelMapper.MapToModel(item), token: ct);
        }
        async Task<int> IToDoRepository.CountActiveAsync(Guid userId, CancellationToken ct)
        {
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();
            return await dbContext.ToDoItem.Where(m => m.IdToDoUser == userId && m.State == ToDoItemState.Active).CountAsync(ct); 
        }
        async Task IToDoRepository.DeleteAsync(Guid id, CancellationToken ct)
        {
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();
            await dbContext.ToDoItem
                .Where(i => i.Id == id)
                .DeleteAsync(ct);
        }
        async Task<bool> IToDoRepository.ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
        {
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();
            return await dbContext.ToDoItem.AnyAsync(m => m.IdToDoUser == userId && m.Name == name, ct);
        }
        async Task<IReadOnlyList<ToDoItem>> IToDoRepository.FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            return await GetItemsAsync(userId, predicate, ct);
        }
        async Task<IReadOnlyList<ToDoItem>> IToDoRepository.GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        {
            return await GetItemsAsync(userId, item => item.State == ToDoItemState.Active, ct);
        }
        async Task<IReadOnlyList<ToDoItem>> IToDoRepository.GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            return await GetItemsAsync(userId, item => true, ct);
        }
        async Task<ToDoItem> IToDoRepository.GetAsync(Guid id, CancellationToken ct)
        {
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();
            ToDoItemModel model = await dbContext.ToDoItem.FirstOrDefaultAsync(m => m.Id == id, ct);
            return ModelMapper.MapFromModel(model);
        }
        async Task IToDoRepository.UpdateAsync(ToDoItem item, CancellationToken ct)
        {
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();
            await dbContext.UpdateAsync<ToDoItemModel>(ModelMapper.MapToModel(item), token: ct);
        }
        private async Task<IReadOnlyList<ToDoItem>> GetItemsAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            List<ToDoItem> res = [];
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();
            List<ToDoItemModel> models = [.. dbContext.ToDoItem.Where(m => m.IdToDoUser == userId)];
            foreach (ToDoItemModel model in models)
            {
                ToDoItem toDoItem = ModelMapper.MapFromModel(model);
                if (predicate(toDoItem)) res.Add(toDoItem);
            }
            return (IReadOnlyList<ToDoItem>)res;
        }
    }
}
