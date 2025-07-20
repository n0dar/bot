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
        
        private ToDoItemState _state;
        private DateTime _stateChangedAt;
        
        public Guid Id {get;} 
        public ToDoUser User {get;} 
        public string Name {get;}
        public DateTime CreatedAt {get;}
        public ToDoItemState State
        {
            get {return _state;}
            set
            {
                _state = value;
                _stateChangedAt = DateTime.UtcNow;
            }
        }
        public DateTime StateChangedAt
        {
            get { return _stateChangedAt; }
        }
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
