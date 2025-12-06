using LinqToDB.Mapping;
using System;
using System.ComponentModel.DataAnnotations;

namespace bot.Core.DataAccess.Models
{
    [Table("toDoList")]
    public class ToDoListModel
    {
        [PrimaryKey, Column("id")]
        public Guid Id { get; set; }

        [Column("name"), NotNull, MaxLength(64)]
        public string Name { get; set; } = string.Empty;

        [Column("idToDoUser"), NotNull]
        public Guid IdToDoUser { get; set; }

        [Column("createdAt"), NotNull]
        public DateTime CreatedAt { get; set; }

        [LinqToDB.Mapping.Association(ThisKey = nameof(IdToDoUser), OtherKey = nameof(ToDoUserModel.Id))]
        public ToDoUserModel ToDoUser { get; set; } = null!;
    }
}
