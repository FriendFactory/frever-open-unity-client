using UnityEngine;

namespace TipsManagment
{
    public interface ITipTargetsProvider
    {
        bool TryGetTarget(TipId id, out ITipTarget target);
        void AddTarget(TipId id, ITipTarget target);
        void RemoveTarget(TipId id);
    }

    public interface ITipTarget
    {
        TipId Id { get; }
        RectTransform TargetTransform { get; }
    }
}