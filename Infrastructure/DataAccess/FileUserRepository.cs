using bot.Core.DataAccess;
using bot.Core.Entities;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

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
            await using MemoryStream jsonStream = new();
            await JsonSerializer.SerializeAsync(jsonStream, user, cancellationToken: ct);
            jsonStream.Position = 0;
            using StreamReader jsonStreamReader = new(jsonStream, Encoding.UTF8);
            string jsonString = await jsonStreamReader.ReadToEndAsync(ct);
            await File.WriteAllTextAsync(Path.Combine(_path, $"{user.UserId}.json"), jsonString, ct);
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
