using bot.Core.Entities;
using System;
using System.Collections.Generic;

namespace bot.Core.Services.Interfaces   
{
    internal interface IToDoService
    {
        IReadOnlyList<ToDoItem> GetAllByUserId(long userId);
        //Возвращает ToDoItem для UserId со статусом Active
        IReadOnlyList<ToDoItem> GetActiveByUserId(long userId);
        ToDoItem Add(ToDoUser user, string name);
        void MarkCompleted(Guid id);
        void Delete(Guid id);
    }
}
