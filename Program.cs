using System;

namespace bot
{
    class Program
    {
        static void DoStart(out string userName)
        {
            do
            {
                Console.WriteLine("Введите ваше имя...\r\n");
                userName = Console.ReadLine();
            }
            while (userName.Trim() == "");
        }
        static void DoHelp(bool fStarted, string userName="")
        {
            Console.WriteLine
            (
                (userName!="" ? userName + ", " : "") +
                (userName!="" ? "д" : "Д") + "ля взаимодействия со мной вам доступен следующий список команд:\r\n\r\n" +
                "/" + (fStarted ? "echo  — отображу введенную вами строку текста, указанную после команды через пробел" : "start — начните работу с этой команды") + ";\r\n" +
                "/help  — покажу справочную информацию;\r\n" +
                "/info  — покажу свои версию и дату создания;\r\n" +
                "/exit  — завершу работу.\r\n" +
                "Завершайте ввод нажатием на Enter\r\n"
            );
        }
        static void DoInfo(string userName)
        {
            Console.WriteLine
            (
                (userName != "" ? userName + ", " : "") +
                (userName != "" ? "в" : "В") + "ерсия — 0.0.0, дата создания — 03.06.2025\r\n"
            );
        }
        static void DoEcho(string command)
        {
            Console.WriteLine(command.Replace("/echo ", "") + "\r\n");
        }
        static void Main()
        {
            bool fStarted = false;
            string command;
            string userName = "";
            
            Console.WriteLine("Привет! Я — бот. \r\n\r\n");
            DoHelp(fStarted);

            do
            {
                Console.WriteLine("Жду вашу команду...\r\n\r\n");
                command = Console.ReadLine();
                switch (command)
                {
                    case "/start":
                        if (!fStarted)
                        {
                            DoStart(out userName);
                            fStarted = true;
                        }
                        else
                        { 
                            DoHelp(fStarted, userName);
                        }
                        break;
                    case "/help":
                        DoHelp(fStarted, userName);
                        break;
                    case "/info":
                        DoInfo(userName);
                        break;
                    default:
                        if (command.StartsWith("/echo ") && command.Length>6)
                        {
                            DoEcho(command);
                        }
                        else
                        {
                            DoHelp(fStarted, userName);
                        }
                        break;
                }
            }
            while (command != "/exit");
        }
    }
}

