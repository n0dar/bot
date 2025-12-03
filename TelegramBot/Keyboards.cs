using bot.Core.Entities;
using bot.Helpers;
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
                    InlineKeyboardButton.WithCallbackData($"{item.Name}", (new ToDoItemCallbackDto() { Action = action, ToDoItemId = item.Id}).ToString())
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
                InlineKeyboardButton.WithCallbackData("✅Да", (new ToDoItemCallbackDto() {Action = String.Concat(action,"yes"), ToDoItemId = Id}).ToString()),
                InlineKeyboardButton.WithCallbackData("❌Нет", (new ToDoItemCallbackDto() {Action = String.Concat(action,"no"), ToDoItemId = Id}).ToString())
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
                    InlineKeyboardButton.WithCallbackData($"{item.Name}", (new ToDoItemCallbackDto() { Action = "showtask", ToDoItemId = item.Id}).ToString())
                ]);
            }
            return showToDoItemsKeyboard;
        }
        public static InlineKeyboardMarkup CompleteDeleteTaskKeyboard(Guid id)
        {
            InlineKeyboardMarkup showKeyboard = new();
            showKeyboard.AddNewRow
            ([
                InlineKeyboardButton.WithCallbackData("✅Выполнить", (new ToDoItemCallbackDto() { Action = "completetask",ToDoItemId = id }).ToString()),
                InlineKeyboardButton.WithCallbackData("❌Удалить", (new ToDoItemCallbackDto() { Action = "deletetask", ToDoItemId = id }).ToString()),
            ]);
            return showKeyboard;
        }
        public static InlineKeyboardMarkup PagedButtonsKeyboard(IReadOnlyList<ToDoItem> toDoItems, PagedListCallbackDto listDto)
        {

            int _pageSize = 5;
            InlineKeyboardMarkup pagedButtonsKeyboard = new();

            IEnumerable<int> batch = EnumerableExtension.GetBatchByNumber(_pageSize, listDto.Page);

            foreach (var index in batch.Where(i => i < toDoItems.Count))
            {
                pagedButtonsKeyboard.AddNewRow
                ([
                    InlineKeyboardButton.WithCallbackData(toDoItems[index].Name,   $"showtask|{toDoItems[index].Id}")
                ]);
            }

            List<InlineKeyboardButton> navigationKeybordButtons = new();

            if (listDto.Page > 0)
            {
                navigationKeybordButtons.Add(InlineKeyboardButton.WithCallbackData("⬅️", (new PagedListCallbackDto { Action = listDto.Action, ToDoListId = listDto.ToDoListId, Page = listDto.Page - 1 }).ToString()));
            }

            int totalPages = (int)Math.Ceiling((double)toDoItems.Count / _pageSize);
            if (listDto.Page < totalPages - 1)
            {
                navigationKeybordButtons.Add(InlineKeyboardButton.WithCallbackData("➡️", (new PagedListCallbackDto { Action = listDto.Action, ToDoListId = listDto.ToDoListId, Page = listDto.Page + 1 }).ToString()));
            }

            if (navigationKeybordButtons.Count > 0) pagedButtonsKeyboard.AddNewRow(navigationKeybordButtons.ToArray());

            return pagedButtonsKeyboard;
        }
    }
}


