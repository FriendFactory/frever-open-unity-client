using System.Linq;
using Extensions;
using JetBrains.Annotations;
using Modules.Amplitude.Events.Core;
using Modules.Amplitude.Signals;
using Modules.AssetsStoraging.Core;
using Modules.PageLoadTracking;
using Modules.UniverseManaging;
using Navigation.Core;
using Zenject;

namespace Modules.Amplitude.Events.PageChange
{
    [UsedImplicitly]
    public sealed class PageChangeAmplitudeEventSignalEmitter: BaseAmplitudeEventSignalEmitter
    {
        private readonly IPageLoadTimeTracker[] _pageLoadTimeTrackers;
        private readonly PageManager _pageManager;
        private readonly IMetadataProvider _metadataProvider;
        private readonly IUniverseManager _universeManager;

        public PageChangeAmplitudeEventSignalEmitter(IPageLoadTimeTracker[] pageLoadTimeTrackers, SignalBus signalBus, PageManager pageManager, IMetadataProvider metadataProvider, IUniverseManager universeManager) : base(signalBus)
        {
            _pageLoadTimeTrackers = pageLoadTimeTrackers;
            _pageManager = pageManager;
            _metadataProvider = metadataProvider;
            _universeManager = universeManager;
        }

        public override void Initialize()
        {
            _pageManager.PageSwitchingBegan += OnPageSwitchingBegan;

            _pageLoadTimeTrackers.ForEach(loadTimeTracker => loadTimeTracker.TrackingEnded += OnTrackingEnded);
        }

        public override void Dispose()
        {
            _pageManager.PageSwitchingBegan -= OnPageSwitchingBegan;
            
            _pageLoadTimeTrackers.ForEach(loadTimeTracker => loadTimeTracker.TrackingEnded -= OnTrackingEnded);
        } 
        
        private void OnPageSwitchingBegan(PageId? pageId, PageData nextPageData)
        {
            var tracker = nextPageData.PageId switch
            {
                PageId.LevelEditor => _pageLoadTimeTrackers.First(x => x.Type == LoadTimeTrackerType.LevelEditor),
                PageId.UmaEditor => _pageLoadTimeTrackers.First(x => x.Type == LoadTimeTrackerType.UmaEditor),
                _ => _pageLoadTimeTrackers.First(x => x.Type == LoadTimeTrackerType.Default)
            };

            tracker.BeginTracking(nextPageData);
        }

        private void OnTrackingEnded(PageArgs pageArgs, long elapsedTime)
        {
            Emit(GetAmplitudeEvent());

            IAmplitudeEvent GetAmplitudeEvent()
            {
                var targetPage = GetTargetPage(pageArgs.TargetPage);
                var amplitudeEvent = new PageChangeAmplitudeEvent(targetPage, elapsedTime);

                return targetPage switch
                {
                    PageId.UserProfile => new ProfilePageChangeAmplitudeEventDecorator(amplitudeEvent, pageArgs),
                    PageId.Feed or PageId.GamifiedFeed => new FeedPageChangeAmplitudeEventDecorator(amplitudeEvent, pageArgs),
                    PageId.UmaEditor => new UmaEditorPageChangeAmplitudeEventDecorator(amplitudeEvent, pageArgs),
                    PageId.GenderSelection => new SelectedUniverseAmplitudeEventDecorator(amplitudeEvent, pageArgs, _metadataProvider),
                    PageId.LevelEditor or PageId.VideoMessage => _universeManager.LastSelectedUniverse != null
                        ? new SelectedUniverseAmplitudeEventDecorator(amplitudeEvent, _universeManager)
                        : new SelectedUniverseAmplitudeEventDecorator(amplitudeEvent, pageArgs, _metadataProvider),
                    _ => amplitudeEvent
                };
            }
            
            PageId GetTargetPage(PageId targetPage)
            {
                return targetPage switch
                {
                    PageId.UmaEditorNew => PageId.UmaEditor,
                    PageId.CharacterStyleSelection => PageId.GenderSelection,
                    _ => targetPage
                };
            }
        }
    }
}