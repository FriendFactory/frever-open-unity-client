using Extensions;
using Models;
using Modules.CameraSystem.CameraAnimations;
using Event = Models.Event;

namespace Modules.LevelManaging.Editing.CameraManaging
{
    /// <summary>
    ///     Provides last camera animation frame of previous event.
    ///     Necessary for forcing camera to last position of previous event for recording next event,
    ///     unless user changed camera angle manually. It allows to have smooth camera animation, because guarantees
    ///     same camera position between events
    /// </summary>
    internal sealed class PreviousEventLastCameraFrameProvider
    {
        private readonly CameraAnimationFrameProvider _cameraAnimationFrameProvider;

        private CameraAnimationFrame _cachedFrame;
        private long? _cachedCameraAnimId;
        private string _cachedCameraAnimVersion;

        internal PreviousEventLastCameraFrameProvider(CameraAnimationFrameProvider cameraAnimationFrameProvider)
        {
            _cameraAnimationFrameProvider = cameraAnimationFrameProvider;
        }

        public CameraAnimationFrame GetPreviousEventCameraAnimationLastFrame(Level currentLevel, Event targetEvent)
        {
            var previousEvent = currentLevel.GetEventBefore(targetEvent);
            if (previousEvent == null) return null;

            var previousCameraAnimation = previousEvent.GetCameraAnimation();

            if (HasCached(previousCameraAnimation.Id, previousCameraAnimation.GetVersion()))
                return _cachedFrame;

            _cachedFrame = _cameraAnimationFrameProvider.GetLastFrame(previousCameraAnimation);
            _cachedCameraAnimId = previousCameraAnimation.Id;
            _cachedCameraAnimVersion = previousCameraAnimation.GetVersion();
            return _cachedFrame;
        }

        public void ClearCache()
        {
            _cachedFrame = null;
            _cachedCameraAnimId = null;
        }

        private bool HasCached(long cameraAnimId, string version)
        {
            return _cachedCameraAnimId == cameraAnimId && _cachedCameraAnimVersion == version;
        }
    }
}
