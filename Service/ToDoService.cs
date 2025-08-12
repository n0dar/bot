#nullable enable
using System;
using System.Collections.Generic;
using static bot.ToDoItem;

namespace bot
{
    internal class ToDoService : IToDoService
    {
        public readonly int TaskCountLimit=100;
        public readonly int TaskLengthLimit =100;
        private readonly List<ToDoItem> toDoItemList = [];
        public ToDoItem Add(ToDoUser user, string name)
        {
            if (toDoItemList.Count == TaskCountLimit) throw new TaskCountLimitException(TaskCountLimit);
            if (name.Length > TaskLengthLimit) throw new TaskLengthLimitException(name.Length, TaskLengthLimit);
            if (toDoItemList.Exists(item => item.Name == name)) throw new DuplicateTaskException(name);
            ToDoItem ToDoItem=new(user, name);
            toDoItemList.Add(ToDoItem);
            return ToDoItem;
        }
        public void Delete(Guid id)
        {
            if (toDoItemList.Exists(item => item.Id == id)) toDoItemList.RemoveAll(item => item.Id == id);
            else throw new TaskDoesNotExistException("Задача с таким GUID не существует");
        }
        public IReadOnlyList<ToDoItem> GetActiveByUserId(long userId)
        {
            return toDoItemList.FindAll(item => item.User.TelegramUserId == userId && item.State == ToDoItemState.Active);
        }
        public IReadOnlyList<ToDoItem> GetAllByUserId(long userId)
        {
            return toDoItemList.FindAll(item => item.User.TelegramUserId == userId);
        }
        public void MarkCompleted(Guid id)
        {
            ToDoItem? toDoItem = toDoItemList.Find(item => item.Id == id && item.State == ToDoItemState.Active);
            if (toDoItem != null)
            {
                toDoItem.State = ToDoItemState.Completed;
                toDoItem.StateChangedAt = DateTime.UtcNow;
            }
            else throw new TaskDoesNotExistException("Активная задача с таким GUID не существует");
        }
    }
}
