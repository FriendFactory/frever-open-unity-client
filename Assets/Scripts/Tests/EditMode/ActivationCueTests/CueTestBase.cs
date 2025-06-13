using Extensions;
using Models;
using Modules.LevelManaging.Editing.LevelManagement;
using Moq;

namespace Tests.EditMode.ActivationCueTests
{
    internal abstract class CueTestBase
    {
        protected IContext GetContextMockWithSetupLevel(Event addEvent = null)
        {
            var level = new Level();
            if(addEvent != null) level.AddEvent(addEvent);
            var contextMock = new Mock<IContext>();
            contextMock.Setup(x => x.CurrentLevel).Returns(level);
            return contextMock.Object;
        }
    }
}