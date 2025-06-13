using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Navigation.Core;
using TipsManagment.Args;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TipsManagment
{
    [UsedImplicitly]
    public sealed class TipManager: ITipTargetsProvider
    {
        private readonly TutorialConfig _tutorialConfig;
        private readonly PageManager _pageManager;

        private readonly List<BaseTip> _pageTips = new List<BaseTip>();
        private readonly List<BaseTip> _stateTips = new List<BaseTip>();
        private RectTransform _tipsHolder;
        private readonly TutorialProgress _progress;
        private readonly HashSet<TipId> _ignoredTipIds = new HashSet<TipId>();
        private BaseTip _currentTip;
        private readonly Dictionary<TipId, ITipTarget> _targetsMap;
        
        public TipManager(PageManager pageManager, TutorialConfig tutorialConfig)
        {
            _pageManager = pageManager;
            _tutorialConfig = tutorialConfig;
            _targetsMap = new Dictionary<TipId, ITipTarget>();
            _progress = new TutorialProgress();
            _progress.Load();
            
            _pageManager.PageDisplayed += OnPageDisplayed;
        }
        
        public bool TryGetTarget(TipId id, out ITipTarget target)
        {
            return _targetsMap.TryGetValue(id, out target);
        }
        
        public void AddTarget(TipId id, ITipTarget target)
        {
            if (_targetsMap.ContainsKey(id))
            {
                _targetsMap[id] = target;
                return;
            }
            
            _targetsMap.Add(id, target);
        }
        
        public void RemoveTarget(TipId id)
        {
            if (!_targetsMap.ContainsKey(id)) return;
            
            _targetsMap.Remove(id);
        }

        public void ShowTipsById(TipId id)
        {
            PrepareTips(_tutorialConfig.GetDirectTips(id), TipType.Direct);
            RollTips(TipType.Direct);
        }
        
        public void ShowTipsByPageId(PageId pageId, TipType pageType = TipType.Page)
        {
            PrepareTips(_tutorialConfig.GetPageTips(pageId), TipType.Page);
            if (pageType == TipType.PageParallel)
            {
                RollTipsParallel(pageType);
            }
            else
            {
                RollTips(pageType);
            }
        }

        private void PrepareTips(List<TipPreset> tipPresets, TipType type)
        {
            DespawnOldTips(type);
            
            if (type == TipType.Page)
            {
                RemoveShownTips(tipPresets);
            }
            
            SortTips(ref tipPresets);
            SpawnTips(tipPresets, type);
        }

        public void SetHintHolder(RectTransform holderTransform)
        {
            _tipsHolder = holderTransform;
        }

        public bool AreDirectTipsDone(TipId tipId)
        {
            var tipPresets = _tutorialConfig.GetDirectTips(tipId);

            return tipPresets.All(IsTipDone);
        }

        private void RemoveShownTips(IList<TipPreset> tipsList)
        {
            for (var i = tipsList.Count - 1; i >= 0; i--)
            {
                var tip = tipsList[i];
                if (IsTipDone(tip))
                {
                    tipsList.Remove(tip);
                }
            }
        }

        private bool IsTipDone(TipPreset tipPreset)
        {
            var progressItem = _progress.GetItem((long)tipPreset.Settings.Id);
            return progressItem.Done || _ignoredTipIds.Contains(tipPreset.Settings.Id);
        }

        private static void SortTips(ref List<TipPreset> tipPresets)
        {
            tipPresets = tipPresets.OrderBy(x => x.Settings.HintSequenceOrder).ToList();
        }

        private void OnPageDisplayed(PageData data)
        {
            if (!data.PageArgs.ShowHintsOnDisplay)
            {
                return;
            }
            
            ShowTipsByPageId(data.PageId);
        }

        private void SpawnTips(List<TipPreset> tipPresets, TipType type)
        {
            if (_pageManager.CurrentPage == null)
            {
                return;
            }
            
            foreach (var tipPreset in tipPresets)
            {
                var tipObject = Object.Instantiate(tipPreset.TipPrefab, _tipsHolder);
                var tip = tipObject.GetComponent<BaseTip>();

                GetTipsByType(type).Add(tip);

                var target = tipPreset.UseTarget
                    ? TryGetTarget(tipPreset.Settings.Id, out var tipTarget) ? tipTarget : null
                    : null;
                var tipArgs = new TipArgs(tipPreset, _pageManager.CurrentPage.transform, type, target);

                tip.Init(tipArgs);
                tipObject.SetActive(false);
            }
        }

        private void DespawnOldTips(TipType type)
        {
            var tips = GetTipsByType(type);
            
            foreach (var targetTip in tips.Where(tip => tip != null))
            {
                Object.Destroy(targetTip.gameObject);
            }

            tips.Clear();
        }

        private async void RollTips(TipType type)
        {
            var tips = GetTipsByType(type);
            
            if (!tips.Any())
            {
                return;
            }
            
            _currentTip = tips.First();
            _currentTip.TipDone += OnTipDone;
            _currentTip.TipIgnored += OnTipIgnored;
            await _currentTip.Activate();
        }
        
        private async void RollTipsParallel(TipType type)
        {
            try
            {
                var tips = GetTipsByType(type);
                
                if (!tips.Any())
                {
                    return;
                }
                
                var activateTasks = tips.Select(tip =>
                {
                    tip.TipDone += OnParallelTipDone;
                    tip.TipIgnored += OnParallelTipIgnore;

                    return tip.Activate();
                });
                
                await Task.WhenAll(activateTasks);

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            void OnParallelTipDone(BaseTip tip)
            {
                RemoveFromListParallel(tip);
                _progress.MarkTipAsDone((long)tip.Id);
                _progress.Save();
            }
            
            void OnParallelTipIgnore(BaseTip tip)
            {
                RemoveFromListParallel(tip);
                _ignoredTipIds.Add(tip.Id);
                var progressItem = _progress.GetItem((long)tip.Id);
                if (tip.PromptAgain >= 0 && ++progressItem.IgnoreCount >= tip.PromptAgain)
                {
                    _progress.MarkTipAsDone((long)tip.Id);
                }
                _progress.Save();
            }
            
            void RemoveFromListParallel(BaseTip tip)
            {
                var candidate = _pageTips.First(pageTip => pageTip.Id == tip.Id);
                
                _pageTips.Remove(candidate);
                candidate.TipDone -= OnParallelTipDone;
                candidate.TipIgnored -= OnParallelTipIgnore;
                Object.Destroy(candidate.gameObject);
            }
        }

        private void OnTipDone(BaseTip tip)
        {
            RemoveTipFromList(tip);
            RollTips(tip.CurrentType);
            _progress.MarkTipAsDone((long)tip.Id);
            _progress.Save();
        }

        private void OnTipIgnored(BaseTip tip)
        {
            RemoveTipFromList(tip);
            RollTips(tip.CurrentType);
            _ignoredTipIds.Add(tip.Id);
            var progressItem = _progress.GetItem((long)tip.Id);
            if (tip.PromptAgain >= 0 && ++progressItem.IgnoreCount >= tip.PromptAgain)
            {
                _progress.MarkTipAsDone((long)tip.Id);
            }
            _progress.Save();
        }

        private void RemoveTipFromList(BaseTip tip)
        {
            if (_currentTip == tip)
            {
                _currentTip = null;
            }

            var tips = GetTipsByType(tip.CurrentType);
            
            tips.Remove(tip);
            tip.TipDone -= OnTipDone;
            tip.TipIgnored -= OnTipIgnored;
            Object.Destroy(tip.gameObject);
        }

        private List<BaseTip> GetTipsByType(TipType type)
        {
            switch (type)
            {
                case TipType.Page:
                case TipType.PageParallel:
                    return _pageTips;
                case TipType.Direct:
                    return _stateTips;
                default:
                    Debug.LogError($"Unknown tip type: {type}");
                    return null;
            }
        }
    }
}