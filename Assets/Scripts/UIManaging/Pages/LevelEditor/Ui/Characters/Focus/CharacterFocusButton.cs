using System;
using System.Linq;
using System.Threading;
using Bridge;
using Extensions;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using Zenject;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.LevelEditor.Ui.Characters
{
    internal sealed class CharacterFocusButton : CharacterFocusButtonBase
    {
        [Inject] private IBridge _bridge;
        [Inject] private ILevelManager _levelManager;
        
        private CancellationTokenSource _cancellationTokenSource;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public CharacterInfo Character { get; set; }

        private ICharacterAsset CharacterAsset => GetCharacterAsset();
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _cancellationTokenSource?.Cancel();
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override int TargetSequenceNumber
        {
            get
            {
                var targetEvent = _levelManager.TargetEvent;

                if (targetEvent == null) return -1;
                if (Character == null) return -1;
                
                return targetEvent.GetCharacterControllerByCharacterId(Character.Id)?.ControllerSequenceNumber ?? 0;
            }
        }

        public override void FocusOnTarget()
        {
            if (LevelManager.GetTargetEventSetLocationAsset() == null || CharacterAsset == null) return;

            var lookAtBone = CharacterAsset.LookAtBoneGameObject;
            var followBone = CharacterAsset.GameObject;
            CameraSystem.SetTargets(lookAtBone, followBone, false);
        }

        public async void UpdateThumbnail(Action<CharacterFocusButton, Sprite> onThumbnailUpdated = null)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var thumbnailFile = Character.Files.First(x => x.Resolution == Resolution._128x128);
            var result = await _bridge.GetCharacterThumbnailAsync(Character.Id, thumbnailFile, cancellationToken:_cancellationTokenSource.Token);

            if (result.IsRequestCanceled)
            {
                return;
            }

            if (result.IsSuccess)
            {
                Thumbnail.sprite = (result.Object as Texture2D).ToSprite();
                onThumbnailUpdated?.Invoke(this, Thumbnail.sprite);
            }
            else
            {
                Debug.LogWarning(result.ErrorMessage);
            }
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private ICharacterAsset GetCharacterAsset()
        {
            return Character == null ? null : LevelManager.GetCurrentCharactersAssets().FirstOrDefault(x => x.Id == Character.Id);
        }
    }
}