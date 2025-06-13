using Extensions;
using Models;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.AssetChangers;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers
{
    internal sealed class CharacterPlayer: AssetPlayerBase<ICharacterAsset>
    {
        private readonly ILayerManager _layerManager;

        public CharacterPlayer(ILayerManager layerManager)
        {
            _layerManager = layerManager;
        }

        public void PrepareCharacter(Event ev)
        {
            var characterController = ev.GetCharacterControllerByCharacterId(Target.Id);
            var sequenceNumber = characterController.ControllerSequenceNumber;
            var layer = _layerManager.GetCharacterLayer(sequenceNumber);
            Target.SetLayer(layer);
        }
        
        public override void Simulate(float time)
        {
            Target.SetActive(true);
        }

        protected override void OnPlay()
        {
            Target.SetActive(true);
        }

        protected override void OnPause()
        {
        }

        protected override void OnResume()
        {
        }

        protected override void OnStop()
        {
        }
    }
}