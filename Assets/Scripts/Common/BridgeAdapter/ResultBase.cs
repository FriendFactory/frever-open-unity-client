namespace Common.BridgeAdapter
{
    public class ResultBase
    {
        public bool IsSuccess;
        public string ErrorMessage;
        public bool IsCancelled;
    }

    public class ModelsResult<T> : ResultBase
    {
        public T[] Models;
    }
}