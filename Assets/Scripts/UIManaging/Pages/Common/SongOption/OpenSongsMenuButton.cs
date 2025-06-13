using Modules.Amplitude;
using JetBrains.Annotations;
using UIManaging.Core;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Common.SongOption
{
    internal sealed class OpenSongsMenuButton : ButtonBase
    {
        [SerializeField] private MusicSelectionPage _musicSelectionPage;
        [SerializeField] private SelectionPurpose _selectionPurpose;
        private AmplitudeManager _amplitudeManager;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject, UsedImplicitly]
        private void Construct(AmplitudeManager amplitudeManager)
        {
            _amplitudeManager = amplitudeManager;
        }

        //---------------------------------------------------------------------
        // public
        //---------------------------------------------------------------------
        
        public void SetInteractable(bool interactable)
        {
            Interactable = interactable;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnClick()
        {
            // workaround because displaying using PageManger leads to error during PiP opening
            _musicSelectionPage.Display(new MusicSelectionPageArgs
            {
                SelectionPurpose = _selectionPurpose
            }, null, null);
            _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.SONG_MENU_BUTTON_CLICKED);
        }
    }
}