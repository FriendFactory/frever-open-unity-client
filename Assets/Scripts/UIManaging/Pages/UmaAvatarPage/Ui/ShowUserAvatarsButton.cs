using System;
using Navigation.Args;
using Navigation.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.UmaAvatarPage.Ui
{
    [RequireComponent(typeof(Button))]
    public class ShowUserAvatarsButton : MonoBehaviour
    {
        private Button _button;
        private Func<Action> _backButtonActionGetter;
        private PageManager _manager;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Init(PageManager manager, Func<Action> backButtonActionGetter)
        {
            _manager = manager;
            _backButtonActionGetter = backButtonActionGetter;
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OpenAvatarPage);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OpenAvatarPage()
        {
            var args = new UmaAvatarArgs();
            _manager.MoveNext(PageId.AvatarPage, args);
        }
    }
}