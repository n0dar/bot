using System;

namespace bot.Core.Entities
{
    internal class Notification
    {
        Guid Id { get; set; }
        ToDoUser User { get; set; }
        string Type { get; set; }//Тип нотификации. Например: DeadLine_{ToDoItem.Id}, Today_{DateOnly.FromDateTime(DateTime.UtcNow)}
        string Text { get; set; }//Текст, который будет отправлен
        DateTime ScheduledAt { get; set; }//Запланированная дата отправки
        bool IsNotified { get; set; }//Флаг отправки
        DateTime? NotifiedAt { get; set; }//Фактическая дата отправки
    }
}
