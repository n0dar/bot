using bot.Core.DataAccess;
using bot.Core.Entities;
using bot.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using static bot.Core.Entities.ToDoItem;

namespace bot.Infrastructure.DataAccess
{
    internal class FileToDoRepository: IToDoRepository
    {
        private readonly string _path;
        public FileToDoRepository(string Path)
        {
            _path = Path;
            Directory.CreateDirectory(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _path));
        }
        async Task IToDoRepository.AddAsync(ToDoItem item, CancellationToken ct)
        {
            await using MemoryStream jsonStream = new();
            await JsonSerializer.SerializeAsync(jsonStream, item, cancellationToken: ct);
            jsonStream.Position = 0;
            using StreamReader jsonStreamReader = new(jsonStream, Encoding.UTF8);
            string jsonString = await jsonStreamReader.ReadToEndAsync(ct);
            await File.WriteAllTextAsync(Path.Combine(_path, $"{item.Id}.json"), jsonString, ct);
        }
        async Task<int> IToDoRepository.CountActiveAsync(Guid userId, CancellationToken ct)
        {
            int res = 0;
            foreach (string file in Directory.EnumerateFiles(_path, "*.json"))
            {
                using FileStream jsonFileStream = File.OpenRead(file);
                ToDoItem item = await JsonSerializer.DeserializeAsync<ToDoItem>(jsonFileStream, cancellationToken: ct);
                if (item.User.UserId == userId) res++;
            }
            return res;
        }
        async Task IToDoRepository.DeleteAsync(Guid id, CancellationToken ct)
        {
            await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                string filePath = Path.Combine(_path, $"{id}.json");
                if (File.Exists(filePath)) File.Delete(filePath);
                else throw new TaskDoesNotExistException("Задача с таким GUID не существует");
            }, ct);
        }
        async Task<bool> IToDoRepository.ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
        {
            foreach (string file in Directory.EnumerateFiles(_path, "*.json"))
            {
                using FileStream jsonFileStream = File.OpenRead(file);
                ToDoItem item = await JsonSerializer.DeserializeAsync<ToDoItem>(jsonFileStream, cancellationToken: ct);
                if (item.User.UserId == userId && item.Name == name) return true;   
            }
            return false;
        }
        async Task<IReadOnlyList<ToDoItem>> IToDoRepository.FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            List<ToDoItem> res = [];
            foreach (string file in Directory.EnumerateFiles(_path, "*.json"))
            {
                using FileStream jsonFileStream = File.OpenRead(file);
                ToDoItem item = await JsonSerializer.DeserializeAsync<ToDoItem>(jsonFileStream, cancellationToken: ct);
                if (item.User.UserId == userId && predicate(item)) res.Add(item);
            }
            return (IReadOnlyList<ToDoItem>)res;
        }
        async Task<IReadOnlyList<ToDoItem>> IToDoRepository.GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        {
            List<ToDoItem> res = [];
            foreach (string file in Directory.EnumerateFiles(_path, "*.json"))
            {
                using FileStream jsonFileStream = File.OpenRead(file);
                ToDoItem item = await JsonSerializer.DeserializeAsync<ToDoItem>(jsonFileStream, cancellationToken: ct);
                if (item.User.UserId == userId && item.State == ToDoItemState.Active) res.Add(item);
            }
            return (IReadOnlyList<ToDoItem>)res;
        }
        async Task<IReadOnlyList<ToDoItem>> IToDoRepository.GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            List<ToDoItem> res = [];
            foreach (string file in Directory.EnumerateFiles(_path, "*.json"))
            {
                using FileStream jsonFileStream = File.OpenRead(file);
                ToDoItem item = await JsonSerializer.DeserializeAsync<ToDoItem>(jsonFileStream, cancellationToken: ct);
                if (item.User.UserId == userId)
                {
                    res.Add(item);
                }
            }
            return (IReadOnlyList<ToDoItem>)res;
        }
        async Task<ToDoItem> IToDoRepository.GetAsync(Guid id, CancellationToken ct)
        {
            string filePath = Path.Combine(_path, $"{id}.json");
            if (File.Exists(filePath)) 
            {
                using FileStream jsonFileStream = File.OpenRead(filePath);
                return await JsonSerializer.DeserializeAsync<ToDoItem>(jsonFileStream, cancellationToken: ct);
            }
            return null;
        }
        async Task IToDoRepository.UpdateAsync(ToDoItem item, CancellationToken ct)
        {
            await using MemoryStream jsonStream = new();
            await JsonSerializer.SerializeAsync(jsonStream, item, cancellationToken: ct);
            jsonStream.Position = 0;
            using StreamReader jsonStreamReader = new(jsonStream, Encoding.UTF8);
            string jsonString = await jsonStreamReader.ReadToEndAsync(ct);
            await File.WriteAllTextAsync(Path.Combine(_path, $"{item.Id}.json"), jsonString, ct);
        }
    }
}
