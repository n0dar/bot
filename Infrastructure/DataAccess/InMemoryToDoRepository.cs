using bot.Core.DataAccess;
using bot.Core.Entities;
using bot.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static bot.Core.Entities.ToDoItem;

namespace bot.Infrastructure.DataAccess
{
    internal class InMemoryToDoRepository : IToDoRepository
    {
        public readonly List<ToDoItem> _toDoItemList = [];
        public async Task AddAsync(ToDoItem item, CancellationToken ct)
        {
            await Task.Run(() => _toDoItemList.Add(item), ct);
        }
        public async Task<int> CountActiveAsync(Guid userId, CancellationToken ct)
        {
            return await Task.Run(() => _toDoItemList.FindAll(x => x.User.UserId == userId && x.State == ToDoItemState.Active).Count, ct);
        }
        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            bool exists = await Task.Run(() => _toDoItemList.Exists(x => x.Id == id), ct); 
            if (exists) 
                await Task.Run(() => _toDoItemList.RemoveAll(x => x.Id == id), ct);
            else 
                throw new TaskDoesNotExistException("Задача с таким GUID не существует");
        }
        public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
        {
            return await Task.Run(() => _toDoItemList.Exists(x => x.User.UserId == userId && x.Name == name), ct);
        }
        public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            return await Task.Run(() => _toDoItemList.FindAll(x => x.User.UserId == userId && predicate(x)), ct);
        }
        public async Task<ToDoItem> GetAsync(Guid id, CancellationToken ct)
        {
            return await Task.Run(() => _toDoItemList.Find(x => x.Id == id), ct);
        }
        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        {
            return await Task.Run(() => _toDoItemList.FindAll(x => x.User.UserId == userId && x.State == ToDoItemState.Active), ct);
        }
        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            return await Task.Run(() => _toDoItemList.FindAll(x => x.User.UserId == userId), ct);
        }
        public async Task UpdateAsync(ToDoItem item, CancellationToken ct)
        {
            int index = await Task.Run(() => _toDoItemList.FindIndex(x => x.Id == item.Id), ct);
            await Task.Run(()=>_toDoItemList[index] = item, ct);
        }
    }
}
