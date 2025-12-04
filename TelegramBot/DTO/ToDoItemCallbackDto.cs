using System;

namespace bot.TelegramBot.DTO
{
    internal class ToDoItemCallbackDto : CallbackDto
    {
        public Guid? ToDoItemId { get; set; }
        public static new ToDoItemCallbackDto FromString(string input)
        {
            string[] parts = input.Split('|');
            return new ToDoItemCallbackDto
            {
                Action = parts[0],
                ToDoItemId = (parts[1] == "" ? null : Guid.Parse(parts[1]))
            };
        }
        public override string ToString()
        {
            return $"{base.ToString()}|{ToDoItemId}"; 
        }
        
    }
}
