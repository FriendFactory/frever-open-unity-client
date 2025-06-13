namespace Abstract
{
    public interface IDisplayable
    {
        bool IsDisplayed { get; }

        void Show();
        void Hide();
    }
}