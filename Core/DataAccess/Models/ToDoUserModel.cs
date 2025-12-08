using LinqToDB.Mapping;
using System;
using System.ComponentModel.DataAnnotations;

namespace bot.Core.DataAccess.Models
{
    [Table("toDoUser")]
    public class ToDoUserModel
    {
        [PrimaryKey, Column("id")]                              public Guid Id { get; set; }
        [Column("telegramUserId"), NotNull]                     public long TelegramUserId { get; set; }
        [Column("telegramUserName"), NotNull, MaxLength(32)]    public string TelegramUserName { get; set; }
        [Column("registeredAt")]                                public DateTime RegisteredAt { get; set; }
    }
}
