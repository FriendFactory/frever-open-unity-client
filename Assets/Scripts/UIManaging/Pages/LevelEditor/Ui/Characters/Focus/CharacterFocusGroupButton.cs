using Cinemachine;
using Extensions;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Editing.LevelManagement;

namespace UIManaging.Pages.LevelEditor.Ui.Characters
{
    internal sealed class CharacterFocusGroupButton : CharacterFocusButtonBase
    {
        private CameraZoomOutHelper _zoomOutHelper;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override int TargetSequenceNumber => -1;

        private CinemachineTargetGroup LookAtTargetGroup => CameraSystem.GetCinemachineLookAtTargetGroup();
        private CinemachineTargetGroup FollowTargetGroup => CameraSystem.GetCinemachineFollowTargetGroup();

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void Initialize(ILevelManager levelManager, ICameraSystem cameraSystem)
        {
            _zoomOutHelper = new CameraZoomOutHelper(cameraSystem);
            base.Initialize(levelManager, cameraSystem);
        }

        public override void FocusOnTarget()
        {
            CameraSystem?.SetGroupTargets(false);
        }

        public void UpdateTargetGroups()
        {
            LevelManager.RefreshCameraGroupFocus();
        }

        public void UpdateGroupTargetRotation()
        {
            var setLocation = LevelManager.GetTargetEventSetLocationAsset();
            if(setLocation == null) return;

            var currentSpawnPosition = LevelManager.TargetEvent.CurrentCharacterSpawnPosition();
            var spawnPositionTransform = setLocation.GetCharacterSpawnPositionTransform(currentSpawnPosition.UnityGuid);
            var rotation = spawnPositionTransform.rotation;
            LookAtTargetGroup.transform.rotation = rotation;
            FollowTargetGroup.transform.rotation = rotation;
        }

        public override void SetSelected(bool isSelected)
        {
            base.SetSelected(isSelected);
            
            if (isSelected)
            {
                _zoomOutHelper?.ZoomOut();
            }
        }
    }
}