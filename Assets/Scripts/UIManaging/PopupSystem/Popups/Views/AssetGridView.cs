using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    public class AssetGridView : MonoBehaviour
    {
        private RectTransform _content;
        [SerializeField] private Text _title;

        public RectTransform Content {
            get {return _content;}
        }
        public Text Title {
            get {return _title;}
        }
        public void Setup(string title, Action OnSeeAllAction) {
            _title.text = title;
        }
    }
}
