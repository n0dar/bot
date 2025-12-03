using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot.TelegramBot.DTO
{
    internal class CallbackDto
    {
        public string Action { get; set; } 
        public static CallbackDto FromString(string input) 
        {
            string[] parts = input.Split('|');
            return new CallbackDto
            {
                Action = parts[0]
            };
        }
        public override string ToString()
        {
            return Action;
        }
    }
}
