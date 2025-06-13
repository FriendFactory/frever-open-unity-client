using System;

namespace UIManaging.Pages.Crews
{
	public class BotItemViewsHolder : MessageItemViewsHolder
	{
        private BotMessageView _messageItemView;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override bool CanPresentModelType(Type modelType) { return modelType == typeof(BotMessageModel); }

        internal override MessageItemView<MessageItemModel> MessageItemView => _messageItemView;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public override void CollectViews()
        {
            base.CollectViews();
            _messageItemView = root.GetComponent<BotMessageView>();
        }
	}
}