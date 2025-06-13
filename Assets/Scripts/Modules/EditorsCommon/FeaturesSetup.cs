using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.EditorsSetting;

namespace Modules.EditorsCommon
{
    public abstract class FeaturesSetup<TFeatureControl, TFeatureSettings, TEditorSettings, TFeatureType> 
        where TFeatureControl: IFeatureControl<TFeatureType>
        where TEditorSettings: IEditorSettings<TFeatureSettings>
        where TFeatureType: Enum
        where TFeatureSettings: IEditorSetting
    {
        private readonly IReadOnlyCollection<TFeatureControl> _featureControls;
        
        protected FeaturesSetup(TFeatureControl[] featureControls)
        {
            _featureControls = featureControls;
        }

        public void Setup(TEditorSettings editorSetup)
        {
            foreach (var featureSettings in editorSetup.Settings)
            {
                var settingsType = featureSettings.GetType();
                var featureControl = GetFeatureControl(settingsType);
                featureControl?.Setup(featureSettings);
            }
        }
        
        public TFeatureControl GetFeatureControl(TFeatureType featureType)
        {
            return _featureControls.FirstOrDefault(x => x.FeatureType.Equals(featureType));
        }

        private TFeatureControl GetFeatureControl(Type settingsType)
        {
            return _featureControls.FirstOrDefault(x => x.SettingsType == settingsType);
        }
    }
}