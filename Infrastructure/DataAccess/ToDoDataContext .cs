using bot.Core.DataAccess.Models;
using bot.Infrastructure.DataAccess.Models;
using LinqToDB;
using LinqToDB.Data;

namespace bot.Infrastructure.DataAccess
{
    internal class ToDoDataContext : DataConnection
    {
        public ToDoDataContext(string connectionString) : base(ProviderName.PostgreSQL, connectionString){}
        public ITable<ToDoUserModel> ToDoUser => this.GetTable<ToDoUserModel>();
        public ITable<ToDoListModel> ToDoList => this.GetTable<ToDoListModel>();
        public ITable<ToDoItemModel> ToDoItem => this.GetTable<ToDoItemModel>();
        public ITable<NotificationModel> Notifications => this.GetTable<NotificationModel>();

    }
}
