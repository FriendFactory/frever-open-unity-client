using System;

namespace UIManaging.Pages.Feed.Core.Metrics
{
    public class ToggleMetricsModel
    {
        public event Action OnMetricsValueChangedEvent;
        public event Action OnSetIsOnEvent;

        public long EntityId { get; private set; }
        public long GroupId { get; private set; }
        public long MetricsValue { get; private set; }

        public bool IsOn { get; private set; }

        public ToggleMetricsModel(long entityId, long groupId, long metricsValue, bool isOn = false)
        {
            EntityId = entityId;
            GroupId = groupId;
            MetricsValue = metricsValue;
            IsOn = isOn;
        }

        public void SetIsOn(bool value)
        {
            IsOn = value;
            OnSetIsOnEvent?.Invoke();
        }
        
        public void SetIsOnSilent(bool value)
        {
            IsOn = value;
        }

        public void Add()
        {
            MetricsValue++;
            OnMetricsValueChangedEvent?.Invoke();
        }

        public void Subtract()
        {
            MetricsValue--;
            OnMetricsValueChangedEvent?.Invoke();
        }
    }
}