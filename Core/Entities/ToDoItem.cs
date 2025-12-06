#nullable enable
using System;

namespace bot.Core.Entities
{
    public class ToDoItem()
    {
        public enum ToDoItemState
        {
            Active = 1,
            Completed = 0
        }
        public Guid Id { get; set; }
        public ToDoUser User { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public ToDoItemState State { get; set; }
        public DateTime? StateChangedAt { get; set; }
        public DateOnly Deadline { get; set; }
        public ToDoList? List { get; set; }
        public override string ToString()
        {
            return $"({State}) {Name} - {CreatedAt} - '{Id}'";
        }
    }
}
