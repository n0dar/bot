using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot.Core.Entities
{
    public class ToDoList
    {
        Guid Id { get; set; }
        string Name {  get; set; }
        ToDoUser User {  get; set; }
        DateTime CreatedAt {  get; set; }
    }
}
