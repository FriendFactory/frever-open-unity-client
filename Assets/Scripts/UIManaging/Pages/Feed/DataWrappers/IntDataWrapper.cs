namespace Core.DataWrapper
{
    public class IntDataWrapper : BaseDataWrapper<int>
    {
        public IntDataWrapper(int value) : base(value) {}

        protected override int AddInternal(int value)
        {
            return Value + value;
        }

        protected override int SubtractInternal(int value)
        {
            return Value - value;
        }
    }
}