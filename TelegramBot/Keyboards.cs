using bot.Core.Entities;
using bot.TelegramBot.DTO;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace bot.TelegramBot
{
    internal class Keyboards
    {
        public static ReplyKeyboardMarkup DefaultKeyboard = new (new KeyboardButton[] { "/addtask", "/show", "/report" })
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

        //readonly static ToDoListCallbackDto toDoListCallbackDto = new();

        public static InlineKeyboardMarkup ShowKeyboard(IReadOnlyList<ToDoList> toDoList)
        {
            InlineKeyboardMarkup showKeyboard = new();
            showKeyboard.AddButton(InlineKeyboardButton.WithCallbackData("📌Без списка", "ToDoListCallbackDto(\"show\").ToString()"));
            foreach (ToDoList item in toDoList)
            {
                showKeyboard.AddNewRow
                ([
                    InlineKeyboardButton.WithCallbackData($"{item.Name}", $"ToDoListCallbackDto(\"show|{item.Id}\").ToString()"),
                ]);
            }

            showKeyboard.AddNewRow
            ([
                    InlineKeyboardButton.WithCallbackData("🆕Добавить", "ToDoListCallbackDto(\"addlist\").ToString()"),
                    InlineKeyboardButton.WithCallbackData("❌Удалить", "ToDoListCallbackDto(\"deletelist\").ToString()")
            ]);
            return showKeyboard;
        }
    }




}
