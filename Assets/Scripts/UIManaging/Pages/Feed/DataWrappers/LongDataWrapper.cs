namespace Core.DataWrapper
{
    public class LongDataWrapper : BaseDataWrapper<long>
    {
        public LongDataWrapper(long value) : base(value) {}

        protected override long AddInternal(long value)
        {
            return Value + value;
        }

        protected override long SubtractInternal(long value)
        {
            return Value - value;
        }
    }
}