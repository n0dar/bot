using System;

namespace bot.Core.Exceptions
{
    internal class TaskDoesNotExistException(string description) : Exception(description);
}
