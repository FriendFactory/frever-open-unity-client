using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abstract;
using Bridge;
using Bridge.Models.ClientServer.UserActivity;
using EnhancedUI.EnhancedScroller;
using Extensions;
using UIManaging.Pages.Crews.TrophyHunt.Lootbox.Lootbox;
using UnityEngine;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Crews.TrophyHunt.Lootbox
{
    public class LootboxRewardsScroller : BaseContextDataView<LootboxRewardModel[]>, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScrollerCellView _rewardPrefab;
        [SerializeField] private EnhancedScroller _scroller;

        [Inject] private IBridge _bridge;

        private HashSet<EnhancedScrollerCellView> _visibleCells = new HashSet<EnhancedScrollerCellView>();
        
        private int _selectedIndex;
        private Action _onComplete;
        private bool _spinCompleted;

        private void Awake()
        {
            _scroller.cellViewVisibilityChanged += OnVisibilityChanged;
        }

        protected override void OnInitialized()
        {
            _visibleCells.Clear();
            _scroller.Delegate = this;
            _spinCompleted = false;
        }

        private void OnVisibilityChanged(EnhancedScrollerCellView cellview)
        {
            if (cellview.active)
            {
                _visibleCells.Add(cellview);
            }
            else
            {
                _visibleCells.Remove(cellview);
            }
        }

        public async void Spin(int selectedIndex, Action onComplete)
        {
            _selectedIndex = selectedIndex;
            _onComplete = onComplete;
            
            await Task.Delay(100);
            _scroller.Velocity = new Vector2(-60000, 0);
            
            await Task.Delay(2000);
            CompleteSelection();
        }

        public void CompleteSelection()
        {
            if (_spinCompleted)
            {
                return;
            }
            
            _spinCompleted = true;
            
            _scroller.Velocity = Vector2.zero;
            
            var offset = 0.5f - GetCellViewSize(_scroller, 0) / _scroller.ScrollRectSize / 2;

            _scroller.JumpToDataIndex(_selectedIndex, 
                                      forceCalculateRange:true,
                                      tweenTime:1f, 
                                      tweenType:EnhancedScroller.TweenType.easeOutQuad,
                                      scrollerOffset:offset,
                                      useSpacing:false,
                                      loopJumpDirection:EnhancedScroller.LoopJumpDirectionEnum.Down,
                                      jumpComplete:() =>
                                      {
                                          PlaySelectedFadeOut();
                                          _onComplete?.Invoke();
                                      });
        }
        
        public int GetNumberOfCells(EnhancedScroller scroller) => ContextData.Length;

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex) => 215;

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(_rewardPrefab);
            var reward = cellView.GetComponent<LootBoxRewardAssetView>();
            reward.Initialize(ContextData[dataIndex]);
            return cellView;
        }

        private void PlaySelectedFadeOut()
        {
            foreach (var cell in _visibleCells)
            {
                var reward = cell.GetComponent<LootBoxRewardAssetView>();

                if(cell.dataIndex == _selectedIndex)
                {
                    reward.SetAsSelected();
                    continue;
                }
                                           
                reward.FadeOut();
            }
        }
    }
}