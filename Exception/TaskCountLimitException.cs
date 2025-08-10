using System;

namespace bot
{
    public class TaskCountLimitException(int taskCountLimit) : Exception($"Превышено максимальное количество задач равное {taskCountLimit}\r\n");
}
