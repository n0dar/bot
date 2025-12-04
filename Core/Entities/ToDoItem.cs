#nullable enable
using System;

namespace bot.Core.Entities
{
    internal class ToDoItem(ToDoUser user, string name, DateOnly deadline, ToDoList? list)
    {
        public enum ToDoItemState
        {
            Active,
            Completed
        }
        public Guid Id { get; set; } = Guid.NewGuid();
        public ToDoUser User { get; set; } = user;
        public string Name { get; set; } = name;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ToDoItemState State { get; set; } = ToDoItemState.Active;
        public DateTime? StateChangedAt { get; set; }
        public DateOnly Deadline { get; set; } = deadline;
        public ToDoList? List { get; set; } = list;
        public override string ToString()
        {
            return $"({State}) {Name} - {CreatedAt} - '{Id}'";
        }
    }
}
