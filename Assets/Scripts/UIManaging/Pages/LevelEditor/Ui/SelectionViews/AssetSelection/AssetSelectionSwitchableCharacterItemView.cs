using System.Linq;
using Extensions;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection
{
    public class AssetSelectionSwitchableCharacterItemView : AssetSelectionAnimatedImageItemView
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private AspectRatioFitter aspectRatioFitter;

        [Inject] private SnackBarHelper _snackBarHelper;
        
        private AssetSelectionCharacterModel _characterModel;
        
        protected override void OnInitialized()
        {
            _characterModel = ContextData as AssetSelectionCharacterModel;
            base.OnInitialized();
            Refresh();
        }

        protected override void OnStopUpdatingAsset(DbModelType type, long id)
        {
            base.OnStopUpdatingAsset(type, id);
            
            if(type == DbModelType.Character)
            {
                Refresh();
            }
        }

        protected override void RefreshSelectionGameObjects()
        {
            base.RefreshSelectionGameObjects();
            var colors = Button.colors;
            colors.disabledColor = ContextData.IsSelected
               ? Button.colors.disabledColor.SetAlpha(1f)
               : Button.colors.disabledColor.SetAlpha(100f/255f);
            Button.colors = colors;
        }

        private void Refresh()
        {
            if (ContextData == null)
            {
                Button.interactable = false;
                return;
            }
            
            var currentCharacterId = ContextData.ItemId;
            var characterControllers = LevelManager.TargetEvent.CharacterController;
            var isButtonInteractable = characterControllers.FirstOrDefault(cc => cc.Character.Id == currentCharacterId) == null;
            Button.interactable = isButtonInteractable && !LevelManager.IsReplacingCharacter;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Refresh();
        }

        protected override void OnThumbnailLoaded(long id, object downloadedTexture)
        {
            var texture = (Texture2D) downloadedTexture;
            if (texture == null) return;

            if (IsDestroyed || !gameObject.activeInHierarchy || ContextData.ThumbnailOwner.Id != id)
            {
                return;
            }
            
            base.OnThumbnailLoaded(id, downloadedTexture);

            canvasGroup.alpha = _characterModel.HasAccess ? 1 : 0.5f;
            aspectRatioFitter.aspectRatio = texture.width / (float)texture.height;
        }

        protected override void OnClicked()
        {
            if (LevelManager.IsLoadingAssetsOfType(GetCurrentItemType()))
            {
                return;
            }

            if (_characterModel.HasAccess)
            {
                base.OnClicked();
            }
            else
            {
                _snackBarHelper.ShowInformationSnackBar($"User {_characterModel.DisplayName} doesn't allow anyone to use their character");
            }
        }
    }
}
