using System;
using System.Text.Json.Serialization;

namespace bot.Core.Entities
{
    internal class ToDoItem(ToDoUser user, string name, DateOnly deadline)
    {
        [JsonConstructor]
        public ToDoItem(Guid id, ToDoUser user, string name, DateOnly deadline) : this(user, name, deadline)
        {
            this.Id = id;
            this.User= user;
            this.Name = name;
            this.Deadline = deadline;
        }
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
        public DateTime? StateChangedAt { get; set; }
        public DateOnly Deadline { get; set; }
        public override string ToString()
        {
            return $"({State}) {Name} - {CreatedAt} - '{Id}'";
        }
    }
}
