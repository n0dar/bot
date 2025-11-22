using bot.Core.DataAccess;
using bot.Core.Entities;
using bot.Core.Exceptions;
using bot.TelegramBot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace bot.Infrastructure.DataAccess
{
    internal class FileToDoListRepository : IToDoListRepository
    {
        private readonly string _path;
        private readonly string _indexFilePath;
        public FileToDoListRepository(string Path)
        {
            _path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path);
            Directory.CreateDirectory(_path);
            _indexFilePath = System.IO.Path.Combine(_path, "index.json");
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
                    ToDoList list = await JsonSerializer.DeserializeAsync<ToDoList>(jsonFileStream, cancellationToken: ct);
                    index.Add(list.Id, list.User.UserId);
                }
                await File.WriteAllTextAsync(_indexFilePath, await Utils.ObjestToJsonStringAsync<Dictionary<Guid, Guid>>(index, ct), ct);
            }
        }
        private async Task<Dictionary<Guid, Guid>> ReadIndexAsync(CancellationToken ct)
        {
            if (File.Exists(_indexFilePath))
            {
                using FileStream jsonFileStream = File.OpenRead(_indexFilePath);
                return await JsonSerializer.DeserializeAsync<Dictionary<Guid, Guid>>(jsonFileStream, cancellationToken: ct);
            }
            return null;
        }
        private async Task UpdateIndexAsync(Dictionary<Guid, Guid> index, CancellationToken ct)
        {
            await File.WriteAllTextAsync(_indexFilePath, await Utils.ObjestToJsonStringAsync<Dictionary<Guid, Guid>>(index, ct), ct);
        }
        async Task IToDoListRepository.AddAsync(ToDoList list, CancellationToken ct)
        {
            Directory.CreateDirectory(Path.Combine(_path, $"{list.User.UserId}"));
            await File.WriteAllTextAsync(Path.Combine(_path, $"{list.User.UserId}", $"{list.Id}.json"), await Utils.ObjestToJsonStringAsync<ToDoList>(list, ct), ct);
            if (File.Exists(_indexFilePath))
            {
                Dictionary<Guid, Guid> index = await ReadIndexAsync(ct);
                index.Add(list.Id, list.User.UserId);
                await UpdateIndexAsync(index, ct);
            }
            else await BuildIndexAsync(ct);
        }
        async Task IToDoListRepository.DeleteAsync(Guid id, CancellationToken ct)
        {
            Dictionary<Guid, Guid> index = await ReadIndexAsync(ct);

            if (index.TryGetValue(id, out Guid userId))
            {
                index.Remove(id);
                await UpdateIndexAsync(index, ct);
                string filePath = Path.Combine(_path, $"{userId}", $"{id}.json");
                if (File.Exists(filePath)) File.Delete(filePath);
                else throw new ListDoesNotExistException("Задача с таким GUID не существует");
            }
        }
        async Task<bool> IToDoListRepository.ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
        {
            string userPath = Path.Combine(_path, $"{userId}");
            if (Directory.Exists(userPath))
            {
                foreach (string file in Directory.EnumerateFiles(userPath, "*.json"))
                {
                    using FileStream jsonFileStream = File.OpenRead(file);
                    ToDoList item = await JsonSerializer.DeserializeAsync<ToDoList>(jsonFileStream, cancellationToken: ct);
                    if (item.Name == name) return true;
                }
            }
            return false;
        }
        async Task<ToDoList> IToDoListRepository.GetAsync(Guid id, CancellationToken ct)
        {
            Dictionary<Guid, Guid> index = await ReadIndexAsync(ct);

            if (index.TryGetValue(id, out Guid userId))
            {
                using FileStream jsonFileStream = File.OpenRead(Path.Combine(_path, $"{userId}", $"{id}.json"));
                return await JsonSerializer.DeserializeAsync<ToDoList> (jsonFileStream, cancellationToken: ct);
            }
            return null;
        }
        async Task<IReadOnlyList<ToDoList>> IToDoListRepository.GetByUserIdAsync(Guid userId, CancellationToken ct)
        {
            List<ToDoList> res = [];
            string userPath = Path.Combine(_path, $"{userId}");
            if (Directory.Exists(userPath))
            {
                foreach (string file in Directory.EnumerateFiles(userPath, "*.json"))
                {
                    using FileStream jsonFileStream = File.OpenRead(file);
                    ToDoList item = await JsonSerializer.DeserializeAsync<ToDoList>(jsonFileStream, cancellationToken: ct);
                    if (item.User.UserId == userId) res.Add(item);
                }
            }
            return (IReadOnlyList<ToDoList>)res;
        }
    }
}
