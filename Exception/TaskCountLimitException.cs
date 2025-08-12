using System;

namespace bot
{
    public class TaskCountLimitException(int TaskCountLimit) : Exception($"Превышено максимальное количество задач равное {TaskCountLimit}\r\n");
}
