using Extensions;
using TMPro;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class TaskInfoPopup: BasePopup<TaskInfoConfiguration>
    {
        [SerializeField] private Button _okButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _description;

        private void Awake()
        {
            _okButton.onClick.AddListener(Hide);
            _closeButton.onClick.AddListener(Hide);
        }

        protected override void OnConfigure(TaskInfoConfiguration configuration)
        {
            _title.text = configuration.Title;
            _description.text = configuration.Description;
            _description.transform.parent.SetActive(!string.IsNullOrEmpty(configuration.Description));
        }
    }
}