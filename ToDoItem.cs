using System;
namespace bot
{
    internal class ToDoItem
    {
        public enum ToDoItemState
        {
            Active,
            Completed
        }
        public Guid Id {get;} 
        public ToDoUser User {get;} 
        public string Name {get;}
        public DateTime CreatedAt {get;}
        public ToDoItemState State {get;set;}
        public DateTime StateChangedAt { get;set; }
        public ToDoItem(ToDoUser user, string name)
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            State = ToDoItemState.Active;
            User = user;
            Name = name;
        }
    }
}
