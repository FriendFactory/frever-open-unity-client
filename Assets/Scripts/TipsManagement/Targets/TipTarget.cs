using UnityEngine;
using Zenject;

namespace TipsManagment
{
    [RequireComponent(typeof(RectTransform))]
    internal class TipTarget: MonoBehaviour, ITipTarget
    {
        [SerializeField] private TipId _tipId;
        
        [Inject] private ITipTargetsProvider _tipTargetsProvider;

        public TipId Id => _tipId;
        public RectTransform TargetTransform => transform as RectTransform;

        private void OnEnable()
        {
            _tipTargetsProvider.AddTarget(_tipId, this);
        }

        private void OnDisable()
        {
            _tipTargetsProvider.RemoveTarget(_tipId);
        }
    }
}