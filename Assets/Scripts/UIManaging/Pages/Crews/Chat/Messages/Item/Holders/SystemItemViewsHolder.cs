using System;

namespace UIManaging.Pages.Crews
{
	public class SystemItemViewsHolder : MessageItemViewsHolder
	{
        private SystemMessageView _messageItemView;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override bool CanPresentModelType(Type modelType) { return modelType == typeof(SystemMessageModel); }

        internal override MessageItemView<MessageItemModel> MessageItemView => _messageItemView;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public override void CollectViews()
        {
            base.CollectViews();
            _messageItemView = root.GetComponent<SystemMessageView>();
        }
	}
}