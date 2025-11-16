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
        public static InlineKeyboardMarkup ShowToDoListKeyboard(IReadOnlyList<ToDoList> toDoList)
        {
            InlineKeyboardMarkup showKeyboard = new();

            showKeyboard.AddButton(InlineKeyboardButton.WithCallbackData("📌Без списка", (new ToDoListCallbackDto() { Action = "show"}).ToString()));
            foreach (ToDoList item in toDoList)
            {
                showKeyboard.AddNewRow
                ([
                    InlineKeyboardButton.WithCallbackData($"{item.Name}", (new ToDoListCallbackDto() { Action = "show", ToDoListId=item.Id}).ToString())
                ]);
            }

            showKeyboard.AddNewRow
            ([
                InlineKeyboardButton.WithCallbackData("🆕Добавить", (new ToDoListCallbackDto() { Action = "addlist" }).ToString()),
                InlineKeyboardButton.WithCallbackData("❌Удалить", (new ToDoListCallbackDto() { Action = "deletelist" }).ToString())
            ]);
            return showKeyboard;
        }
        public static InlineKeyboardMarkup DeleteToDoListKeyboard(IReadOnlyList<ToDoList> toDoList)
        {
            InlineKeyboardMarkup deleteKeyboard = new();

            foreach (ToDoList item in toDoList)
            {
                deleteKeyboard.AddNewRow
                ([
                    InlineKeyboardButton.WithCallbackData($"{item.Name}", (new ToDoListCallbackDto() { Action = "deletelistbyid", ToDoListId=item.Id}).ToString())
                ]);
            }
            return deleteKeyboard;
        }
        public static InlineKeyboardMarkup AddTaskToDoListKeyboard(IReadOnlyList<ToDoList> toDoList)
        {
            InlineKeyboardMarkup addTaskKeyboard = new();

            foreach (ToDoList item in toDoList)
            {
                addTaskKeyboard.AddNewRow
                ([
                    InlineKeyboardButton.WithCallbackData($"{item.Name}", (new ToDoListCallbackDto() { Action = "addtask", ToDoListId=item.Id}).ToString())
                ]);
            }
            return addTaskKeyboard;
        }

        public static InlineKeyboardMarkup YesNoKeyboard()
        {
            InlineKeyboardMarkup showKeyboard = new();
            showKeyboard.AddNewRow
            ([
                InlineKeyboardButton.WithCallbackData("✅Да", (new ToDoListCallbackDto() { Action = "yes"}).ToString()),
                InlineKeyboardButton.WithCallbackData("❌Нет", (new ToDoListCallbackDto() { Action = "no"}).ToString())
            ]);
            return showKeyboard;
        }
    }
}
