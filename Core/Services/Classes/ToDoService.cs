#nullable enable
using bot.Core.DataAccess;
using bot.Core.Entities;
using bot.Core.Exceptions;
using bot.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static bot.Core.Entities.ToDoItem;

namespace bot.Core.Services.Classes
{
    internal class ToDoService(IToDoRepository toDoRepository) : IToDoService
    {
        private readonly int _taskCountLimit = 100;
        private readonly int _taskLengthLimit = 100;
        public async Task<ToDoItem> AddAsync(ToDoUser user, string name, DateOnly deadline, ToDoList? list, CancellationToken ct)
        {
            int countActive = await toDoRepository.CountActiveAsync(user.UserId, ct);
            if (countActive == _taskCountLimit) throw new TaskCountLimitException(_taskCountLimit);
            if (name.Length > _taskLengthLimit) throw new TaskLengthLimitException(name.Length, _taskLengthLimit);
            if (await toDoRepository.ExistsByNameAsync(user.UserId, name, ct)) throw new DuplicateTaskException(name);
            ToDoItem toDoItem = new ToDoItem
            {
                Id = Guid.NewGuid(),
                User = user,
                Name = name,
                CreatedAt = DateTime.Now,
                State = ToDoItemState.Active,
                Deadline = deadline,
                List = list,
            };
            await toDoRepository.AddAsync(toDoItem, ct);
            return toDoItem;
        }
        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            await toDoRepository.DeleteAsync(id, ct);
        }
        public async Task<IReadOnlyList<ToDoItem>> FindAsync(ToDoUser user, string namePrefix, CancellationToken ct)
        {
            //return await toDoRepository.FindAsync(user.UserId, x => x.Name.StartsWith(namePrefix), ct);
            throw new NotImplementedException();
        }
        public async Task<IReadOnlyList<ToDoItem>?> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        {
            return await toDoRepository.GetActiveByUserIdAsync(userId, ct);
        }
        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            return await toDoRepository.GetAllByUserIdAsync(userId, ct);
        }
        public async Task MarkCompletedAsync(Guid id, CancellationToken ct)
        {
            ToDoItem? toDoItem = await toDoRepository.GetAsync(id, ct);
            if (toDoItem != null)
            {
                toDoItem.State = ToDoItemState.Completed;
                toDoItem.StateChangedAt = DateTime.Now;
                await toDoRepository.UpdateAsync(toDoItem, ct);
            }
            else throw new TaskDoesNotExistException("Активная задача с таким GUID не существует");
        }
        public async Task<IReadOnlyList<ToDoItem>?> GetByUserIdAndListAsync(Guid userId, Guid? listId, ToDoItemState? toDoItemState, CancellationToken ct)
        {
            IReadOnlyList<ToDoItem> toDoItems = await GetAllByUserIdAsync(userId, ct);
            return [.. toDoItems.Where(toDoItem => toDoItem.List?.Id == listId && toDoItem.State == (toDoItemState ?? toDoItem.State))]; ;
        }
        public async Task<int> DeleteByUserIdAndListAsync(Guid userId, Guid listId, CancellationToken ct)
        {
            IReadOnlyList<ToDoItem> toDoItems = await GetAllByUserIdAsync(userId, ct);
            toDoItems = [.. toDoItems.Where(toDoItems => toDoItems.List != null).Where(toDoItems => toDoItems.List.Id == listId)];
            foreach (ToDoItem item in toDoItems)
            {
                await DeleteAsync(item.Id, ct);
            }
            return toDoItems.Count;
        }

        public async Task<ToDoItem?> GetAsync(Guid toDoItemId, CancellationToken ct)
        {
            return await toDoRepository.GetAsync(toDoItemId, ct);
        }

    }
}
