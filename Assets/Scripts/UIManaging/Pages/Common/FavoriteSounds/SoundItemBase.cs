using Abstract;
using UnityEngine;

namespace UIManaging.Pages.Common.FavoriteSounds
{
    public abstract class SoundItemBase<TModel>: BaseContextDataView<TModel> where TModel: SoundItemModel
    {
        [SerializeField] private SoundPreviewPanelBase _soundPreviewPanel;
        [SerializeField] private SoundInfoPanel _soundInfoPanel;

        protected override void OnInitialized()
        {
            _soundPreviewPanel.Initialize(ContextData.Sound);
            _soundInfoPanel.Initialize(ContextData.Sound);
        }

        protected override void BeforeCleanup()
        {
            _soundPreviewPanel.CleanUp();
            _soundInfoPanel.CleanUp();
        }
    }
}