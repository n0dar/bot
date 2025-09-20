using bot.Core.DataAccess;
using bot.Core.Entities;
using bot.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static bot.Core.Entities.ToDoItem;

namespace bot.Infrastructure.DataAccess
{
    internal class FileToDoRepository: IToDoRepository
    {
        private readonly string _path;
        public FileToDoRepository(string Path)
        {
            _path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path) ;
            Directory.CreateDirectory(_path);
        }
        private async Task<IReadOnlyList<ToDoItem>> Get(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            List<ToDoItem> res = [];
            string userPath = Path.Combine(_path, $"{userId}");
            if (Directory.Exists(userPath))
            {
                foreach (string file in Directory.EnumerateFiles(userPath, "*.json"))
                {
                    using FileStream jsonFileStream = File.OpenRead(file);
                    ToDoItem item = await JsonSerializer.DeserializeAsync<ToDoItem>(jsonFileStream, cancellationToken: ct);
                    if (predicate(item)) res.Add(item);
                }
            }
            return (IReadOnlyList<ToDoItem>)res;
        }
        async Task IToDoRepository.AddAsync(ToDoItem item, CancellationToken ct)
        {
            await using MemoryStream jsonStream = new();
            await JsonSerializer.SerializeAsync(jsonStream, item, cancellationToken: ct);
            jsonStream.Position = 0;
            using StreamReader jsonStreamReader = new(jsonStream, Encoding.UTF8);
            string jsonString = await jsonStreamReader.ReadToEndAsync(ct);
            Directory.CreateDirectory(Path.Combine(_path, $"{item.User.UserId}"));
            await File.WriteAllTextAsync(Path.Combine(_path, $"{item.User.UserId}", $"{item.Id}.json"), jsonString, ct);
        }
        async Task<int> IToDoRepository.CountActiveAsync(Guid userId, CancellationToken ct)
        {
            int res = 0;
            string userPath = Path.Combine(_path, $"{userId}");
            if (Directory.Exists(userPath))
            { 
                foreach (string file in Directory.EnumerateFiles(userPath, "*.json"))
                {
                    using FileStream jsonFileStream = File.OpenRead(file);
                    ToDoItem item = await JsonSerializer.DeserializeAsync<ToDoItem>(jsonFileStream, cancellationToken: ct);
                    if (item.State == ToDoItemState.Active) res++;
                }
            }
            return res;
        }
        async Task IToDoRepository.DeleteAsync(Guid id, CancellationToken ct)
        {
            ToDoItem item = await ((IToDoRepository)this).GetAsync(id, ct);
            if (item != null)
            { 
                await Task.Run(() =>
                {
                    ct.ThrowIfCancellationRequested();
                    string filePath = Path.Combine(_path, $"{item.User.UserId}", $"{id}.json");
                    if (File.Exists(filePath)) File.Delete(filePath);
                    else throw new TaskDoesNotExistException("Задача с таким GUID не существует");
                }, ct);
            }
        }
        async Task<bool> IToDoRepository.ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
        {
            string userPath = Path.Combine(_path, $"{userId}");
            if (Directory.Exists(userPath))
            {
                foreach (string file in Directory.EnumerateFiles(userPath, "*.json"))
                {
                    using FileStream jsonFileStream = File.OpenRead(file);
                    ToDoItem item = await JsonSerializer.DeserializeAsync<ToDoItem>(jsonFileStream, cancellationToken: ct);
                    if (item.Name == name) return true;
                }
            }
            return false;
        }
        async Task<IReadOnlyList<ToDoItem>> IToDoRepository.FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            return await Get(userId, predicate, ct);
        }
        async Task<IReadOnlyList<ToDoItem>> IToDoRepository.GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        {
            return await Get(userId, item => item.State == ToDoItemState.Active, ct);
        }
        async Task<IReadOnlyList<ToDoItem>> IToDoRepository.GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            return await Get(userId, item => true, ct);
        }
        async Task<ToDoItem> IToDoRepository.GetAsync(Guid id, CancellationToken ct)
        {
            IEnumerable<string> files = Directory.EnumerateFiles(_path, $"{id}.json", SearchOption.AllDirectories);
            if (files.Any()) 
            {
                using FileStream jsonFileStream = File.OpenRead(files.First());
                return await JsonSerializer.DeserializeAsync<ToDoItem>(jsonFileStream, cancellationToken: ct);
            }
            return null;
        }
        async Task IToDoRepository.UpdateAsync(ToDoItem item, CancellationToken ct)
        {
            await ((IToDoRepository)this).AddAsync(item, ct);
        }
    }
}
