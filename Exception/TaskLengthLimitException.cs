using System;

namespace bot
{
    public class TaskLengthLimitException(int taskLength, int TaskLengthLimit) : Exception($"Длина задачи {taskLength} превышает максимально допустимое значение {TaskLengthLimit}");
}
