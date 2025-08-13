using bot.Core.DataAccess;
using bot.Core.Entities;
using bot.Core.Exceptions;
using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
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
        public int CountActive(long userId)
        {
            return _toDoItemList.FindAll(x => x.User.TelegramUserId == userId && x.State == ToDoItemState.Active).Count;
        }
        public void Delete(Guid id)
        {
            if (_toDoItemList.Exists(x => x.Id == id)) _toDoItemList.RemoveAll(x => x.Id == id);
            else throw new TaskDoesNotExistException("Задача с таким GUID не существует");
        }
        public bool ExistsByName(long userId, string name)
        {
            return _toDoItemList.Exists(x => x.User.TelegramUserId == userId && x.Name == name);
        }
        public ToDoItem Get(Guid id)
        {
            return _toDoItemList.Find(x => x.Id == id);
        }
        public IReadOnlyList<ToDoItem> GetActiveByUserId(long userId)
        {
            return _toDoItemList.FindAll(x => x.User.TelegramUserId == userId && x.State == ToDoItemState.Active);
        }
        public IReadOnlyList<ToDoItem> GetAllByUserId(long userId)
        {
            return _toDoItemList.FindAll(x => x.User.TelegramUserId == userId);
        }
        public void Update(ToDoItem item)
        {
            _toDoItemList[_toDoItemList.FindIndex(x => x.Id == item.Id)] = item;
        }
    }
}
