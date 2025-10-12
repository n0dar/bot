#nullable enable
using bot.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace bot.Core.DataAccess
{
    public interface IToDoListRepository
    {
        //Если списка нет, то возвращает null
        Task<ToDoList?> GetAsync(Guid id, CancellationToken ct);
        Task<IReadOnlyList<ToDoList>> GetByUserIdAsync(Guid userId, CancellationToken ct);
        Task AddAsync(ToDoList list, CancellationToken ct);
        Task DeleteAsync(Guid id, CancellationToken ct);
        //Проверяет, если ли у пользователя список с таким именем
        Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct);
    }
}
