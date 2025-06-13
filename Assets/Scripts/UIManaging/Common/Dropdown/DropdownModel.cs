using System.Collections.Generic;

namespace UIManaging.Common.Dropdown
{
    public sealed class DropdownModel
    {
        public readonly List<string> Options;

        public DropdownModel(List<string> options)
        {
            Options = options;
        }
    }
}