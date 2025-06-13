namespace UIManaging.Core
{
    public class BackButton : ButtonBase
    {
        protected override void OnClick()
        {
            Manager.MoveBack();
        }
    }
}