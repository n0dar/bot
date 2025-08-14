using bot.Core.Entities;
using System;
using System.Collections.Generic;

namespace bot.Core.Services.Interfaces   
{
    internal interface IToDoService
    {
        IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId);
        //Возвращает ToDoItem для UserId со статусом Active
        IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId);
        ToDoItem Add(ToDoUser user, string name);
        void MarkCompleted(Guid id);
        void Delete(Guid id);
        //Возвращает все задачи пользователя, которые начинаются на namePrefix
        IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix);
    }
}
