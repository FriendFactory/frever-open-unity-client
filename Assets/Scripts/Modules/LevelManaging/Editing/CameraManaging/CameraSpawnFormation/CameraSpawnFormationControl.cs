using Extensions;
using Modules.AssetsManaging;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.CameraManaging.CameraSpawnFormation
{
    internal sealed class CameraSpawnFormationControl
    {
        private readonly ICameraSystem _cameraSystem;
        private readonly IEventEditor _eventEditor;
        private readonly IAssetManager _assetManager;
        private readonly ISpawnFormationCameraAngleProvider _angleProvider;

        public CameraSpawnFormationControl(ICameraSystem cameraSystem, IEventEditor eventEditor, IAssetManager assetManager, ISpawnFormationCameraAngleProvider angleProvider)
        {
            _cameraSystem = cameraSystem;
            _eventEditor = eventEditor;
            _assetManager = assetManager;
            _angleProvider = angleProvider;
        }

        public void PutCameraOnDefaultPosition(long spawnFormationId, int focusedCharacterSeqNumber)
        {
            var angle = GetAngle(spawnFormationId, focusedCharacterSeqNumber);
            SetCameraRelativeCharacter(angle);
        }

        private float GetAngle(long spawnFormationId, int characterSequenceNumber)
        {
            return _angleProvider.GetAngle(spawnFormationId, characterSequenceNumber);
        }
        
        private void SetCameraRelativeCharacter(float angle)
        {
            var focusedCharacterId = _eventEditor.TargetEvent.GetTargetCharacterController().CharacterId;
            var character = _assetManager.GetActiveAssetOfType<ICharacterAsset>(focusedCharacterId);
          
            var lookToFaceXAxisValue = _cameraSystem.GetXAxisValueToFocusOnPoint(character.GameObject.transform);
            
            _cameraSystem.Set(CameraAnimationProperty.AxisX, lookToFaceXAxisValue + angle);
        }
    }
}