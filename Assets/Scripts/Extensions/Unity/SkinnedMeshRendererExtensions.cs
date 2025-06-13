using UnityEngine;

namespace Extensions
{
    public static class SkinnedMeshRendererExtensions
    {
        public static void RecalculateBounds(this SkinnedMeshRenderer smr)
        {
            //workaround how to trigger bounds recalculation
            var initialState = smr.enabled;
            smr.enabled = !initialState;
            smr.enabled = initialState;
        }
    }
}