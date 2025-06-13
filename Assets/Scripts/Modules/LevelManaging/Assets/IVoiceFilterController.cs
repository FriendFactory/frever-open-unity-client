using System;
using Bridge.Models.ClientServer.Assets;
using Extensions;

namespace Modules.LevelManaging.Assets
{
    public interface IVoiceFilterController
    {
        VoiceFilterFullInfo Current { get; }
        long NoVoiceFilterId { get;}
        VoiceFilterFullInfo NoVoiceFilter { get; }
        event Action<DbModelType> VoiceFilterChanged;
        event Action<VoiceFilterFullInfo> VoiceFilterChangedSilently;
        void Change(VoiceFilterFullInfo data, Action onSuccess, Action<string> onFail);
        void ChangeSilently(VoiceFilterFullInfo data, Action onSuccess, Action<string> onFail);
    }
}
