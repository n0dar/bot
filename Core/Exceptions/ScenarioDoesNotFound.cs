using bot.TelegramBot.Scenarios;
using System;

namespace bot.Core.Exceptions
{
    public class ScenarioDoesNotFound (ScenarioType scenario) : Exception($"Сценарий {scenario} не найден!");
}
