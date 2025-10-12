using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace bot.TelegramBot
{
    public static class Utils
    {
        public static async Task<string> ObjestToJsonStringAsync(object obj, CancellationToken ct)
        {
            await using MemoryStream jsonStream = new();
            await JsonSerializer.SerializeAsync(jsonStream, obj, cancellationToken: ct);
            jsonStream.Position = 0;
            using StreamReader jsonStreamReader = new(jsonStream, Encoding.UTF8);
            return await jsonStreamReader.ReadToEndAsync(ct);
        }
    }
}
