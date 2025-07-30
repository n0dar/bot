#nullable enable
using System;
using System.Collections.Generic;
using static bot.ToDoItem;

namespace bot
{
    class Program
    {
        private const int           taskCountMax = 100;
        private const int           taskLengthMax = 100;

        private static bool         isStarted = false;
        private static string       command = "";
        private static int          taskCountLimit;
        private static int          taskLengthLimit;

        private static ToDoUser? toDoUser;
        private static List<ToDoItem> toDoItemList = [];
        private static int ParseAndValidateInt(string? str, int min, int max)
        {
            int.TryParse(str, out int res);
            if (res < min || res > max) throw new ArgumentException($"Значение должно находиться в интервале [{min};{max}]");
            return res;
        }
        private static string ReadLine() 
        {
            string? res = Console.ReadLine();
            if ((res ?? "").Trim() == "") throw new ArgumentException("Значение не может быть пустым");
            return res;
        }
        private static string GetStringDependsOnUserName(string str)
        {
            if (toDoUser is null || toDoUser.TelegramUserName=="") return char.ToUpper(str[0]) + str[1..];
            return toDoUser.TelegramUserName + ", " + char.ToLower(str[0]) + str[1..];
        }
        private static void Start()
        {
            string userName;
            do
            {
                Console.WriteLine("Введите ваше имя...\r\n");
                userName = ReadLine();
            }
            while (userName.Trim() == "");
            if (userName == "/exit") command = "/exit";
            else
            {
                Console.Clear();
                isStarted = true;
                toDoUser = new(userName);
            }
        }
        private static void AddTask()
        {
            if (toDoItemList.Count == taskCountLimit) throw new TaskCountLimitException(taskCountLimit);

            string taskName;
            do
            {
                Console.WriteLine(GetStringDependsOnUserName("Введите описание задачи...\r\n"));
                taskName = ReadLine().Trim();
            }
            while (taskName == "");

            if (taskName.Length > taskLengthLimit) throw new TaskLengthLimitException(taskName.Length, taskLengthLimit);
            if (toDoItemList.Exists(item => item.Name == taskName)) throw new DuplicateTaskException(taskName);
            toDoItemList.Add(new(toDoUser, taskName));
            Console.WriteLine("Задача добавлена.\r\n");
        }
        private static void ShowTasks(string msg)
        {
            if (toDoItemList.Exists(item => item.State == ToDoItemState.Active))
            {
                Console.WriteLine(GetStringDependsOnUserName(msg));
                foreach (ToDoItem item in toDoItemList.FindAll(item => item.State == ToDoItemState.Active))
                {
                    Console.WriteLine($"{item.Name} - {item.CreatedAt} - {item.Id}");
                }
            }
            else Console.WriteLine("Список активных задач пуст.\r\n");
        }
        private static void ShowAllTasks(string msg)
        {
            if (toDoItemList.Count > 0)
            {
                Console.WriteLine(GetStringDependsOnUserName(msg));
                foreach (ToDoItem item in toDoItemList)
                {
                    Console.WriteLine($"({item.State}) {item.Name} - {item.CreatedAt} - {item.Id}");
                }
            }
            else Console.WriteLine("Список задач пуст.\r\n");
        }
        private static void RemoveTask()
        {
            if (toDoItemList.Count > 0)
            {
                ShowAllTasks("Укажите порядковый номер задачи, которую необходимо удалить:");
                if (int.TryParse(ReadLine(), out int taskNumber))
                {
                    if (taskNumber != 0 && taskNumber <= toDoItemList.Count)
                    {
                        toDoItemList.RemoveAt(--taskNumber);
                        Console.WriteLine("Задача удалена.\r\n");
                    }
                    else Console.WriteLine("Задача с таким номером не существует.\r\n");
                }
                else Console.WriteLine("Некорректный номер задачи.\r\n");
            }
            else Console.WriteLine("Список задач пуст.\r\n");
        }
        private static void CompleteTask()
        {
            if (toDoItemList.Exists(item => item.State == ToDoItemState.Active))
            {
                ShowTasks("Укажите Id активной задачи, которую необходимо завершить:");
                string id = ReadLine();
                ToDoItem? ToDoItem = toDoItemList.Find(item => item.Id.ToString() == id && item.State == ToDoItemState.Active);
                if (ToDoItem != null)
                {
                    ToDoItem.State = ToDoItemState.Completed;
                    ToDoItem.StateChangedAt = DateTime.UtcNow;
                    Console.WriteLine("Задача завершена.\r\n");
                }
                else Console.WriteLine("Активная задача с таким Id не существует.\r\n");
            }
            else Console.WriteLine("В списке задач нет активных задач\r\n");
        }
        private static void Help()
        {
            Console.WriteLine
            (
                GetStringDependsOnUserName("Для взаимодействия со мной вам доступен следующий список команд:\r\n\r\n") +
                (isStarted ? "" : "/start        — начните работу с этой команды;\r\n")  +
                (isStarted ?      "/addtask      — добавлю задачу в список;\r\n" : "") +
                (isStarted ?      "/showalltasks — покажу список всех задач;\r\n" : "") +
                (isStarted ?      "/showtasks    — покажу список активных задач;\r\n" : "") +
                (isStarted ?      "/removetask   — удалю задачу из списка;\r\n" : "") +
                (isStarted ?      "/completetask — изменю статус задачи с \"Активна\" на \"Выполнена\";\r\n" : "") +
                (isStarted ?      "/echo         — отображу введенную вами строку текста, указанную после команды через пробел;\r\n" : "")  +
                                  "/help         — покажу справочную информацию;\r\n" +
                                  "/info         — покажу свои версию и дату создания;\r\n" +
                                  "/exit         — завершу работу.\r\n" +
                "Завершайте ввод нажатием на Enter\r\n"
            );
        }
        private static void Info()
        {
            Console.WriteLine(GetStringDependsOnUserName("Версия — 0.0.3, дата создания — 13.06.2025\r\n"));
        }
        private static void Echo(string command)
        {
            Console.WriteLine(command.Replace("/echo ", "") + "\r\n");
        }
        private static void Main()
        {
            try
            {
                Console.WriteLine("Привет! Я — бот. \r\n\r\n");

                Console.WriteLine("Введите максимально допустимое количество задач...");
                taskCountLimit = ParseAndValidateInt(ReadLine(), 1, taskCountMax);

                Console.WriteLine("Введите максимально допустимую длину наименования задачи...");
                taskLengthLimit = ParseAndValidateInt(ReadLine(), 1, taskLengthMax);

                Help();

                do
                {
                    Console.WriteLine("Жду вашу команду...\r\n\r\n");
                    command = ReadLine();
                    switch (command)
                    {
                        case "/start":
                            if (!isStarted) Start();
                            Help();
                            break;
                        case "/addtask":
                            if (isStarted) AddTask();
                            break;
                        case "/showalltasks":
                            if (isStarted) ShowAllTasks("Список задач:");
                            break;
                        case "/showtasks":
                            if (isStarted) ShowTasks("Список активных задач:");
                            break;
                        case "/removetask":
                            if (isStarted) RemoveTask();
                            break;
                        case "/completetask":
                            if (isStarted) CompleteTask();
                            break;
                        case "/help":
                            Help();
                            break;
                        case "/info":
                            Info();
                            break;
                        default:
                            if (command.StartsWith("/echo ") && command.Length > 6) Echo(command);
                            else Help();
                            break;
                    }
                }
                while (command != "/exit");
            }
            catch (Exception ex) 
            when 
            (
                ex is ArgumentException || 
                ex is TaskCountLimitException || 
                ex is TaskLengthLimitException  || 
                ex is DuplicateTaskException
            )
            {
                Console.WriteLine($"{ex.Message}\r\n");
            }
            catch (Exception ex)  
            {
                Console.WriteLine
                (
                    $"Произошла непредвиденная ошибка:\r\n" +
                    $"{ex.GetType().FullName}\r\n" +
                    $"{ex.Message}\r\n" +
                    $"{ex.StackTrace}\r\n" +
                    $"{ex.InnerException}\r\n"
                );
            }
        }
    }
}

