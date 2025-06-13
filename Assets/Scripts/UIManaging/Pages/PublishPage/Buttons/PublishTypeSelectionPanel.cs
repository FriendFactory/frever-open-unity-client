using System;
using System.Linq;
using Common.Publishers;
using UnityEngine;

namespace UIManaging.Pages.PublishPage.Buttons
{
    internal sealed class PublishTypeSelectionPanel: MonoBehaviour
    {
        [SerializeField] private PublishTypeSelectionButton[] _postTypeSelectionButtons;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public PublishingType CurrentlySelected => _postTypeSelectionButtons.First(x => x.IsSelected).PublishingType;
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action<PublishingType> PublishTypeChanged;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            foreach (var selectionButton in _postTypeSelectionButtons)
            {
                selectionButton.Selected += OnPublishTypeChanged;
            }
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Init(PublishingType initialSelected)
        {
            foreach (var selectionButton in _postTypeSelectionButtons)
            {
                selectionButton.Init(initialSelected == selectionButton.PublishingType);
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnPublishTypeChanged(PublishingType publishingType)
        {
            PublishTypeChanged?.Invoke(publishingType);
        }
    }
}