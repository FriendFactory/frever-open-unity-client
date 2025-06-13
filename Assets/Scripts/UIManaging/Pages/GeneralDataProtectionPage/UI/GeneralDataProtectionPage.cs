using Navigation.Core;
using UI.UIAnimators;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.PageHeader;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.GeneralDataProtectionPage.Ui
{
    public class GeneralDataProtectionPage : GenericPage<GeneralDataProtectionPageArgs>
    {
        public override PageId Id { get; } = PageId.GeneralDataProtection;
        
        [SerializeField] private Toggle _toggle;
        [SerializeField] private RectTransform _bodyRectTransform;
        [SerializeField] private PageHeaderView _pageHeaderView;
        [SerializeField] private SequentialUiAnimationPlayer _sequentialUiAnimationPlayer;
        
        [Inject] private PageManager _pageManager;
        [Inject] private LocalUserDataHolder _localUserDataHolder;

        protected override void OnInit(PageManager pageManager)
        {
            _pageHeaderView.Init(new PageHeaderArgs("Data Collection", new ButtonArgs(string.Empty, _pageManager.MoveBack)));
        }
        
        protected override void OnDisplayStart(GeneralDataProtectionPageArgs args)
        {
            base.OnDisplayStart(args);
            _toggle.onValueChanged.RemoveAllListeners();

            var isEnabled = _localUserDataHolder.DataCollection.HasValue && _localUserDataHolder.DataCollection.Value;
            _toggle.isOn = isEnabled;
            _toggle.onValueChanged.AddListener(call: (value) => args.OnGeneralDataProtectionToggleChanged(value));
        }
    }
}