using Bridge.Models.Common;

namespace Extensions.ResetEntity
{
    internal sealed class SimplestResetMainIdAlgorithm: ResetAlgorithm
    {
        public override void Reset(IEntity entity)
        {
            entity.Id = 0;
        }
    }
}