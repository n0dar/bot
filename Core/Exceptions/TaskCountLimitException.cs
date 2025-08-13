using System;

namespace bot.Core.Exceptions
{
    public class TaskCountLimitException(int TaskCountLimit) : Exception($"Превышено максимальное количество активных задач равное {TaskCountLimit}\r\n");
}
