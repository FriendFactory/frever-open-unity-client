using System.Linq;
using Bridge;
using Bridge.Models.VideoServer;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Common.VideoDetails.VideoAttributes
{
    public class TemplateVideoAttribute : VideoAttribute
    {
        [SerializeField] private TMP_Text _templateName;
        [SerializeField] private Button _enabledButton;
        [SerializeField] private Button _disabledButton;
        
        private long[] _templateIdsToIgnore;
        
        private IDataFetcher _userData;
        private IBridge _bridge;

        [Inject, UsedImplicitly]
        public void Construct(IDataFetcher userData, IBridge bridge)
        {
            _userData = userData;
            _bridge = bridge;
        }

        protected override bool ShouldBeVisible()
        {
            var showButton = ShouldShowVideoBasedOnTemplateButton() 
                            && Video.MainTemplate != null
                            && Video.Access == VideoAccess.Public;

            if (showButton)
            {
                OnTemplateButtonShown();
                _enabledButton.onClick.AddListener(OnClicked);
                return true;
            }
            
            if (Video.GeneratedTemplateId.HasValue && Video.GroupId == _bridge.Profile.GroupId && Video.Access != VideoAccess.Public)
            {
                _disabledButton.gameObject.SetActive(true);
            }

            return false;
        }

        protected virtual void OnTemplateButtonShown()
        {
            _templateName.text = ContextData.Video.MainTemplate.Title;
        }

        protected override void BeforeCleanUp()
        {
            _enabledButton.onClick.RemoveListener(OnClicked);
        }

        private bool ShouldShowVideoBasedOnTemplateButton()
        {
            _templateIdsToIgnore ??= new[] { _userData.DefaultTemplateId };
            
            var template = Video.MainTemplate;
            var show = template != null && !_templateIdsToIgnore.Contains(template.Id) && ContextData.ShowBasedOnTemplateAttribute;
            
            return show;
        }
    }
}