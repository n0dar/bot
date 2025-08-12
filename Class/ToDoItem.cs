using Otus.ToDoList.ConsoleBot.Types;
using System;

namespace bot
{
    internal class ToDoItem(ToDoUser user, string name)
    {
        public enum ToDoItemState
        {
            Active,
            Completed
        }
        public Guid Id { get; } = Guid.NewGuid();
        public ToDoUser User { get; } = user;
        public string Name { get; } = name;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ToDoItemState State { get; set; } = ToDoItemState.Active;
        public DateTime? StateChangedAt { get;set; }
        public override string ToString()
        {
            return $"({State}) {Name} - {CreatedAt} - {Id}";
        }
    }
}
