using System;

namespace bot.Core.Entities
{
    public class ToDoList()//(string Name, ToDoUser User)
    {
        public Guid Id { get; set; }// = Guid.NewGuid();
        public string Name { get; set; }// = Name;
        public ToDoUser User { get; set; }// = User;
        public DateTime CreatedAt {  get; set; }// = DateTime.UtcNow;
    }
}
