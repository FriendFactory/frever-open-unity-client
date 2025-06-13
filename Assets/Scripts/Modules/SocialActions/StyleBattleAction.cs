using Bridge.Models.ClientServer.Tasks;
using Modules.UniverseManaging;
using Modules.UserScenarios.Common;
using Navigation.Args.Feed;
using Navigation.Core;
using UIManaging.Pages.Common.VideoManagement;

namespace Modules.SocialActions
{
    internal sealed class StyleBattleAction : ISocialAction
    {
        private readonly long _actionId;
        private readonly VideoManager _videoManager;
        private readonly PageManager _pageManager;
        private readonly TaskFullInfo _taskFullInfo;
        private readonly IScenarioManager _scenarioManager;
        private readonly IUniverseManager _universeManager;

        public StyleBattleAction(long actionId, TaskFullInfo taskFullInfo, IScenarioManager scenarioManager, PageManager pageManager, VideoManager videoManager, IUniverseManager universeManager)
        {
            _actionId = actionId;
            _taskFullInfo = taskFullInfo;
            
            _scenarioManager = scenarioManager;
            _pageManager = pageManager;
            _videoManager = videoManager;
            _universeManager = universeManager;
        }
        
        public void Execute()
        {
            if (_taskFullInfo.TaskType == TaskType.Voting)
            {
                var args = new TaskFeedArgs(_videoManager, _taskFullInfo.Id);
                _pageManager.MoveNext(args);
            }
            else
            {
                _universeManager.SelectUniverse(universe =>  _scenarioManager.ExecuteTask(universe, _taskFullInfo));
            }
        }
    }
}