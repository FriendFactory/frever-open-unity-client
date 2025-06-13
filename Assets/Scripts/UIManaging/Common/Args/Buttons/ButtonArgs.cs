using System;

namespace UIManaging.Common.Args.Buttons
{
    public class ButtonArgs
    {
        public string Text { get; private set; }
        public Action Action { get; private set; }

        public ButtonArgs(string text, Action action)
        {
            Text = text;
            Action = action;
        }
    }
}