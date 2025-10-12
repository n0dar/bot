using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot.TelegramBot.DTO
{
    internal class CallbackDto
    {
        //с помощью него будет определять за какое действие отвечает кнопка
        public string Action { get; set; } 
        //На вход принимает строку ввида "{action}|{prop1}|{prop2}...".
        //Нужно создать CallbackDto с Action = action.
        //Нужно учесть что в строке может не быть |, тогда всю строку сохраняем в Action.
        public static CallbackDto FromString(string input) 
        {
            string[] parts = input.Split('|');
            return new CallbackDto
            {
                //Action = parts.Length > 1 ? parts[0] : input
                Action = parts[0]
            };
        }
        //- переопределить метод.Он должен возвращать Action
        public override string ToString()
        {
            return Action;
        }
    }
}
