using bot.Core.DataAccess;
using bot.Core.Entities;
using bot.TelegramBot;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace bot.Infrastructure.DataAccess
{
    internal class FileUserRepository : IUserRepository
    {
        private readonly string _path;
        public FileUserRepository(string Path)
        {
            _path = Path;
            Directory.CreateDirectory(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _path));

        }
        async Task IUserRepository.AddAsync(ToDoUser user, CancellationToken ct)
        {
            await File.WriteAllTextAsync(Path.Combine(_path, $"{user.UserId}.json"), await Utils.ObjestToJsonStringAsync(user, ct), ct);
        }
        Task<ToDoUser> IUserRepository.GetUserAsync(Guid userId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
        async Task<ToDoUser> IUserRepository.GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct)
        {
            foreach (string file in Directory.EnumerateFiles(_path, "*.json"))
            {
                using FileStream jsonFileStream = File.OpenRead(file);
                ToDoUser user = await JsonSerializer.DeserializeAsync<ToDoUser>(jsonFileStream, cancellationToken: ct);
                if (user.TelegramUserId == telegramUserId) return user;
            }
            return null;
        }
    }
}
