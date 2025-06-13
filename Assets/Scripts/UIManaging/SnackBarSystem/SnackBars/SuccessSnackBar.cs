using System.Collections;
using UIManaging.SnackBarSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.SnackBarSystem.SnackBars
{
    internal sealed class SuccessSnackBar : SnackBar<SuccessSnackBarConfiguration>
    {
        [SerializeField] private RectTransform _body;
        
        public override SnackBarType Type => SnackBarType.Success;

        protected override void OnConfigure(SuccessSnackBarConfiguration configuration)
        {
            StartCoroutine(RebuildLayout());
        }

        private IEnumerator RebuildLayout()
        {
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_body);
        }
    }
}
