#nullable enable
using System;
using System.Collections.Generic;

namespace bot
{
    class Program
    {
        private const int           taskCountMax = 100;
        private const int           taskLengthMax = 100;

        private static bool         isStarted = false;
        private static string       userName="";
        private static string       command="";
        private static List<string> taskList = [];
        private static int          taskCountLimit;
        private static int          taskLengthLimit;

        private class TaskCountLimitException : Exception 
        {
            public TaskCountLimitException(int taskCountLimit) : base($"Превышено максимальное количество задач равное {taskCountLimit}\r\n") {}
        }
        private class TaskLengthLimitException : Exception
        {
            public TaskLengthLimitException(int taskLength, int taskLengthLimit) : base($"Длина задачи {taskLength} превышает максимально допустимое значение {taskLengthLimit}") {}
        }
        private class DuplicateTaskException : Exception
        {
            public DuplicateTaskException(string task) : base($"Задача \"{task}\" уже существует") {}
        }
        private static int ParseAndValidateInt(string? str, int min, int max)
        {
            int.TryParse(str, out int res);
            if (res < min || res > max) throw new ArgumentException($"Значение должно находиться в интервале [{min};{max}]");
            return res;
        }
        private static void ValidateString(string? str)
        {
            if ((str ?? "").Trim()=="") throw new ArgumentException("Значение не может быть пустым");
        }
        private static string ReadLine() 
        {
            //return Console.ReadLine() ?? "";
            string? res = Console.ReadLine();
            ValidateString(res);
            return res;
        }
        private static string GetStringDependsOnUserName(string str)
        {
            return 
                (userName != "" ? userName + ", " : "") + 
                (userName != "" ? char.ToLower(str[0]) : char.ToUpper(str[0])) + str[1..];
        }
        private static void Start()
        {
            do
            {
                Console.WriteLine("Введите ваше имя...\r\n");
                userName = ReadLine();
            }
            while (userName.Trim() == "");
            if (userName == "/exit") command = "/exit";
            else Console.Clear();
            isStarted = true;
        }
        private static void AddTask()
        {
            if (taskList.Count == taskCountLimit) throw new TaskCountLimitException(taskCountLimit);
            
            string taskName;
            do
            {
                Console.WriteLine(GetStringDependsOnUserName("Введите описание задачи...\r\n"));
                taskName = ReadLine().Trim();
            }
            while (taskName == "");

            if (taskName.Length > taskLengthLimit) throw new TaskLengthLimitException(taskName.Length, taskLengthLimit);
            if (taskList.Contains(taskName)) throw new DuplicateTaskException(taskName);

            taskList.Add(taskName);
            Console.WriteLine("Задача добавлена.\r\n");
        }
        private static void ShowTasks(string msg)
        {
            if (taskList.Count > 0)
            {
                uint i = 0;
                Console.WriteLine(GetStringDependsOnUserName(msg));
                foreach (string task in taskList)
                {
                    Console.WriteLine($"{++i} {task}");
                }
            }
            else
            {
                Console.WriteLine("Список задач пуст.");
            }
            Console.WriteLine("\r\n");
        }
        private static void RemoveTask()
        {
            if (taskList.Count > 0)
            {
                ShowTasks("Укажите номер задачи, которую хотите удалить:");
                if (int.TryParse(ReadLine(), out int taskNumber))
                {
                    if (taskNumber != 0 && taskNumber <= taskList.Count)
                    {
                        taskList.RemoveAt(--taskNumber);
                        Console.WriteLine("Задача удалена.\r\n");
                    }
                    else Console.WriteLine("Задача с таким номером не существует.\r\n");
                }
                else Console.WriteLine("Некорректный номер задачи.\r\n");
            }
            else Console.WriteLine("Список задач пуст.\r\n");
        }
        private static void Help()
        {
            Console.WriteLine
            (
                GetStringDependsOnUserName("Для взаимодействия со мной вам доступен следующий список команд:\r\n\r\n") +
                (isStarted ? "" : "/start      — начните работу с этой команды" + ";\r\n")  +
                (isStarted ?      "/addtask    — добавлю задачу в список" + ";\r\n" : "") +
                (isStarted ?      "/showtasks  — покажу список задач" + ";\r\n" : "") +
                (isStarted ?      "/removetask — удалю задачу из списка" + ";\r\n" : "") +
                (isStarted ?      "/echo       — отображу введенную вами строку текста, указанную после команды через пробел" + ";\r\n" : "")  +
                                  "/help       — покажу справочную информацию;\r\n" +
                                  "/info       — покажу свои версию и дату создания;\r\n" +
                                  "/exit       — завершу работу.\r\n" +
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
                        case "/showtasks":
                            if (isStarted) ShowTasks("Задачи в списке:");
                            break;
                        case "/removetask":
                            if (isStarted) RemoveTask();
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

