namespace bot.Infrastructure.DataAccess
{
    internal class DataContextFactory : IDataContextFactory<ToDoDataContext>
    {
        public ToDoDataContext CreateDataContext()
        {
            return new ToDoDataContext("");
        }
    }
}
