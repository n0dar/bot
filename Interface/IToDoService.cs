using System;
using System.Collections.Generic;

namespace bot
{
    internal interface IToDoService
    {
        public int TaskCountLimit { get; set; }
        public int TaskLengthLimit { get; set; }
        IReadOnlyList<ToDoItem> GetAllByUserId(long userId);
        //Возвращает ToDoItem для UserId со статусом Active
        IReadOnlyList<ToDoItem> GetActiveByUserId(long userId);
        ToDoItem Add(ToDoUser user, string name);
        void MarkCompleted(Guid id);
        void Delete(Guid id);
    }
}
