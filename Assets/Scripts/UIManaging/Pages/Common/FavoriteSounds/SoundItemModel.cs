using System;
using Bridge.Models.Common;

namespace UIManaging.Pages.Common.FavoriteSounds
{
    public class SoundItemModel
    {
        public IPlayableMusic Sound { get; }

        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                if (_isFavorite == value) return;

                _isFavorite = value;
                
                FavoriteStateChanged?.Invoke(value);
            }
        }

        public event Action<bool> FavoriteStateChanged; 

        private bool _isFavorite;

        public SoundItemModel(IPlayableMusic sound, bool isFavorite = false)
        {
            Sound = sound;
            IsFavorite = isFavorite;
        }
    }
}