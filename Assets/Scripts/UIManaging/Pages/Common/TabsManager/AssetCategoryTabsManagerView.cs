using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using DG.Tweening;
using Extensions;
using Modules.Amplitude;
using JetBrains.Annotations;
using ThirdPackagesExtends.Zenject;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Common.TabsManager
{
    public class AssetCategoryTabsManagerView : TabsManagerView<AssetCategoryTabsManagerArgs>
    {
        [SerializeField] private RectTransform _selectionMarker;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private BaseTabView _myAssetsTab;
        [SerializeField] private BaseTabView _recommendedTab;
        [SerializeField] private Image _selectionMarkerImage;
        [SerializeField] private CanvasGroup _scrollRectCanvasGroup;
        
        private List<RectTransform> _childRects;
        private AmplitudeManager _amplitudeManager;
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action TabsCreated;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        [Inject, UsedImplicitly]
        private void Construct(AmplitudeManager amplitudeManager)
        {
            _amplitudeManager = amplitudeManager;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public override void Init(AssetCategoryTabsManagerArgs tabsManagerArgs)
        {
            base.Init(tabsManagerArgs);
            
            _childRects = new List<RectTransform>();
            for (int i = 1; i < _scrollRect.content.childCount; i++)
            {
                var child = _scrollRect.content.GetChild(i);
                if (child.gameObject.activeSelf)
                {
                    _childRects.Add(child.GetComponent<RectTransform>());
                }
            }
            
            foreach (var tab in TabViews)
            {
                tab.OnSelected -= OnCategorySelected;
                tab.OnSelected += OnCategorySelected;
            }
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnTabsSpawned()
        {
            base.OnTabsSpawned();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.GetComponent<RectTransform>());
            TabsCreated?.Invoke();
        }

        protected override void OnToggleSetOn(int index, bool setByUser)
        {
            base.OnToggleSetOn(index, setByUser);
            CoroutineSource.Instance.ExecuteWithFramesDelay(2, Callback);
            
            if(!setByUser)
            {
                _selectionMarkerImage.color = Color.white.SetAlpha(0f);
                _scrollRectCanvasGroup.alpha = 0f;
                CoroutineSource.Instance.ExecuteWithFramesDelay(3, () =>
                {
                    if (IsDestroyed) return;
                    
                    _selectionMarkerImage.color = Color.white.SetAlpha(1f);
                    _scrollRectCanvasGroup.alpha = 1f;
                });
            }
            
            void Callback()
            {
                if (IsDestroyed) return;
                
                var tab = TabViews.FirstOrDefault(x => x.Toggle.isOn);
                if (tab == null) tab = TabViews.FirstOrDefault();
                AdjustToSelectedCategory(tab.RectTransform, setByUser);
                DoTransitionSelectionMarker(tab.RectTransform, setByUser);
            }
        }

        protected override void SpawnTabs()
        {
            TabViews = new List<ITabView>();
            
            var showMyAssetsCategory = TabsManagerArgs.ShowMyAssetsCategory && _amplitudeManager.IsShoppingCartFeatureEnabled();
            
            _myAssetsTab.SetActive(showMyAssetsCategory);
            
            if (showMyAssetsCategory)
            {
                var myAssetsTabModel = new TabModel(AssetCategoryTabsManagerArgs.MY_ASSETS_TAB_INDEX, AssetCategoryTabsManagerArgs.MY_ASSETS_TAB_NAME);
                _myAssetsTab.gameObject.InjectDependenciesIfNeeded();
                _myAssetsTab.Initialize(myAssetsTabModel);
                TabViews.Add(_myAssetsTab);
            }
            
            _recommendedTab.SetActive(TabsManagerArgs.ShowRecommendedCategory);

            if (TabsManagerArgs.ShowRecommendedCategory)
            {
                var recommendedTabModel = new TabModel(AssetCategoryTabsManagerArgs.RECOMMENDED_TAB_INDEX, AssetCategoryTabsManagerArgs.RECOMMENDED_TAB_NAME);
                _recommendedTab.gameObject.InjectDependenciesIfNeeded();
                _recommendedTab.Initialize(recommendedTabModel);
                TabViews.Add(_recommendedTab);
            }
            
            base.SpawnTabs();
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void DoTransitionSelectionMarker(RectTransform targetRect, bool overtime = true)
        {
            _selectionMarker.DOKill();
            _scrollRect.content.DOKill();
            var duration = overtime ? 0.1f : 0f;
            _selectionMarker.DOAnchorPosX(targetRect.anchoredPosition.x, duration)
                .SetEase(Ease.InOutQuart)
                .SetUpdate(true);
        }

        private void AdjustToSelectedCategory(RectTransform targetRect, bool overtime = true)
        {
            var targetIndex = _childRects.IndexOf(targetRect);
            var targetScrollPosition = 0f;
            
            if (targetIndex == _childRects.Count - 1)
            {
                targetScrollPosition = 1f;
            }
            else if (targetIndex > 0)
            {
                targetScrollPosition = targetRect.anchoredPosition.x / _scrollRect.content.sizeDelta.x;
            }

            _scrollRect.DOKill();
            _scrollRect.DOHorizontalNormalizedPos(targetScrollPosition, overtime ? 0.1f : 0f)
                .SetEase(Ease.InOutQuart)
                .SetUpdate(true);
        }

        private void OnCategorySelected(int id, string name)
        {
            var categoryMetaData = new Dictionary<string, object>(2)
            {
                [AmplitudeEventConstants.EventProperties.CATEGORY_ID] = id,
                [AmplitudeEventConstants.EventProperties.CATEGORY_NAME] = name
            };
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.ASSET_CATEGORY_SELECTED, categoryMetaData);
        }
    }
}