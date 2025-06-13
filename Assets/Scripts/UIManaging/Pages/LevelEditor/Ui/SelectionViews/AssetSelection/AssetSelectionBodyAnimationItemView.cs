namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection
{
    internal sealed class AssetSelectionBodyAnimationItemView : AssetSelectionAnimatedGifItemView
    {
        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            LevelManager.CharactersOutfitsUpdated += OnCharactersOutfitsUpdated;
            _button.interactable = !LevelManager.IsChangingOutfit;
        }
    }
}
