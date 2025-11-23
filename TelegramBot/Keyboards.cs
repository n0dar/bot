using bot.Core.Entities;
using bot.TelegramBot.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public static InlineKeyboardMarkup ToDoListKeyboard(IReadOnlyList<ToDoList> toDoList, string action)
        {
            InlineKeyboardMarkup inlineKeyboardMarkup = new();

            foreach (ToDoList item in toDoList)
            {
                inlineKeyboardMarkup.AddNewRow
                ([
                    InlineKeyboardButton.WithCallbackData($"{item.Name}", (new CallbackDto() { Action = action, Id=item.Id}).ToString())
                ]);
            }
            return inlineKeyboardMarkup;
        }
        public static InlineKeyboardMarkup ShowToDoListKeyboard(IReadOnlyList<ToDoList> toDoList)
        {
            InlineKeyboardMarkup showToDoListKeyboard = new();

            showToDoListKeyboard.AddButton(InlineKeyboardButton.WithCallbackData("📌Без списка", (new CallbackDto() { Action = "show"}).ToString()));

            InlineKeyboardMarkup toDoListKeyboard = ToDoListKeyboard(toDoList, "show");
            foreach (var buttonRow in toDoListKeyboard.InlineKeyboard)
            {
                showToDoListKeyboard.AddNewRow([.. buttonRow]);
            }

            showToDoListKeyboard.AddNewRow
            ([
                InlineKeyboardButton.WithCallbackData("🆕Добавить", (new CallbackDto() { Action = "addlist" }).ToString()),
                InlineKeyboardButton.WithCallbackData("❌Удалить", (new CallbackDto() { Action = "deletelist" }).ToString())
            ]);
            return showToDoListKeyboard;
        }
        public static InlineKeyboardMarkup YesNoKeyboard(string action, Guid? Id)
        {
            InlineKeyboardMarkup showKeyboard = new();
            showKeyboard.AddNewRow
            ([
                InlineKeyboardButton.WithCallbackData("✅Да", (new CallbackDto() {Action = String.Concat(action,"yes"), Id = Id}).ToString()),
                InlineKeyboardButton.WithCallbackData("❌Нет", (new CallbackDto() {Action = String.Concat(action,"no"), Id = Id}).ToString())
            ]);
            return showKeyboard;
        }
        public static InlineKeyboardMarkup ShowToDoItemsKeyboard(IReadOnlyList<ToDoItem> toDoItems)
        {
            InlineKeyboardMarkup showToDoItemsKeyboard = new();

            foreach (ToDoItem item in toDoItems)
            {
                showToDoItemsKeyboard.AddNewRow
                ([
                    InlineKeyboardButton.WithCallbackData($"{item.Name}", (new CallbackDto() { Action = "showtask", Id=item.Id}).ToString())
                ]);
            }
            return showToDoItemsKeyboard;
        }
        public static InlineKeyboardMarkup CompleteDeleteTaskKeyboard(Guid id)
        {
            InlineKeyboardMarkup showKeyboard = new();
            showKeyboard.AddNewRow
            ([
                InlineKeyboardButton.WithCallbackData("✅Выполнить", (new CallbackDto() { Action = "completetask", Id = id }).ToString()),
                InlineKeyboardButton.WithCallbackData("❌Удалить", (new CallbackDto() { Action = "deletetask", Id = id }).ToString()),
            ]);
            return showKeyboard;
        }
    }
}
