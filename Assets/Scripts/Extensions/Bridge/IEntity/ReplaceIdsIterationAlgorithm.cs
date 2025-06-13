using System;
using Bridge.Models.Common;
using Models;

namespace Extensions
{
    internal class ReplaceIdsIterationAlgorithm : IReplaceAlgorithm<Level>
    {
        public void ReplaceId(IEntity entity, Func<long, bool> condition, Func<string, long> generator)
        {
            if (entity is Level level)
            {
                level.ReplaceIds(condition, generator);
            }
            else
            {
                throw new InvalidOperationException($"Current entity type {entity.GetType().Name} is not target type {nameof(Level)}");
            }
        }
    }
}
