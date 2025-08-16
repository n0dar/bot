#nullable enable
using bot;
using bot.Core.DataAccess;
using bot.Core.Entities;
using bot.Core.Exceptions;
using bot.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using static bot.Core.Entities.ToDoItem;

namespace bot.Core.Services.Classes
{
    internal class ToDoService(IToDoRepository toDoRepository) : IToDoService
    {
        private readonly int _taskCountLimit = 100;
        private readonly int _taskLengthLimit = 100;
        
        public ToDoItem Add(ToDoUser user, string name)
        {
            if (toDoRepository.CountActive(user.UserId) == _taskCountLimit) throw new TaskCountLimitException(_taskCountLimit);
            if (name.Length > _taskLengthLimit) throw new TaskLengthLimitException(name.Length, _taskLengthLimit);
            if (toDoRepository.ExistsByName(user.UserId, name)) throw new DuplicateTaskException(name);
            ToDoItem ToDoItem = new(user, name);
            toDoRepository.Add(ToDoItem);
            return ToDoItem;
        }
        public void Delete(Guid id)
        {
            toDoRepository.Delete(id);
        }

        public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
        {
            return toDoRepository.Find(user.UserId, x => x.Name.StartsWith(namePrefix));
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return toDoRepository.GetActiveByUserId(userId);
        }

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return toDoRepository.GetAllByUserId(userId);
        }

        public void MarkCompleted(Guid id)
        {
            ToDoItem? toDoItem = toDoRepository.Get(id);
            if (toDoItem != null)
            {
                toDoItem.State = ToDoItemState.Completed;
                toDoItem.StateChangedAt = DateTime.UtcNow;
                toDoRepository.Update(toDoItem);
            }
            else throw new TaskDoesNotExistException("Активная задача с таким GUID не существует");
        }
    }
}
