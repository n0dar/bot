using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot.TelegramBot.DTO
{
    internal class ToDoListCallbackDto : CallbackDto
    {
        public Guid? ToDoListId { get; set; }
        //На вход принимает строку ввида "{action}|{toDoListId}|{prop2}...".
        //Нужно создать ToDoListCallbackDto с Action = action и ToDoListId = toDoListId.
        public static new ToDoListCallbackDto FromString(string input)
        {
            string[] parts = input.Split('|');
            //if (parts[1] == null) ToDoListId = null;
            //Guid.TryParse(parts[1], out Guid ToDoListId);

            //Guid.TryParse(parts[1], out Guid ToDoListId);

            return new ToDoListCallbackDto
            {
                Action = parts[0],
                ToDoListId = (parts[1] == "" ? null : Guid.Parse(parts[1]))
            };
        }
        //переопределить метод.Он должен возвращать $"{base.ToString()}|{ToDoListId}"
        public override string ToString()
        {
            return $"{base.ToString()}|{ToDoListId}"; 
        }
        
    }
}
