using System;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Common.BridgeAdapter;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.CharacterManagement;
using Zenject;

namespace Modules.LevelManaging.Editing.LevelManagement
{
    public interface ILevelForVideoMessageProvider
    {
        Task<Level> GetLevelForVideoMessage(Race race);
    }
    
    [UsedImplicitly]
    internal sealed class LevelForVideoMessageProvider: ILevelForVideoMessageProvider
    {
        private readonly ILevelService _levelService;
        private readonly ILevelHelper _levelHelper;
        private readonly CharacterManager _characterManager;

        public LevelForVideoMessageProvider(ILevelService levelService, ILevelHelper levelHelper, CharacterManager characterManager)
        {
            _levelService = levelService;
            _levelHelper = levelHelper;
            _characterManager = characterManager;
        }

        public async Task<Level> GetLevelForVideoMessage(Race race)
        {
            var levelResult = await _levelService.GetLevelForVideoMessageAsync();
            if (!levelResult.IsSuccess)
            {
                throw new InvalidOperationException("Failed to get video message template from server");
            }
            var level = levelResult.Level;
            await _levelHelper.PrepareLevelForVideoMessage(level);
            var mainCharacter = await _characterManager.GetCharacterAsync(_characterManager.RaceMainCharacters[race.Id]);
            foreach (var cc in level.Event.SelectMany(x=>x.CharacterController))
            {
                cc.SetCharacter(mainCharacter);
                var faceVoiceController = cc.GetCharacterControllerFaceVoiceController();
                faceVoiceController.SetFaceAnimation(null);
                faceVoiceController.SetVoiceTrack(null);
            }
            return level;
        }
    }
}