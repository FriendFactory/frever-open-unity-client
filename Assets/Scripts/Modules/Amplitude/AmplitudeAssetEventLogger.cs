using System.Collections.Generic;
using Modules.Amplitude;
using Zenject;

namespace Modules.Amplitude
{
    public sealed class AmplitudeAssetEventLogger 
    {
        [Inject] private AmplitudeManager _amplitudeManager;

        public void LogSelectedVfxAmplitudeEvent(long id)
        {
            var vfxMetaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.VFX_ID] = id
            };
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.VFX_SELECTED, vfxMetaData);
        }
        
        public void LogSelectedCameraFilterAmplitudeEvent(long id)
        {
            var cameraFilterMetaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.CAMERA_FILTER_ID] = id
            };
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.CAMERA_FILTER_SELECTED, cameraFilterMetaData);
        }
        
        public void LogSelectedCameraFilterVariantAmplitudeEvent(long id)
        {
            var cameraFilterVariantMetaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.CAMERA_FILTER_VARIANT_ID] = id
            };
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.CAMERA_FILTER_VARIANT_SELECTED, cameraFilterVariantMetaData);
        }

        public void LogSelectedVoiceFilterAmplitudeEvent(long id)
        {
            var voiceFilterMetaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.VOICE_FILTER_ID] = id
            };
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.VOICE_FILTER_SELECTED, voiceFilterMetaData);
        }
        
        public void LogSelectedBodyAnimationAmplitudeEvent(long id, float loadTime)
        {
            var bodyAnimationMetaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.BODY_ANIMATION_ID] = id,
                [AmplitudeEventConstants.EventProperties.LOADING_TIME] = loadTime
            };
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.BODY_ANIMATION_SELECTED, bodyAnimationMetaData);
        }
        
        public void LogSelectedSetLocationAmplitudeEvent(long id, float loadTime)
        {
            var setLocationMetaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.SET_LOCATION_ID] = id,
                [AmplitudeEventConstants.EventProperties.LOADING_TIME] = loadTime
            };
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.SET_LOCATION_SELECTED, setLocationMetaData);
        }
        
        public void LogSelectedSpawnPositionAmplitudeEvent(long id)
        {
            var spawnPositionMetaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.SPAWN_POSITION_ID] = id
            };
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.SPAWN_POSITION_SELECTED, spawnPositionMetaData);
        }
        
        public void LogSelectedCameraTemplateAmplitudeEvent(long id, float loadTime)
        {
            var cameraTemplateMetaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.CAMERA_TEMPLATE_ID] = id,
                [AmplitudeEventConstants.EventProperties.LOADING_TIME] = loadTime
            };
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.CAMERA_TEMPLATE_SELECTED, cameraTemplateMetaData);
        }
        
        public void LogAddedCharacterAmplitudeEvent(long id, float loadTime, bool baked)
        {
            var characterMetaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.CHARACTER_ID] = id,
                [AmplitudeEventConstants.EventProperties.LOADING_TIME] = loadTime,
                [AmplitudeEventConstants.EventProperties.BAKED] = baked,
            };
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.CHARACTER_ADDED, characterMetaData);
        }
        
        public void LogRemovedCharacterAmplitudeEvent(long id)
        {
            var characterMetaData = new Dictionary<string, object> {[AmplitudeEventConstants.EventProperties.CHARACTER_ID] = id};
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.CHARACTER_REMOVED, characterMetaData);
        }
        
        public void LogSelectedOutfitAmplitudeEvent(long id, float loadTime)
        {
            var characterMetaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.OUTFIT_ID] = id,
                [AmplitudeEventConstants.EventProperties.LOADING_TIME] = loadTime
            };
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.OUTFIT_SELECTED, characterMetaData);
        }
    }
}
