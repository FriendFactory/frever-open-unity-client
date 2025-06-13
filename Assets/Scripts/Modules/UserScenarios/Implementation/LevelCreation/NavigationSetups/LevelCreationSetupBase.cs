using System;
using System.Linq;
using System.Threading.Tasks;
using Modules.AssetsStoraging.Core;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Modules.UserScenarios.Implementation.LevelCreation.Transitions;
using UIManaging.PopupSystem;
using Zenject;
using static System.StringComparison;

namespace Modules.UserScenarios.Implementation.LevelCreation.NavigationSetups
{
    internal enum LevelCreationSetup
    {
        DefaultLevelCreation,
        LevelEditorToPip,
        PostRecordOnly,
        PostRecordWithOutfitCreation,
        PostRecordWithOutfitAndNewEventCreation,
        PostRecordWithNewEventCreation,
        RemixSetup,
        RemixSocialAction,
        CharacterDressingToPostRecord,
        EditLocallySavedLevel,
        TemplateActionSetup,
    }

    internal abstract class LevelCreationSetupBase : StatesSetupBase
    {
        private IMetadataProvider _metadataProvider;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public abstract LevelCreationSetup Type { get; }

        private IMetadataProvider MetadataProvider
        {
            get { return _metadataProvider ??= ResolveService<IMetadataProvider>(); }
        }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        protected LevelCreationSetupBase(DiContainer diContainer) : base(diContainer)
        {
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected ITransition GetLevelEditorToNonLevelVideoUploadDefaultTransition()
        {
            var transition = new EmptyTransition(ScenarioState.PublishFromGallery);
            transition.SetOnRunningBehaviour(() =>
            {
                var levelManager = ResolveService<ILevelManager>();
                levelManager.UnloadAllAssets();
                levelManager.CleanUp();
                return Task.CompletedTask;
            });
            return transition;
        }

        protected ExitScenarioFromLevelEditor ResolveExitFromLevelEditor(ScenarioState exitDestination)
        {
            return new ExitScenarioFromLevelEditor(exitDestination, ResolveService<ILevelManager>(),
                                                   ResolveService<PopupManager>());
        }
        
        protected ExitScenarioFromPostRecordEditor ResolveExitFromPostRecordEditor(ScenarioState exitDestination)
        {
            return new ExitScenarioFromPostRecordEditor(exitDestination, ResolveService<ILevelManager>(),
                                                        ResolveService<PopupManager>());
        }

        protected bool IsRatingFeedEnabled()
        {
            const string featureName = "RatingFeed";
            const string featureValue = "Enabled";

            var featureFlags = MetadataProvider.MetadataStartPack.FeatureFlags;

            try
            {
                var ratingFeedFlag = featureFlags.First(flag => flag.Name.Equals(featureName, OrdinalIgnoreCase));
                var x = ratingFeedFlag.Value.Equals(featureValue, OrdinalIgnoreCase);
                return x;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException or InvalidOperationException) return false;
                throw;
            }
        }
    }
}