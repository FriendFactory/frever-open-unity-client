using System;
using Bridge.Models.Common;

namespace Extensions
{
    internal interface IReplaceAlgorithm<T>
    {
        void ReplaceId(IEntity entity, Func<long, bool> condition, Func<string, long> generator);
    }
}