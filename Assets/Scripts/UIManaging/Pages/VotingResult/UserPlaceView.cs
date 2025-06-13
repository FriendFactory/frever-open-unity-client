using System;
using System.Linq;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.VotingResult
{
    internal sealed class UserPlaceView: MonoBehaviour
    {
        [SerializeField] private Image _winnerIconImage;
        [SerializeField] private PlaceIcon[] _placeIcons;
        [SerializeField] private TMP_Text _text;

        public void Setup(int place)
        {
            var shouldShowIcon = _placeIcons.Any(x => x.Place == place);
            _winnerIconImage.SetActive(shouldShowIcon);
            _text.SetActive(!shouldShowIcon);
            if (shouldShowIcon)
            {
                _winnerIconImage.sprite = _placeIcons.First(x => x.Place == place).Icon;
            }
            else
            {
                _text.text = $"{place}th";
            }
        }
        
        [Serializable]
        private struct PlaceIcon
        {
            public int Place;
            public Sprite Icon;
        }
    }
}