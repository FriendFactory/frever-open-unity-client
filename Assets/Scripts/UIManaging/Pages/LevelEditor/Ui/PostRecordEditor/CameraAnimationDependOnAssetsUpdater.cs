using Extensions;
using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using PlayMode = Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    /// <summary>
    ///     Updates all assets which impacts final camera position/rotation(only body animation for now)
    ///     todo: we could reuse logic from LevelManager.Simulate if we have recorded user path. For a while we should use directly Update on Animators
    /// </summary>
    internal sealed class CameraAnimationDependOnAssetsUpdater
    {
        private static readonly DbModelType[] DependedAssetTypes = {DbModelType.BodyAnimation};

        private readonly BodyAnimationsControl _bodyAnimationsControl;
        private readonly ILevelPlayer _levelPlayer;

        public CameraAnimationDependOnAssetsUpdater(ILevelPlayer levelPlayer)
        {
            _bodyAnimationsControl = new BodyAnimationsControl();
            _levelPlayer = levelPlayer;
        }

        public void Prepare(Animator[] animators)
        {
            PutAssetsOnStartPosition();
            
            _bodyAnimationsControl.SetTargets(animators);
            _bodyAnimationsControl.PrepareForForcingUpdate();
        }

        public void Update(float deltaTime)
        {
            _bodyAnimationsControl.Update(deltaTime);
        }

        public void Complete()
        {
            _bodyAnimationsControl.CompleteForcingUpdate();
        }

        private void PutAssetsOnStartPosition()
        {
            _levelPlayer.Simulate(0, PlayMode.Preview, DependedAssetTypes);
        }
    }
}