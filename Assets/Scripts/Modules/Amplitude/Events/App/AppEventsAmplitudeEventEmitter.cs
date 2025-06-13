using System;
using Common.ApplicationCore;
using JetBrains.Annotations;
using Modules.Amplitude.Events.AppEvents;
using Modules.Amplitude.Signals;
using UnityEngine;
using Zenject;

namespace Modules.Amplitude.Events.App
{
    [UsedImplicitly]
    public sealed class AppEventsAmplitudeEventSignalEmitter: BaseAmplitudeEventSignalEmitter
    {
        private readonly IAppEventsSource _appEventsSource;

        public AppEventsAmplitudeEventSignalEmitter(IAppEventsSource appEventsSource, SignalBus signalBus) : base(signalBus)
        {
            _appEventsSource = appEventsSource;
        }

        public override void Initialize()
        {
            _appEventsSource.ApplicationFocused += OnApplicationFocused;
            _appEventsSource.ApplicationQuit += OnApplicationQuit;
        }

        public override void Dispose()
        {
            _appEventsSource.ApplicationFocused -= OnApplicationFocused;
            _appEventsSource.ApplicationQuit -= OnApplicationQuit;
        }

        private void OnApplicationQuit() => Emit(new AppQuitAmplitudeEvent());
        private void OnApplicationFocused(bool hasFocus) => Emit(new AppFocusedAmplitudeEvent(hasFocus, Application.internetReachability));
    }
}