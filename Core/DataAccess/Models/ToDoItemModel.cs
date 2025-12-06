#nullable enable
using LinqToDB.Mapping;
using System;
using System.ComponentModel.DataAnnotations;

namespace bot.Core.DataAccess.Models
{
    [Table("toDoItem")]
    public class ToDoItemModel
    {
        [PrimaryKey, Column("id")]
        public Guid Id { get; set; }

        [PrimaryKey, Column("idToDoUser"), NotNull]
        public Guid IdToDoUser { get; set; }

        [Column("name"), NotNull, MaxLength(64)]
        public string Name { get; set; } = string.Empty;

        [Column("createdAt"), NotNull]
        public DateTime CreatedAt { get; set; }

        [Column("state"), NotNull]
        public bool State { get; set; } 

        [Column("stateChangedAt")]
        public DateTime? StateChangedAt { get; set; }

        [Column("deadline"), NotNull]
        public DateOnly Deadline { get; set; }

        [Column("idToDoList")]
        public Guid? IdToDoList { get; set; }

        [LinqToDB.Mapping.Association(ThisKey = nameof(IdToDoUser), OtherKey = nameof(ToDoUserModel.Id))]
        public ToDoUserModel ToDoUser { get; set; } = null!;

        [LinqToDB.Mapping.Association(ThisKey = nameof(IdToDoList), OtherKey = nameof(ToDoListModel.Id))]
        public ToDoListModel? ToDoList { get; set; }
    }
}
