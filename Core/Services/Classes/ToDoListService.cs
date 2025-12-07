using bot.Core.DataAccess;
using bot.Core.Entities;
using bot.Core.Exceptions;
using bot.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace bot.Core.Services.Classes
{
    internal class ToDoListService(IToDoListRepository toDoListRepository) : IToDoListService
    {
        private readonly int _toDoListLengthLimit = 10;
        async Task<ToDoList> IToDoListService.AddAsync(ToDoUser user, string name, CancellationToken ct)
        {
            if (name.Length > _toDoListLengthLimit) throw new ToDoListLengthLimitException(name.Length, _toDoListLengthLimit);
            if (await toDoListRepository.ExistsByNameAsync(user.UserId, name, ct)) throw new DuplicateToDoListException(name);
            ToDoList toDoList = new ToDoList
            {
                Id = Guid.NewGuid(),
                Name = name,
                User = user,
                CreatedAt = DateTime.Now,
                
            };
            await toDoListRepository.AddAsync(toDoList, ct);
            return toDoList;
        }
        async Task IToDoListService.DeleteAsync(Guid id, CancellationToken ct)
        {
            await toDoListRepository.DeleteAsync(id, ct);
        }
        async Task<ToDoList> IToDoListService.GetAsync(Guid id, CancellationToken ct)
        {
            return await toDoListRepository.GetAsync(id, ct);
        }
        async Task<IReadOnlyList<ToDoList>> IToDoListService.GetUserListsAsync(Guid userId, CancellationToken ct)
        {
            return await toDoListRepository.GetByUserIdAsync(userId, ct);
        }
    }
}
