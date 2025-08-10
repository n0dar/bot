using System;

namespace bot
{
    internal class TaskDoesNotExistException(string description) : Exception(description);
}
