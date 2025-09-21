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
        private readonly string _indexFilePath;
        public FileToDoRepository(string Path)
        {
            _path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path) ;
            Directory.CreateDirectory(_path);
            _indexFilePath = System.IO.Path.Combine(_path, "index.json");
        }
        private static async Task<string> ObjestToJsonStringAsync(object obj, CancellationToken ct)
        {
            await using MemoryStream jsonStream = new();
            await JsonSerializer.SerializeAsync(jsonStream, obj, cancellationToken: ct);
            jsonStream.Position = 0;
            using StreamReader jsonStreamReader = new(jsonStream, Encoding.UTF8);
            return await jsonStreamReader.ReadToEndAsync(ct);
        }
        private async Task BuildIndexAsync(CancellationToken ct)
        {
            IEnumerable<string> files = Directory
                .EnumerateFiles(_path, $"*.json", SearchOption.AllDirectories)
                .Where(file => System.IO.Path.GetFileName(file) != "index.json");

            if (files.Any())
            {
                Dictionary<Guid, Guid> index = [];
                foreach (string file in files)
                {
                    using FileStream jsonFileStream = File.OpenRead(file);
                    ToDoItem item = await JsonSerializer.DeserializeAsync<ToDoItem>(jsonFileStream, cancellationToken: ct);
                    index.Add(item.Id, item.User.UserId);
                }
                await File.WriteAllTextAsync(_indexFilePath, await ObjestToJsonStringAsync(index, ct), ct);
            }
        }
        private async Task<Dictionary<Guid, Guid>> ReadIndexAsync(CancellationToken ct)
        {
            if (File.Exists(_indexFilePath))
            {
                using (FileStream jsonFileStream = File.OpenRead(_indexFilePath))
                {
                    return await JsonSerializer.DeserializeAsync<Dictionary<Guid, Guid>>(jsonFileStream, cancellationToken: ct);
                }
            }
            return null;
        }
        private async Task UpdateIndexAsync(Dictionary<Guid, Guid> index, CancellationToken ct)
        {
            await File.WriteAllTextAsync(_indexFilePath, await ObjestToJsonStringAsync(index, ct), ct);
        }
        private async Task<IReadOnlyList<ToDoItem>> GetItemsAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
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
            Directory.CreateDirectory(Path.Combine(_path, $"{item.User.UserId}"));
            await File.WriteAllTextAsync(Path.Combine(_path, $"{item.User.UserId}", $"{item.Id}.json"), await ObjestToJsonStringAsync(item, ct), ct);
            if (File.Exists(_indexFilePath))
            {
                Dictionary<Guid, Guid> index = await ReadIndexAsync(ct);
                index.Add(item.Id, item.User.UserId);
                await UpdateIndexAsync(index, ct);
            }
            else await BuildIndexAsync(ct);
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
            Dictionary<Guid, Guid> index = await ReadIndexAsync(ct);

            if (index.TryGetValue(id, out Guid userId))
            {
                index.Remove(id);
                await UpdateIndexAsync(index, ct);
                string filePath = Path.Combine(_path, $"{userId}", $"{id}.json");
                if (File.Exists(filePath)) File.Delete(filePath);
                else throw new TaskDoesNotExistException("Задача с таким GUID не существует");
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
            return await GetItemsAsync(userId, predicate, ct);
        }
        async Task<IReadOnlyList<ToDoItem>> IToDoRepository.GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        {
            return await GetItemsAsync(userId, item => item.State == ToDoItemState.Active, ct);
        }
        async Task<IReadOnlyList<ToDoItem>> IToDoRepository.GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            return await GetItemsAsync(userId, item => true, ct);
        }
        async Task<ToDoItem> IToDoRepository.GetAsync(Guid id, CancellationToken ct)
        {
            Dictionary<Guid, Guid> index = await ReadIndexAsync(ct);

            if (index.TryGetValue(id, out Guid userId))
            {
                using FileStream jsonFileStream = File.OpenRead(Path.Combine(_path, $"{userId}", $"{id}.json"));
                return await JsonSerializer.DeserializeAsync<ToDoItem>(jsonFileStream, cancellationToken: ct);
            }
            return null;
        }
        async Task IToDoRepository.UpdateAsync(ToDoItem item, CancellationToken ct)
        {
            Directory.CreateDirectory(Path.Combine(_path, $"{item.User.UserId}"));
            await File.WriteAllTextAsync(Path.Combine(_path, $"{item.User.UserId}", $"{item.Id}.json"), await ObjestToJsonStringAsync(item, ct), ct);
        }
    }
}
