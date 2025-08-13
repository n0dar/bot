using System;

namespace bot.Core.Exceptions
{
    public class DuplicateTaskException(string task) : Exception($"Задача \"{task}\" уже существует");
}
