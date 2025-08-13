using System;

namespace bot
{
    public class DuplicateTaskException(string task) : Exception($"Задача \"{task}\" уже существует");
}
