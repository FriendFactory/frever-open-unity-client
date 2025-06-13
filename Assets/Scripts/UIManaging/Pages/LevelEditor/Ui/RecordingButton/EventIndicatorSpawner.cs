using System.Collections.Generic;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.RecordingButton
{
    internal sealed class EventIndicatorSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _eventIndicatorPrefab;

        private readonly Stack<GameObject> _evenIndicatorPool = new Stack<GameObject>();
        private readonly Dictionary<long, GameObject> _activeEventIndicators = new Dictionary<long, GameObject>();
    
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void PlaceIndicator(float levelDurationProgress, long eventSequence)
        {
            var indicator = GetIndicator();
            _activeEventIndicators.Add(eventSequence, indicator);
            var indicatorRect = indicator.GetComponent<RectTransform>();
            var indicatorAngle = levelDurationProgress * 360f;
            indicatorRect.eulerAngles = new Vector3(0, 0, -indicatorAngle);
        }
    
        public void RemoveIndicator(long eventSequence)
        {
            var indicator = _activeEventIndicators[eventSequence];
            DisposeIndicator(indicator);
            _activeEventIndicators.Remove(eventSequence);
        }
        
        public void DisposeActiveIndicators()
        {
            foreach (var eventIndicator in _activeEventIndicators.Values)
            {
                DisposeIndicator(eventIndicator);
            }
            _activeEventIndicators.Clear();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void DisposeIndicator(GameObject indicator)
        {
            if (indicator == null) return;

            indicator.SetActive(false);
            _evenIndicatorPool.Push(indicator);
        }
        
        private GameObject GetIndicator()
        {
            var indicator = _evenIndicatorPool.Count != 0 ? _evenIndicatorPool.Pop() : CreateIndicator();
            indicator.SetActive(true);
            return indicator;
        }

        private GameObject CreateIndicator()
        {
            return Instantiate(_eventIndicatorPrefab, transform);
        }
    }
}
