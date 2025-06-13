using System;
using Bridge.Models.ClientServer.EditorsSetting;

namespace Modules.EditorsCommon
{
    public interface IFeatureControl
    {
        bool IsFeatureEnabled { get; }
        void Setup(IEditorSetting settings);
    }

    public interface IFeatureControl<out TFeatureType>: IFeatureControl 
        where TFeatureType: Enum
    {
        Type SettingsType { get; }
        
        TFeatureType FeatureType { get; }
    }
    
    public abstract class FeatureControlBase<TSettings, TFeatureType> : IFeatureControl<TFeatureType>
        where TFeatureType : Enum
        where TSettings: class, IEditorSetting
    {
        protected TSettings Settings;

        public abstract bool IsFeatureEnabled { get; }
        public Type SettingsType => typeof(TSettings);
        public abstract TFeatureType FeatureType { get; }

        public void Setup(IEditorSetting settings)
        {
            Settings = settings as TSettings ?? throw new InvalidOperationException($"Wrong settings type {settings.GetType().Name} for {this.GetType().Name}");
        }
    }
}