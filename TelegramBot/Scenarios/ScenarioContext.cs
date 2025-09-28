#nullable enable
using System.Collections.Generic;

namespace bot.TelegramBot.Scenarios
{
    public enum ScenarioType
    {
        None,
        AddTask
    }
    public class ScenarioContext(ScenarioType scenario)
    {
        //Id пользователя в Telegram
        long UserId { get; set; }
        ScenarioType CurrentScenario { get; set; } = scenario;
        //Текущий шаг сценария
        string? CurrentStep {  get; set; }
        //Дополнительная инфрмация, необходимая для работы сценария
        Dictionary<string, object> Data {  get; set; }
    }
}
