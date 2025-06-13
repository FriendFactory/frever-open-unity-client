using UnityEngine;

namespace UI.UIAnimators
{
    public abstract class BaseGenericUiAnimator<T> : BaseUiAnimator where T : Component
    {
        public T animationTarget;
    }
}
