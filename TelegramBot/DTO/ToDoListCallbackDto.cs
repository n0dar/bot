using System;

namespace bot.TelegramBot.DTO
{
    internal class ToDoListCallbackDto : CallbackDto
    {
        public Guid? ToDoListId { get; set; }
        public static new ToDoListCallbackDto FromString(string input)
        {
            string[] parts = input.Split('|');
            return new ToDoListCallbackDto
            {
                Action = parts[0],
                ToDoListId = (parts[1] == "" ? null : Guid.Parse(parts[1]))
            };
        }
        public override string ToString()
        {
            return $"{base.ToString()}|{ToDoListId}"; 
        }
        
    }
}
