namespace UIManaging.Common.Dropdown
{
    public sealed class DropdownElementModel
    {
        public readonly string Option;
        public readonly int DataIndex;
        
        public DropdownElementModel(int index, string option)
        {
            Option = option;
            DataIndex = index;
        }
    }
}