using System;
using System.Text.Json.Serialization;

namespace bot.Core.Entities
{
    public class ToDoList(string Name, ToDoUser User)
    {
        [JsonConstructor]
        public ToDoList(string Name, ToDoUser User, DateTime CreatedAt) : this(Name, User)
        {
            this.Name = Name;
            this.User = User;
            this.CreatedAt = CreatedAt;
        }
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; } = Name;
        public ToDoUser User { get; } = User;
        public DateTime CreatedAt {  get; } = DateTime.UtcNow;
    }
}
