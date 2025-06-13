using Abstract;

namespace UIManaging.PopupSystem.Popups.SwipeToFollow.States
{
    internal abstract class UserCardViewStateBase: BaseContextDataView<SwipeToFollowUserCardModel> 
    {
        public bool IsActive { get; private set; }
        
        protected override void OnInitialized() { }

        protected abstract void OnEnter();
        protected abstract void OnExit();

        public void Enter()
        {
            IsActive = true;
            
            OnEnter();
        }
        
        public void Exit()
        {
            IsActive = false;
            
            OnExit();
        }
    }
}