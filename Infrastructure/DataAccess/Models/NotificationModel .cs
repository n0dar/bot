using bot.Core.DataAccess.Models;
using LinqToDB.Mapping;
using System;
using System.ComponentModel.DataAnnotations;

namespace bot.Infrastructure.DataAccess.Models
{
    [Table("notification")]
    internal class NotificationModel
    {
        [PrimaryKey, Column("id")]                  public Guid Id { get; set; }
        [PrimaryKey, Column("idToDoUser"), NotNull] public Guid IdToDoUser { get; set; }
        [Column("type"), NotNull, MaxLength(64)]    public string Type { get; set; }//Тип нотификации. Например: DeadLine_{ToDoItem.Id}, Today_{DateOnly.FromDateTime(DateTime.UtcNow)}
        [Column("text"), NotNull, MaxLength(64)]    public string Text { get; set; }//Текст, который будет отправлен
        [Column("scheduledAt"), NotNull]            public DateTime ScheduledAt { get; set; }//Запланированная дата отправки
        [Column("isNotified"), NotNull]             public bool IsNotified { get; set; }//Флаг отправки
        [Column("notifiedAt")]                      public DateTime? NotifiedAt { get; set; }//Фактическая дата отправки    

        [LinqToDB.Mapping.Association(ThisKey = nameof(IdToDoUser), OtherKey = nameof(ToDoUserModel.Id))]
                                                    public ToDoUserModel ToDoUser { get; set; }

    }
}
