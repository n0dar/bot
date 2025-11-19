using System;

namespace bot.TelegramBot.DTO
{
    internal class ToDoItemCallbackDto : CallbackDto
    {
        public Guid? ToDoItemId { get; set; }
        //На вход принимает строку ввида "{action}|{toDoListId}|{prop2}...".
        //Нужно создать ToDoListCallbackDto с Action = action и ToDoListId = toDoListId.
        public static new ToDoItemCallbackDto FromString(string input)
        {
            string[] parts = input.Split('|');
            return new ToDoItemCallbackDto
            {
                Action = parts[0],
                ToDoItemId = (parts[1] == "" ? null : Guid.Parse(parts[1]))
            };
        }
        //переопределить метод.Он должен возвращать $"{base.ToString()}|{ToDoListId}"
        public override string ToString()
        {
            return $"{base.ToString()}|{ToDoItemId}"; 
        }
        
    }
}
