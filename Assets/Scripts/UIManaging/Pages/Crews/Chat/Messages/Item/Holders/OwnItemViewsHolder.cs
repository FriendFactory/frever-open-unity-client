using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIManaging.Pages.Crews
{
	public class OwnItemViewsHolder : MessageItemViewsHolder
	{
        private OwnMessageView _messageItemView;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override bool CanPresentModelType(Type modelType) { return modelType == typeof(OwnMessageModel); }

        internal override MessageItemView<MessageItemModel> MessageItemView => _messageItemView;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void CollectViews()
        {
            base.CollectViews();
            _messageItemView = root.GetComponent<OwnMessageView>();
        }

        public void SetUserThumbnailsCache(Dictionary<long, Texture2D> userThumbnailsCache)
        {
            _messageItemView.SetUserThumbnailsCache(userThumbnailsCache);
        }
	}
}