using UnityEngine;
using UnityEngine.UI;

namespace TipsManagment
{
    internal sealed class ToggleTipTarget : TipTarget
    {
        [SerializeField] private Toggle _toggle;

        public Toggle Toggle => _toggle;
    }
}