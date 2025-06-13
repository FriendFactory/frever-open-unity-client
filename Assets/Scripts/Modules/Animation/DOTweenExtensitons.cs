using DG.Tweening;
using TMPro;

namespace Modules.Animation
{
    public static class DOTweenExtensitons
    {
        public static Tweener DOValueAsTextChange(this TMP_Text text, int startValue, int endValue, float duration)
        {
            var currentValue = startValue;

            return DOTween.To(() => currentValue, x => currentValue = x, endValue, duration)
                          .OnUpdate(() => text.text = currentValue.ToString());
        }
    }
}
