using UIManaging.Common.Args.Buttons;

namespace UIManaging.Common.PageHeader
{
    public class PageHeaderActionArgs : PageHeaderArgs
    {
        public ButtonArgs RightButtonArgs { get; private set; }
        
        public PageHeaderActionArgs(string headerText, ButtonArgs leftButtonArgs, ButtonArgs rightButtonArgs) : base(headerText, leftButtonArgs)
        {
            RightButtonArgs = rightButtonArgs;
        }
    }
}