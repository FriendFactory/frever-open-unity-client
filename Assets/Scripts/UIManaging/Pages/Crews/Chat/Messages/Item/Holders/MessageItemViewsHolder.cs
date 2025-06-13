using System;
using Com.ForbiddenByte.OSA.Core;
using UnityEngine.UI;

namespace UIManaging.Pages.Crews
{
	public abstract class MessageItemViewsHolder : BaseItemViewsHolder
	{
        private ContentSizeFitter _contentSizeFitter;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        internal abstract MessageItemView<MessageItemModel> MessageItemView { get; }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

		public override void CollectViews()
		{
			base.CollectViews();

            _contentSizeFitter = root.GetComponent<ContentSizeFitter>();
            _contentSizeFitter.enabled = false;
		}

		// Override this if you have children layout groups or a ContentSizeFitter on root that you'll use. 
		// They need to be marked for rebuild when this callback is fired
		public override void MarkForRebuild()
		{
			base.MarkForRebuild();
            if (_contentSizeFitter) _contentSizeFitter.enabled = true;
		}

		// Override this if you've also overridden MarkForRebuild() and you have enabled size fitters there (like a ContentSizeFitter)
		public override void UnmarkForRebuild()
		{
            if (_contentSizeFitter) _contentSizeFitter.enabled = false;
			base.UnmarkForRebuild();
		}

        public abstract bool CanPresentModelType(Type modelType);
	}
}