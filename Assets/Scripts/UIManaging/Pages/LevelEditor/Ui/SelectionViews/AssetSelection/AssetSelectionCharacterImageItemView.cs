using DG.Tweening;
using Extensions;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection
{
    public class AssetSelectionCharacterImageItemView : AssetSelectionAnimatedImageItemView
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private AspectRatioFitter _aspectRatioFitter;
        
        [Inject] private SnackBarHelper _snackBarHelper;
        
        private AssetSelectionCharacterModel _characterModel;
        
        protected override string TitleDisplayText => GetTitleDisplayText();
        
        protected override void OnInitialized()
        {
            _characterModel = ContextData as AssetSelectionCharacterModel;
            base.OnInitialized();
        }
        
        private string GetTitleDisplayText()
        {
            return _characterModel.DisplayName;
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
            _aspectRatioFitter.aspectRatio = texture.width / (float) texture.height;
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
