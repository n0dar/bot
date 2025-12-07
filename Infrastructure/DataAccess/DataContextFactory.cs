using System;

namespace bot.Infrastructure.DataAccess
{
    internal class DataContextFactory : IDataContextFactory<ToDoDataContext>
    {
        string _connectionString;
        public DataContextFactory(string connectionString)
        {
            _connectionString = connectionString;
        }
        public ToDoDataContext CreateDataContext()
        {
            return new ToDoDataContext(_connectionString);
        }
    }
}
