using System;

namespace bot.TelegramBot.DTO
{
    internal class CallbackDto
    {
        public string Action { get; set; }
        public Guid? Id { get; set; }
        public static CallbackDto FromString(string input)
        {
            string[] parts = input.Split('|');
            return new CallbackDto
            {
                Action = parts[0],
                Id = (parts[1] == "" ? null : Guid.Parse(parts[1]))
            };
        }
        public override string ToString()
        {
            return $"{Action}|{Id}"; 
        }
        
    }
}
