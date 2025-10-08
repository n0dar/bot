using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace bot.TelegramBot
{
    internal static class Keyboards
    {
        public static ReplyKeyboardMarkup DefaultKeyboard = new (new KeyboardButton[] { "/addtask", "/showalltasks", "/showtasks", "/report" })
        {
            ResizeKeyboard = true
        };

        public static ReplyKeyboardMarkup CancelKeyboard = new(new KeyboardButton("/cancel"))
        {
            ResizeKeyboard = true
        };

        public static ReplyKeyboardMarkup StartKeyboard = new(new KeyboardButton("/start"))
        {
            ResizeKeyboard = true
        };
    }
}
