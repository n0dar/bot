#nullable enable
using System;
using System.Collections.Generic;

namespace bot.TelegramBot.Scenarios
{
    public enum ScenarioType
    {
        None,
        AddTask,
        AddList,
        DeleteList,
        ProcessTask
    }
    public class ScenarioContext(ScenarioType scenario)
    {
        //Id пользователя в Telegram
        long UserId { get; set; }
        public ScenarioType CurrentScenario { get; set; } = scenario;
        //Текущий шаг сценария
        public string? CurrentStep {  get; set; }
        //Дополнительная инфрмация, необходимая для работы сценария
        public Dictionary<string, object> Data {  get; set; } = [];
        public DateTime CreatedAt { get; private set; } = DateTime.Now;
    }
}
