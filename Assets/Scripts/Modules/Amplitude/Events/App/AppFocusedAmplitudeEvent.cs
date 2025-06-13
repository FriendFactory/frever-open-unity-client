using Modules.Amplitude.Events.Core;
using UnityEngine;

namespace Modules.Amplitude.Events.AppEvents
{
    public sealed class AppFocusedAmplitudeEvent: BaseAmplitudeEvent
    {
        public override string Name => AmplitudeEventConstants.EventNames.APP_FOCUSED;

        public AppFocusedAmplitudeEvent(bool hasFocus, NetworkReachability networkReachability)
        {
            _eventProperties.Add(AmplitudeEventConstants.EventProperties.APP_FOCUSED, hasFocus.ToString());
            _eventProperties.Add(AmplitudeEventConstants.EventProperties.INTERNET_CONNECTION_TYPE, GetInternetConnectivityType(networkReachability));
        }

        private string GetInternetConnectivityType(NetworkReachability networkReachability)
        {
            var connectionType = networkReachability switch
            {
                NetworkReachability.NotReachable => "None",
                NetworkReachability.ReachableViaCarrierDataNetwork => "Mobile",
                NetworkReachability.ReachableViaLocalAreaNetwork => "LAN",
                _ => "Unknown"
            };

            return connectionType;
        }
    }
}