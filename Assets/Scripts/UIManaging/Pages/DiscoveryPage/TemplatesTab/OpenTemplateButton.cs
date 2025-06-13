using System;
using Bridge.Models.ClientServer.Template;
using Navigation.Args;
using Navigation.Args.Feed;
using Navigation.Core;
using UIManaging.Pages.Common.VideoManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.DiscoveryPage
{
   [RequireComponent(typeof(Button))]
   internal sealed class OpenTemplateButton : MonoBehaviour
   {
      [Inject] private PageManager _pageManager;
      [Inject] private VideoManager _videoManager;
      [SerializeField] private PageToOpen _pageToOpen;

      private Button _button;
      private TemplateInfo _template;

      //---------------------------------------------------------------------
      // Messages
      //---------------------------------------------------------------------

      private void Awake()
      {
         _button = GetComponent<Button>();
      }

      private void OnEnable()
      {
         _button.onClick.AddListener(OnClick);
      }

      private void OnDisable()
      {
         _button.onClick.RemoveListener(OnClick);
      }

      //---------------------------------------------------------------------
      // Public
      //---------------------------------------------------------------------

      public void Setup(TemplateInfo template)
      {
         _template = template;
      }

      //---------------------------------------------------------------------
      // Helpers
      //---------------------------------------------------------------------

      private void OnClick()
      {
          switch (_pageToOpen)
          {
              case PageToOpen.TemplateVideoGrid:
                  OpenTemplatePage();
                  break;
              case PageToOpen.TemplateVideoFeed:
                  OpenTemplateVideoFeedPage();
                  break;
              default:
                  throw new ArgumentOutOfRangeException();
          }
      }
      
      private void OpenTemplatePage()
      {
          var pageArgs = new VideosBasedOnTemplatePageArgs
          {
              TemplateInfo = _template,
              TemplateName = _template.Title,
              UsageCount = _template.UsageCount
          };

          _pageManager.MoveNext(PageId.VideosBasedOnTemplatePage, pageArgs);
      }

      private void OpenTemplateVideoFeedPage()
      {
          var args = new VideosBasedOnTemplateFeedArgs(_videoManager, _template.Id);
          _pageManager.MoveNext(args);
      }
      
      private enum PageToOpen
      {
          TemplateVideoGrid,
          TemplateVideoFeed
      }
   }
}