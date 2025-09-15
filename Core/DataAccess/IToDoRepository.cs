#nullable enable
using bot.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace bot.Core.DataAccess
{
    internal interface IToDoRepository
    {
        Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct);
        //Возвращает ToDoItem для UserId со статусом Active
        Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct);
        Task<ToDoItem> GetAsync(Guid id, CancellationToken ct);
        Task AddAsync(ToDoItem item, CancellationToken ct);
        Task UpdateAsync(ToDoItem item, CancellationToken ct);
        Task DeleteAsync(Guid id, CancellationToken ct);
        //Проверяет есть ли задача с таким именем у пользователя
        Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct);
        //Возвращает количество активных задач у пользователя
        Task<int> CountActiveAsync(Guid userId, CancellationToken ct);
        //Возвращает все задачи пользователя, которые удовлетворяют предикату
        Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct);
    }
}
