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
        private readonly int _TaskCountLimit = 100;
        private readonly int _TaskLengthLimit = 100;
        private readonly IToDoRepository _toDoRepository = toDoRepository;
        
        public ToDoItem Add(ToDoUser user, string name)
        {
            if (_toDoRepository.CountActive(user.UserId) == _TaskCountLimit) throw new TaskCountLimitException(_TaskCountLimit);
            if (name.Length > _TaskLengthLimit) throw new TaskLengthLimitException(name.Length, _TaskLengthLimit);
            if (_toDoRepository.ExistsByName(user.UserId, name)) throw new DuplicateTaskException(name);
            ToDoItem ToDoItem = new(user, name);
            _toDoRepository.Add(ToDoItem);
            return ToDoItem;
        }
        public void Delete(Guid id)
        {
            _toDoRepository.Delete(id);
        }

        public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
        {
            return _toDoRepository.Find(user.UserId, x => x.Name.StartsWith(namePrefix));
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _toDoRepository.GetActiveByUserId(userId);
        }

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _toDoRepository.GetAllByUserId(userId);
        }

        public void MarkCompleted(Guid id)
        {
            ToDoItem? toDoItem = _toDoRepository.Get(id);
            if (toDoItem != null)
            {
                toDoItem.State = ToDoItemState.Completed;
                toDoItem.StateChangedAt = DateTime.UtcNow;
                _toDoRepository.Update(toDoItem);
            }
            else throw new TaskDoesNotExistException("Активная задача с таким GUID не существует");
        }
    }
}
