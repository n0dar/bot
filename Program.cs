using System;
using System.Collections.Generic;

namespace bot
{
    class Program
    {
        private static bool isStarted = false;
        private static string userName="";
        private static string command;
        private static List<string> taskList = [];
        static string ReadLine()
        {
            return Console.ReadLine() ?? "";
        }
        static string GetStringDependsOnUserName(string str)
        {
            return 
                (userName != "" ? userName + ", " : "") + 
                (userName != "" ? char.ToLower(str[0]) : char.ToUpper(str[0])) + str[1..];
        }
        static void Start()
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
        static void AddTask()
        {
            string taskName;
            do
            {
                Console.WriteLine(GetStringDependsOnUserName("Введите описание задачи...\r\n"));
                taskName = ReadLine();
            }
            while (taskName.Trim() == "");
            taskList.Add(taskName);
            Console.WriteLine("Задача добавлена.\r\n");
        }
        static void ShowTasks(string msg)
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
        static void RemoveTask()
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
        static void Help()
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
        static void Info()
        {
            Console.WriteLine(GetStringDependsOnUserName("Версия — 0.0.1, дата создания — 09.06.2025\r\n"));
        }
        static void Echo(string command)
        {
            Console.WriteLine(command.Replace("/echo ", "") + "\r\n");
        }
        static void Main()
        {
            Console.WriteLine("Привет! Я — бот. \r\n\r\n");
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
                        if (command.StartsWith("/echo ") && command.Length>6) Echo(command);
                        else Help();
                        break;
                }
            }
            while (command != "/exit");
        }
    }
}

