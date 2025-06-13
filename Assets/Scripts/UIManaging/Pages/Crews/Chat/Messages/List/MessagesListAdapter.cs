using System;
using System.Collections.Generic;
using Com.ForbiddenByte.OSA.Core;
using Com.ForbiddenByte.OSA.DataHelpers;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Crews
{
	// There are 2 important callbacks you need to implement, apart from Start(): CreateViewsHolder() and UpdateViewsHolder()
	// See explanations below
	// public class MessagesListAdapter : OSA<BaseParamsWithPrefab, MessageItemViewsHolder>
	public class MessagesListAdapter : OSA<MessagesListParams, MessageItemViewsHolder>
    {
        private const int LOADING_ITEMS_THRESHOLD = 5;

        private readonly Dictionary<long, Texture2D> _userThumbnailsCache  = new Dictionary<long, Texture2D>();

        private double _prevScrollPos = 0;
        private int _firstVisibleItemIndex = -1;
        private int _lastVisibleItemIndex = -1;

        [Inject] private LocalUserDataHolder _userData;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

		// Helper that stores data and notifies the adapter when items count changes
		// Can be iterated and can also have its elements accessed by the [] operator
		public SimpleDataHelper<MessageItemModel> Data { get; private set; }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public Action ScrolledToTop;
        public Action ScrolledToBottom;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void Start()
        {
            Data = new SimpleDataHelper<MessageItemModel>(this);

            // Calling this initializes internal data and prepares the adapter to handle item count changes
            base.Start();
        }

        protected override void OnDestroy()
        {
            foreach (var texture in _userThumbnailsCache.Values)
            {
                Destroy(texture);
            }

            _userThumbnailsCache.Clear();

            base.OnDestroy();
        }

        //---------------------------------------------------------------------
        // OSA Implementation
        //---------------------------------------------------------------------

        protected override void OnScrollPositionChanged(double normPos)
        {
            base.OnScrollPositionChanged(normPos);
            _prevScrollPos = normPos;

            if (VisibleItemsCount == 0) return;

            if (normPos - _prevScrollPos > 0)
            {
                CheckBottomScrollingThreshold();
            }
            else
            {
                CheckTopScrollingThreshold();
            }
        }

		// This is called initially, as many times as needed to fill the viewport, 
		// and anytime the viewport's size grows, thus allowing more items to be displayed
		// Here you create the "ViewsHolder" instance whose views will be re-used
		// *For the method's full description check the base implementation
		protected override MessageItemViewsHolder CreateViewsHolder(int itemIndex)
        {
            var data = Data[itemIndex];
            var modelType = data.MessageType;

            switch (modelType)
            {
                case MessageType.Own:
                {
                    var instance = new OwnItemViewsHolder();
                    instance.Init(_Params.OwnMessagePrefab, _Params.Content, itemIndex);
                    instance.SetUserThumbnailsCache(_userThumbnailsCache);
                    return instance;
                }
                case MessageType.User:
                {
                    var instance = new UserItemViewsHolder();
                    instance.Init(_Params.UserMessagePrefab, _Params.Content, itemIndex);
                    instance.SetUserThumbnailsCache(_userThumbnailsCache);
                    return instance;
                }
                case MessageType.System:
                {
                    var instance = new SystemItemViewsHolder();
                    instance.Init(_Params.SystemMessagePrefab, _Params.Content, itemIndex);
                    return instance;
                }
                case MessageType.Bot:
                {
                    var instance = new BotItemViewsHolder();
                    instance.Init(_Params.BotMessagePrefab, _Params.Content, itemIndex);
                    return instance;
                }
                default:
                    Debug.LogError($"Unknown message type: {modelType}");
                    return null;
            }
        }

        // This is called anytime a previously invisible item become visible, or after it's created,
        // or when anything that requires a refresh happens
        // Here you bind the data from the model to the item's views
        // *For the method's full description check the base implementation
        protected override void UpdateViewsHolder(MessageItemViewsHolder newOrRecycled)
        {
            // In this callback, "newOrRecycled.ItemIndex" is guaranteed to always reflect the
            // index of item that should be represented by this views holder. You'll use this index
            // to retrieve the model from your data set
            var model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.MessageItemView.Initialize(model);

            if (model.HasPendingSizeChange)
            {
                // Height will be available before the next 'twin' pass, inside OnItemHeightChangedPreTwinPass() callback (see above)
                ScheduleComputeVisibilityTwinPass(true);
            }
        }

        /// <summary>Overriding the base implementation, which always returns true. In this case, a views holder is recyclable only if its <see cref="BaseVH.CanPresentModelType(Type)"/> returns true for the model at index <paramref name="indexOfItemThatWillBecomeVisible"/></summary>
        /// <seealso cref="OSA{TParams, TItemViewsHolder}.IsRecyclable(TItemViewsHolder, int, double)"/>
        protected override bool IsRecyclable(MessageItemViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
        {
            var model = Data[indexOfItemThatWillBecomeVisible];
            return potentiallyRecyclable.CanPresentModelType(model.CachedType);
        }

        // <summary>
        /// Only called for vertical ScrollRects. Called just before a "Twin" ComputeVisibility will execute.
        /// This can be used, for example, to disable a ContentSizeFitter on the item which was used to externally calculate the item's size in the current Twin ComputeVisibility pass</summary>
        /// <seealso cref="ScheduleComputeVisibilityTwinPass(bool)"/>
        protected override void OnItemHeightChangedPreTwinPass(MessageItemViewsHolder viewsHolder)
        {
            base.OnItemHeightChangedPreTwinPass(viewsHolder);
            var model = Data[viewsHolder.ItemIndex];
            model.HasPendingSizeChange = false;
        }

        /// <summary>
        /// <para>Called mainly when it's detected that the scroll view's size has changed. Marks everything for a layout rebuild and then calls Canvas.ForceUpdateCanvases(). </para>
        /// <para>IMPORTANT: Make sure to override <see cref="AbstractViewsHolder.MarkForRebuild"/> in your views holder implementation if you have child layout groups and call LayoutRebuilder.MarkForRebuild() on them</para>
        /// <para>After this call, <see cref="Refresh(bool, bool)"/> will be called</para>
        /// </summary>
        protected override void RebuildLayoutDueToScrollViewSizeChange()
        {
            // Invalidate the last sizes so that they'll be re-calculated
            SetAllModelsHavePendingSizeChange();

            base.RebuildLayoutDueToScrollViewSizeChange();
        }

        /// <summary>
        /// When the user resets the count or refreshes, the OSA's cached sizes are cleared so we can recalculate them.
        /// This is provided here for new users that just want to call Refresh() and have everything updated instead of telling OSA exactly what has updated.
        /// But, in most cases you shouldn't need to ResetItems() or Refresh() because of performace reasons:
        /// - If you add/remove items, InsertItems()/RemoveItems() is preferred if you know exactly which items will be added/removed;
        /// - When just one item has changed externally and you need to force-update its size, you'd call ForceRebuildViewsHolderAndUpdateSize() on it;
        /// - When the layout is rebuilt (when you change the size of the viewport or call ScheduleForceRebuildLayout()), that's already handled
        /// So the only case when you'll need to call Refresh() (and override ChangeItemsCount()) is if your models can be changed externally and you'll only know that they've changed, but won't know which ones exactly.
        /// </summary>
        public override void ChangeItemsCount(ItemCountChangeMode changeMode, int itemsCount, int indexIfInsertingOrRemoving = -1, bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
        {
            if (changeMode == ItemCountChangeMode.RESET)
                SetAllModelsHavePendingSizeChange();

            base.ChangeItemsCount(changeMode, itemsCount, indexIfInsertingOrRemoving, contentPanelEndEdgeStationary, keepVelocity);
        }

        private void SetAllModelsHavePendingSizeChange()
        {
            foreach (var model in Data) model.HasPendingSizeChange = true;
        }

        //---------------------------------------------------------------------
        // Data Manipulation
        //---------------------------------------------------------------------

        public void ResetItems(IList<MessageItemModel> items)
        {
            Data.ResetItems(items);
        }

        public void InsertItemsAtStart(IList<MessageItemModel> models, bool freezeEndEdge = false)
        {
            Data.InsertItemsAtStart(models, freezeEndEdge);
        }

        public void InsertItemsAtEnd(IList<MessageItemModel> models, bool freezeEndEdge = false)
        {
            Data.InsertItemsAtEnd(models, freezeEndEdge);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void CheckTopScrollingThreshold()
        {
            var currentItemIndex = GetItemViewsHolder(0).ItemIndex;

            if (_firstVisibleItemIndex == currentItemIndex) return;
            _firstVisibleItemIndex = currentItemIndex;

            if (_firstVisibleItemIndex < LOADING_ITEMS_THRESHOLD) ScrolledToTop?.Invoke();
        }

        private void CheckBottomScrollingThreshold()
        {
            var currentItemIndex = GetItemViewsHolder(VisibleItemsCount - 1).ItemIndex;

            if (_lastVisibleItemIndex == currentItemIndex) return;
            _lastVisibleItemIndex = currentItemIndex;

            if (_lastVisibleItemIndex < LOADING_ITEMS_THRESHOLD) ScrolledToBottom?.Invoke();
        }
	}
}