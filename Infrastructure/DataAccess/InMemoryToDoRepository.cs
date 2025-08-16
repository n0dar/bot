using bot.Core.DataAccess;
using bot.Core.Entities;
using bot.Core.Exceptions;
using System;
using System.Collections.Generic;
using static bot.Core.Entities.ToDoItem;

namespace bot.Infrastructure.DataAccess
{
    internal class InMemoryToDoRepository : IToDoRepository
    {
        public readonly List<ToDoItem> _toDoItemList = [];
        public void Add(ToDoItem item)
        {
            _toDoItemList.Add(item);
        }
        public int CountActive(Guid userId)
        {
            return _toDoItemList.FindAll(x => x.User.UserId == userId && x.State == ToDoItemState.Active).Count;
        }
        public void Delete(Guid id)
        {
            if (_toDoItemList.Exists(x => x.Id == id)) _toDoItemList.RemoveAll(x => x.Id == id);
            else throw new TaskDoesNotExistException("Задача с таким GUID не существует");
        }
        public bool ExistsByName(Guid userId, string name)
        {
            return _toDoItemList.Exists(x => x.User.UserId == userId && x.Name == name);
        }

        public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
        {
            //throw new NotImplementedException();
            return _toDoItemList.FindAll(x => x.User.UserId == userId && predicate(x));
        }

        public ToDoItem Get(Guid id)
        {
            return _toDoItemList.Find(x => x.Id == id);
        }
        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _toDoItemList.FindAll(x => x.User.UserId == userId && x.State == ToDoItemState.Active);
        }
        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _toDoItemList.FindAll(x => x.User.UserId == userId);
        }
        public void Update(ToDoItem item)
        {
            _toDoItemList[_toDoItemList.FindIndex(x => x.Id == item.Id)] = item;
        }
    }
}
