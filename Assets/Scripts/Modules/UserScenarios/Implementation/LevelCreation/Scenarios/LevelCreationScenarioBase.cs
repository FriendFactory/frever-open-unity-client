using System.Threading.Tasks;
using Bridge.Models.ClientServer.EditorsSetting;
using Modules.EditorsCommon;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Zenject;

namespace Modules.UserScenarios.Implementation.LevelCreation.Scenarios
{
    internal abstract class LevelCreationScenarioBase<TScenarioArgs> : ScenarioBase<TScenarioArgs, ILevelCreationScenarioContext>
        where TScenarioArgs : IScenarioArgs
    {
        protected readonly IEditorSettingsProvider EditorSettingsProvider;
        protected readonly ILevelCreationStatesSetupProvider StatesSetupProvider;

        protected LevelCreationScenarioBase(DiContainer diContainer, IEditorSettingsProvider editorSettingsProvider, ILevelCreationStatesSetupProvider statesSetupProvider) : base(diContainer)
        {
            EditorSettingsProvider = editorSettingsProvider;
            StatesSetupProvider = statesSetupProvider;
        }

        protected sealed override async Task<ILevelCreationScenarioContext> SetupContext()
        {
            var settings = await GetSettings();
            var context = new LevelCreationScenarioContext();
            context.PostRecordEditor.PostRecordEditorSettings = settings.PostRecordEditorSettings;
            context.LevelEditor.LevelEditorSettings = settings.LevelEditorSettings;
            context.CharacterEditor.CharacterEditorSettings = settings.CharacterEditorSettings;
            return context;
        }

        protected virtual Task<EditorsSettings> GetSettings()
        {
            return EditorSettingsProvider.GetDefaultEditorSettings();
        }
    }
}