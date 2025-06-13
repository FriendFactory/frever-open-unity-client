using UIManaging.Common.Args.Buttons;

namespace UIManaging.Common.PageHeader
{
    public class PageHeaderArgs
    {
        public string HeaderText { get; private set; }
        public ButtonArgs LeftButtonArgs { get; private set; }

        public PageHeaderArgs(string headerText, ButtonArgs leftButtonArgs)
        {
            HeaderText = headerText;
            LeftButtonArgs = leftButtonArgs;
        }
    }
}