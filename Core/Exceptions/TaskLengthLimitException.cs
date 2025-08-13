using System;

namespace bot.Core.Exceptions
{
    public class TaskLengthLimitException(int taskLength, int TaskLengthLimit) : Exception($"Длина наименования задачи {taskLength} превышает максимально допустимое значение {TaskLengthLimit}");
}
