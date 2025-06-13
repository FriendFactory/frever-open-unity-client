namespace UIManaging.PopupSystem.Popups.SwipeToFollow.States
{
    internal sealed class UserCardReleasedViewState: UserCardViewStateBase
    {
        protected override void OnEnter()
        {
            gameObject.SetActive(false);
        }

        protected override void OnExit()
        {
            gameObject.SetActive(true);
        }
    }
}