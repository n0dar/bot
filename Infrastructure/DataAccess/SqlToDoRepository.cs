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
        public async Task<IReadOnlyList<ToDoItem>> GetActiveWithDeadline(Guid userId, DateTime from, DateTime to, CancellationToken ct)
        {
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();
            List<ToDoItemModel> models = await dbContext.ToDoItem
                .LoadWith(m => m.ToDoUser)
                .LoadWith(m => m.ToDoList)
                .ThenLoad(m => m.ToDoUser)
                .Where(m => m.IdToDoUser == userId && m.State == ToDoItemState.Active && m.Deadline >= DateOnly.FromDateTime(from) && m.Deadline < DateOnly.FromDateTime(to))
                .ToListAsync(ct);

            List<ToDoItem> res = models
                .Select(ModelMapper.MapFromModel)
                .ToList();

            return (IReadOnlyList<ToDoItem>)res;
        }

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
        async Task<IReadOnlyList<ToDoItem>> IToDoRepository.GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        {
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();
            List<ToDoItemModel> models = await dbContext.ToDoItem
                .LoadWith(m => m.ToDoUser)
                .LoadWith(m => m.ToDoList)
                .ThenLoad(m => m.ToDoUser)
                .Where(m => m.IdToDoUser == userId && m.State == ToDoItemState.Active)
                .ToListAsync(ct);

            List<ToDoItem> res = models
                .Select(ModelMapper.MapFromModel)
                .ToList();

            return (IReadOnlyList<ToDoItem>)res;
        }
        async Task<IReadOnlyList<ToDoItem>> IToDoRepository.GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();
            List<ToDoItemModel> models = await dbContext.ToDoItem
                .LoadWith(m => m.ToDoUser)
                .LoadWith(m => m.ToDoList)
                .ThenLoad(m => m.ToDoUser)
                .Where(m => m.IdToDoUser == userId)
                .ToListAsync(ct);

            List<ToDoItem> res = models
                .Select(ModelMapper.MapFromModel)  
                .ToList();

            return (IReadOnlyList<ToDoItem>)res;
        }
        async Task<ToDoItem> IToDoRepository.GetAsync(Guid id, CancellationToken ct)
        {
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();
            ToDoItemModel model = await dbContext.ToDoItem
                .LoadWith(i => i.ToDoUser)
                .LoadWith(i => i.ToDoList)
                .ThenLoad(i => i.ToDoUser)
                .FirstOrDefaultAsync(m => m.Id == id, ct);
            return ModelMapper.MapFromModel(model);
        }
        async Task IToDoRepository.UpdateAsync(ToDoItem item, CancellationToken ct)
        {
            using ToDoDataContext dbContext = dataContextFactory.CreateDataContext();
            await dbContext.UpdateAsync<ToDoItemModel>(ModelMapper.MapToModel(item), token: ct);
        }
    }
}
