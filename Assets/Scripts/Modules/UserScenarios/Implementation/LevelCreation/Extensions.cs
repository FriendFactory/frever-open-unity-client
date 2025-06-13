using System.Collections.Generic;
using Bridge.Models.ClientServer.Tasks;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation
{
    internal static class Extensions
    {
        private static readonly IReadOnlyDictionary<Page, ScenarioState> PAGE_TO_STATE_MAP = new Dictionary<Page, ScenarioState>
        {
            { Page.CharacterEditor, ScenarioState.CharacterEditor },
            { Page.LevelEditor, ScenarioState.LevelEditor },
            { Page.PostRecordEditor, ScenarioState.PostRecordEditor }
        };
        
        public static ScenarioState ToScenarioState(this Page page)
        {
            return PAGE_TO_STATE_MAP[page];
        }
    }
}