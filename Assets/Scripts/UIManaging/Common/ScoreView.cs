using System;
using System.Collections.Generic;
using System.Linq;
using Abstract;
using Bridge.Models.VideoServer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common
{
    internal sealed class ScoreView: BaseContextDataView<VotingResult>
    {
        [SerializeField] private List<PlaceAndIcon> _icons;

        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _score;
        
        protected override void OnInitialized()
        {
            var icon = _icons.FirstOrDefault(x => x.Place == ContextData.Place).Icon;

            if (icon == null)
            {
                gameObject.SetActive(false);
            }
            
            _icon.sprite = icon;
            _score.text = ContextData.Score.ToString("N1");
        }

        [Serializable]
        private struct PlaceAndIcon
        {
            public int Place;
            public Sprite Icon;
        }
    }
}