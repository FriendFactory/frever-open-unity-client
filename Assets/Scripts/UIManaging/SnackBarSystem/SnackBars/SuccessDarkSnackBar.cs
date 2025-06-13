using System.Collections;
using UIManaging.SnackBarSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.SnackBarSystem.SnackBars
{
    internal sealed class SuccessDarkSnackBar : SnackBar<SuccessDarkSnackBarConfiguration>
    {
        [SerializeField] private RectTransform _body;
        
        public override SnackBarType Type => SnackBarType.SuccessDark;

        protected override void OnConfigure(SuccessDarkSnackBarConfiguration configuration)
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