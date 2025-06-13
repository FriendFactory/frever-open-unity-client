using System;
using Navigation.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.UpdateAppPage
{
    internal sealed class UpdateAppPage : GenericPage<UpdateAppPageArgs>
    {
        private const string TESTFLIGHT_URL = "https://testflight.apple.com/join/tcESBad2";
        private const string APP_STORE_URL = "https://apps.apple.com/app/frever/id1471858786";
        private const string GOOGLE_PLAY_URL = "https://play.google.com/store/apps/details?id=com.FriendFactory.Frever";
        private const string HIGHLIGHT_COLOR_STRING = "F8069D";

        [SerializeField] private Button _updateButton;
        [SerializeField] private TextMeshProUGUI _testFlightLinkText;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id => PageId.UpdateAppPage;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager manager)
        {
        }

        protected override void OnDisplayStart(UpdateAppPageArgs args)
        {
            base.OnDisplayStart(args);
            _updateButton.onClick.AddListener(OnUpdateButtonClicked);

            _testFlightLinkText.gameObject.SetActive(Application.platform == RuntimePlatform.IPhonePlayer);
            _testFlightLinkText.text =
                $"On TestFlight?<link=\"{TESTFLIGHT_URL}\"><color=#{HIGHLIGHT_COLOR_STRING}> Update here!</color></link>";
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _updateButton.onClick.RemoveListener(OnUpdateButtonClicked);
            base.OnHidingBegin(onComplete);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static void OnUpdateButtonClicked()
        {
            Application.OpenURL(Application.platform == RuntimePlatform.IPhonePlayer ? APP_STORE_URL : GOOGLE_PLAY_URL);
        }
    }
}