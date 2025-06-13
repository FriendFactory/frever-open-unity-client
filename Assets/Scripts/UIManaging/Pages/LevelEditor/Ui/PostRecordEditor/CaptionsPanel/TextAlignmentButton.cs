using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Level.Full;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.CaptionsPanel
{
    internal sealed class TextAlignmentButton: MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private List<AlignmentIconData> _alignmentIcons;
        private CaptionTextAlignment _alignment;
        private CaptionTextAlignment[] _allAlignments;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<CaptionTextAlignment> Clicked;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClicked);
            _allAlignments = Enum.GetValues(typeof(CaptionTextAlignment)).Cast<CaptionTextAlignment>().ToArray();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Setup(CaptionTextAlignment alignment)
        {
            _alignment = alignment;
            SetIcon(alignment);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnClicked()
        {
            var currentIndex = Array.IndexOf(_allAlignments, _alignment);
            var nextIndex = currentIndex == _allAlignments.Length - 1 ? 0 : currentIndex + 1;
            var nextAlignment = _allAlignments[nextIndex];
            Setup(nextAlignment);
            Clicked?.Invoke(nextAlignment);
        }

        private void SetIcon(CaptionTextAlignment alignment)
        {
            _icon.sprite = _alignmentIcons.First(x => x.Alignment == alignment).Icon;
        }
        
        [Serializable]
        private struct AlignmentIconData
        {
            public Sprite Icon;
            public CaptionTextAlignment Alignment;
        }
    }
}