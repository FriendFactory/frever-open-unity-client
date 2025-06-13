using Bridge.Models.ClientServer.Assets;
using Extensions;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection
{
    internal sealed class SetLocationSubAssetSelectionImageItemView: SubAssetSelectionImageItemView
    {
        [SerializeField] private Image _movementTypeImage;
        [Inject] private IMovementTypeThumbnailsProvider _movementTypeThumbnailsProvider;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            SetupMovementTypeIcon();
        }

        private void SetupMovementTypeIcon()
        {
            var spawnPosition = ContextData.RepresentedObject as CharacterSpawnPositionInfo;
            var movementType = spawnPosition.MovementTypeId;
            if (!movementType.HasValue)
            {
                HideMovementTypeImage();
                return;
            }
            var icon = _movementTypeThumbnailsProvider.GetThumbnail(movementType.Value);
            if (icon == null)
            {
                HideMovementTypeImage();
                return;
            }
            _movementTypeImage.sprite = icon;
            _movementTypeImage.preserveAspect = true;
            _movementTypeImage.SetActive(true);
        }

        private void HideMovementTypeImage()
        {
            _movementTypeImage.SetActive(false);
        }
    }
}