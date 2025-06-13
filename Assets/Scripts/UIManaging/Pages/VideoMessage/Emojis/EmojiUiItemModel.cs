using System;
using Bridge.Models.ClientServer;

namespace UIManaging.Pages.VideoMessage.Emojis
{
    internal sealed class EmojiUiItemModel
    {
        public Emotion Emoji;
        public bool IsSelected;
        public Action<Emotion> OnClick;
    }
}