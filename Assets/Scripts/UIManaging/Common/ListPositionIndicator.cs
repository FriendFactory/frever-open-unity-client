using System.Collections.Generic;
using UnityEngine;

namespace UIManaging.Common
{
    public sealed class ListPositionIndicator : MonoBehaviour
    {
        [SerializeField] private ListPositionIndicatorElement _indicatorPrefab;
        [SerializeField] private Transform _indicatorsParent;

        private List<ListPositionIndicatorElement> _indicators = new List<ListPositionIndicatorElement>();

        public void Initialilze(int numberOfElements)
        {
            InstantiateIndicators(numberOfElements);
            Refresh(0);
        }

        public void CleanUp()
        {
            _indicators.ForEach(i => Destroy(i.gameObject));
            _indicators.Clear();
        }

        public void Refresh(int currentIndex)
        {
            _indicators.ForEach(i => i.Refresh(currentIndex));
        }

        private void InstantiateIndicators(int amount)
        {
            for (var i = 0; i < amount; i++)
            {
                var indicator = Instantiate(_indicatorPrefab, _indicatorsParent, false);
                indicator.Initialize(i);
                _indicators.Add(indicator);
            }
        }
        
        private void OnDisable()
        {
            CleanUp();
        }
    }
}