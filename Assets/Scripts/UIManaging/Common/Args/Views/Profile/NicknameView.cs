using System;
using Abstract;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common.Args.Views.Profile
{
    public sealed class NicknameView : BaseContextDataView<NicknameModel>
    {
        [SerializeField] private TextMeshProUGUI _nicknameText;
        [SerializeField] private Button _button;

        private void Awake()
        {
            _button.onClick.AddListener(OnClicked);
        }

        protected override void OnInitialized()
        {
            _nicknameText.text = string.IsNullOrEmpty(ContextData.Nickname) ? "<NameIsNull>" : ContextData.Nickname;
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            _nicknameText.text = String.Empty;
        }

        private void OnClicked()
        {
            ContextData.OnClicked?.Invoke();
        }
    }

    public sealed class NicknameModel
    {
        public string Nickname { get; set; }
        public bool ShowNotificationMark { get; set; }
        public Action OnClicked { get; set; } 
    }
}