using System;

namespace bot.Core.Exceptions
{
    public class ToDoListLengthLimitException(int ToDoListLength, int ToDoListLengthLimit) : Exception($"Длина наименования списка {ToDoListLength} превышает максимально допустимое значение {ToDoListLengthLimit}");
}
