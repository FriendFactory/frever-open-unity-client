using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.CharacterManagement;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.Templates;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class VideoMessageToLevelCreationScenarioTransition: TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly ILevelManager _levelManager;
        private readonly ITemplateProvider _templateProvider;
        private readonly IMetadataProvider _metadataProvider;
        private readonly CharacterManager _characterManager;
        
        public override ScenarioState To => ScenarioState.ExitToLevelCreation;

        public VideoMessageToLevelCreationScenarioTransition(ILevelManager levelManager, ITemplateProvider templateProvider, IMetadataProvider metadataProvider, CharacterManager characterManager)
        {
            _levelManager = levelManager;
            _templateProvider = templateProvider;
            _metadataProvider = metadataProvider;
            _characterManager = characterManager;
        }

        protected override Task UpdateContext()
        {
            return Task.CompletedTask;
        }

        protected override async Task OnRunning()
        {
            _levelManager.StopCurrentPlayMode();
            var levelCreationTemplate = await _templateProvider.GetTemplateEvent(_metadataProvider.DefaultTemplateId);
            
            var assetToKeep = new List<IAsset>();
            
            var mainCharacter = _characterManager.SelectedCharacter;
            var characterToKeep = _levelManager.GetCurrentCharactersAssets().FirstOrDefault(x=>x.Id == mainCharacter.Id);
            if (characterToKeep != null)
            {
                assetToKeep.Add(characterToKeep);
            }
            
            var setLocationToKeepId = levelCreationTemplate.GetSetLocationId();
            var setLocationAsset = _levelManager.GetTargetEventSetLocationAsset();
            if (setLocationAsset.Id == setLocationToKeepId)
            {
                assetToKeep.Add(setLocationAsset);
            }
            _levelManager.UnloadAllAssets(assetToKeep.ToArray());
            await base.OnRunning();
        }
    }
}