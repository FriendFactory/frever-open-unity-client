using Common.Abstract;
using UnityEngine;
using UIManaging.Pages.Common.FavoriteSounds;

namespace UIManaging.Pages.FavoriteSounds
{
    public class UsedSoundPanel: BaseContextPanel<UsedSoundItemModel>
    {
        [SerializeField] private SoundPreviewToggleBasedPanel _previewPanel;
        [SerializeField] private UsedSoundInfoPanel _usedSoundInfoPanel;
        [SerializeField] private FavoriteSoundToggle _favoriteSoundToggle;
        
        protected override void OnInitialized()
        {
            _previewPanel.Initialize(ContextData.Sound);
            _usedSoundInfoPanel.Initialize(ContextData);
            _favoriteSoundToggle.Initialize(ContextData);
        }

        protected override void BeforeCleanUp()
        {
            _previewPanel.CleanUp();
            _usedSoundInfoPanel.CleanUp();
            _favoriteSoundToggle.CleanUp();
        }
    }
}