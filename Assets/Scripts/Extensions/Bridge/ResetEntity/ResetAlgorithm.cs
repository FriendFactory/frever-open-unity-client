using Bridge.Models.Common;

namespace Extensions.ResetEntity
{
    /// <summary>
    ///     Sets Id to 0 for entity(for single entity, or recursively for all included entities. It depends on algorithm setup)
    /// </summary>
    internal abstract class ResetAlgorithm
    {
        public abstract void Reset(IEntity entity);
    }
}