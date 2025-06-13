using System;
using Navigation.Args;
using Navigation.Core;

namespace UIManaging.Pages.EditUsername
{
    public sealed class EditUsernamePageArgs : PageArgs 
    {
        public override PageId TargetPage => PageId.EditUsername;

        public string Name;

        public Action<string> UpdateRequested;

        public EditUsernamePageArgs(string name)
        {
            Name = name;
        }
    }
}
