using Bridge.Models.ClientServer.Template;
using Bridge;
using Navigation.Args;
using TMPro;
using UIManaging.Common;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.OnboardingTemplateSelection
{
    internal sealed class CarouselTemplateItem : MonoBehaviour
    {
        [SerializeField] private TemplateThumbnail _templateThumbnail;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _descrition;
        [SerializeField] private Button _button;
        
        [Inject] private IBridge _bridge;
        
        private TemplateInfo _templateInfo;

        public void Initialize(TemplateInfo templateInfo, UnityAction onClick)
        {
            _title.text = templateInfo.Title;
            _descrition.text = templateInfo.Description;
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(onClick);
            SetupThumbnail(templateInfo);
        }

        private void SetupThumbnail(TemplateInfo info)
        {
            if (info.Id == _templateInfo?.Id) return;
            
            _templateThumbnail.Initialize(new VideoThumbnailModel(_bridge.GetTemplateVideoUrl(info)));
            _templateInfo = info;
        }
        
    }
}