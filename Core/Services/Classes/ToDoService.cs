#nullable enable
using bot.Core.DataAccess;
using bot.Core.Entities;
using bot.Core.Exceptions;
using bot.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static bot.Core.Entities.ToDoItem;

namespace bot.Core.Services.Classes
{
    internal class ToDoService(IToDoRepository toDoRepository) : IToDoService
    {
        private readonly int _taskCountLimit = 100;
        private readonly int _taskLengthLimit = 100;
        public async Task<ToDoItem> AddAsync(ToDoUser user, string name, CancellationToken ct)
        {
            int countActive = await toDoRepository.CountActiveAsync(user.UserId, ct);
            if (countActive == _taskCountLimit) throw new TaskCountLimitException(_taskCountLimit);
            if (name.Length > _taskLengthLimit) throw new TaskLengthLimitException(name.Length, _taskLengthLimit);
            if (await toDoRepository.ExistsByNameAsync(user.UserId, name, ct)) throw new DuplicateTaskException(name);
            ToDoItem ToDoItem = new(user, name);
            await toDoRepository.AddAsync(ToDoItem, ct);
            return ToDoItem;
        }
        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            await toDoRepository.DeleteAsync(id, ct);
        }
        public async Task<IReadOnlyList<ToDoItem>> FindAsync(ToDoUser user, string namePrefix, CancellationToken ct)
        {
            return await toDoRepository.FindAsync(user.UserId, x => x.Name.StartsWith(namePrefix), ct);
        }
        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
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
                toDoItem.StateChangedAt = DateTime.UtcNow;
                await toDoRepository.UpdateAsync(toDoItem, ct);
            }
            else throw new TaskDoesNotExistException("Активная задача с таким GUID не существует");
        }
    }
}
