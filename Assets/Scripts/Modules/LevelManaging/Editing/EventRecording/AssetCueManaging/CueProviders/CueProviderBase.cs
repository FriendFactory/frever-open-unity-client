using Extensions;
using Models;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.LevelManaging.Editing.Templates;

namespace Modules.LevelManaging.Editing.EventRecording.AssetCueManaging.CueProviders
{
    internal abstract class CueProviderBase
    {
        private readonly IContext _context;
        private readonly IEventTemplateManager _eventTemplateManager;

        protected CueProviderBase(IContext context, IEventTemplateManager eventTemplateManager)
        {
            _context = context;
            _eventTemplateManager = eventTemplateManager;
        }

        protected Event GetPreviousEvent()
        {
            return _context.CurrentLevel.GetLastEvent();
        }

        protected bool IsMadeFromTemplate(Event target)
        {
            return _eventTemplateManager.LastMadeEvent == target;
        }
    }
}