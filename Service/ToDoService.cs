#nullable enable
using System;
using System.Collections.Generic;
using static bot.ToDoItem;

namespace bot
{
    internal class ToDoService : IToDoService
    {
        private int _taskCountLimit;
        private int _taskLengthLimit;
        private readonly int _taskCountMax = 100;
        private readonly int _taskLengthMax = 100;
        private List<ToDoItem> _toDoItemList = [];

        public int TaskCountLimit 
        {
            get { return _taskCountLimit;}
            set 
            {
                if (value < 1 || value > _taskCountMax) throw new ArgumentException($"Значение должно находиться в интервале [1;{_taskCountMax}]");
                _taskCountLimit = value;
            }
        }
        public int TaskLengthLimit
        {
            get { return _taskLengthLimit;}
            set
            {
                if (value < 1 || value > _taskLengthMax) throw new ArgumentException($"Значение должно находиться в интервале [1;{_taskLengthMax}]");
                _taskLengthLimit= value;
            }
        }
        public ToDoItem Add(ToDoUser user, string name)
        {
            if (_toDoItemList.Count == _taskCountLimit) throw new TaskCountLimitException(_taskCountLimit);
            if (name.Length > _taskLengthLimit) throw new TaskLengthLimitException(name.Length, _taskLengthLimit);
            if (_toDoItemList.Exists(item => item.Name == name)) throw new DuplicateTaskException(name);
            ToDoItem ToDoItem=new(user, name);
            _toDoItemList.Add(ToDoItem);
            return ToDoItem;
        }
        public void Delete(Guid id)
        {
            if (_toDoItemList.Exists(item => item.Id == id)) _toDoItemList.RemoveAll(item => item.Id == id);
            else throw new TaskDoesNotExistException("Задача с таким GUID не существует");
        }
        public IReadOnlyList<ToDoItem> GetActiveByUserId(long userId)
        {
            return _toDoItemList.FindAll(item => item.User.TelegramUserId == userId && item.State == ToDoItemState.Active);
        }
        public IReadOnlyList<ToDoItem> GetAllByUserId(long userId)
        {
            return _toDoItemList.FindAll(item => item.User.TelegramUserId == userId);
        }
        public void MarkCompleted(Guid id)
        {
            ToDoItem? toDoItem = _toDoItemList.Find(item => item.Id == id && item.State == ToDoItemState.Active);
            if (toDoItem != null)
            {
                toDoItem.State = ToDoItemState.Completed;
                toDoItem.StateChangedAt = DateTime.UtcNow;
            }
            else throw new TaskDoesNotExistException("Активная задача с таким GUID не существует");
        }
    }
}
