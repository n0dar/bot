using System;

namespace bot.Core.Exceptions
{
    public class DuplicateToDoListException(string Name) : Exception($"Список \"{Name}\" уже существует");
}
