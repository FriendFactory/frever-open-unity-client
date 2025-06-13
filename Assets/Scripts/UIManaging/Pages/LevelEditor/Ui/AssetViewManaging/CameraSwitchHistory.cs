using System.Collections.Generic;

namespace UIManaging.Pages.LevelEditor.Ui.SushiBarComponents
{
    internal class CameraSwitchHistory : ICameraSwitchingHistory
    {
        private readonly Dictionary<long, long> _setLocationIdToCameraId = new Dictionary<long, long>();

        public void Save(long setLocationId, long cameraIndex)
        {
            if (!_setLocationIdToCameraId.ContainsKey(setLocationId))
            {
                _setLocationIdToCameraId.Add(setLocationId,0);
            }

            _setLocationIdToCameraId[setLocationId] = cameraIndex;
        }

        public long GetCamera(long setLocationIndex)
        {
            if (_setLocationIdToCameraId.ContainsKey(setLocationIndex))
            {
                return _setLocationIdToCameraId[setLocationIndex];
            }
            
            return 0;
        }
    }
}
