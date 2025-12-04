using System;

namespace bot.TelegramBot.DTO
{
    internal class PagedListCallbackDto: ToDoListCallbackDto
    {
        public int Page {  get; set; }

        public static new PagedListCallbackDto FromString(string input)
        {
            string[] parts = input.Split('|');
            return new PagedListCallbackDto
            {
                Action = parts[0],
                ToDoListId = parts.Length > 1  && Guid.TryParse(parts[1], out Guid ToDoListId) ? ToDoListId : null,
                Page = parts.Length > 2 && int.TryParse(parts[2], out int page) ? page : 0
            };
        }
        public override string ToString() 
        {
            return $"{base.ToString()}|{Page}";
        }
    }
}
