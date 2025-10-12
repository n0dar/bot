using System;

namespace bot.Core.Exceptions
{
    internal class ListDoesNotExistException(string description) : Exception(description);
}
