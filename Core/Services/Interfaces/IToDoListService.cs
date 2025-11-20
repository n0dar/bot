#nullable enable
using bot.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace bot.Core.Services.Interfaces
{
    public interface IToDoListService
    {
        Task<ToDoList> AddAsync(ToDoUser user, string name, CancellationToken ct);
        Task<ToDoList?> GetAsync(Guid id, CancellationToken ct);
        Task DeleteAsync(Guid id, CancellationToken ct);
        Task<IReadOnlyList<ToDoList>> GetUserListsAsync(Guid userId, CancellationToken ct);
    }
}
