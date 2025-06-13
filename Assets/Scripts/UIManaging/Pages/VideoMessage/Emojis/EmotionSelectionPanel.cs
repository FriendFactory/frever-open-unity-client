using System;
using System.Linq;
using Bridge.Models.ClientServer;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.VideoMessage.Emojis
{
    internal sealed class EmotionSelectionPanel: MonoBehaviour
    {
        [SerializeField] private EmotionPanelItemsScrollView _scrollView;
        private EmotionPanelItemsScrollViewModel _scrollViewModel;
        [Inject] private EmotionsProvider _emotionsProvider;

        public event Action<long> EmotionSelected; 

        public async void Init()
        {
            _scrollViewModel = new EmotionPanelItemsScrollViewModel();

            var emotions = await _emotionsProvider.GetEmotionsAsync();
            var emotionUiItemModels = emotions.Select(x => new EmojiUiItemModel
            {
                Emoji = x,
                OnClick = OnEmojiSelected
            });
            _scrollViewModel.EmojiUiItemModels = emotionUiItemModels.ToList();
            _scrollView.Init(_scrollViewModel);
        }

        private void OnEmojiSelected(Emotion emoji)
        {
            foreach (var emojiUiItemModel in _scrollViewModel.EmojiUiItemModels)
            {
                emojiUiItemModel.IsSelected = emoji.Id == emojiUiItemModel.Emoji.Id;
            }
            
            _scrollView.RefreshSelectionState();
            EmotionSelected?.Invoke(emoji.Id);
        }
    }
}