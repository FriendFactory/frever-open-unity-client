using Bridge.Models.ClientServer.Assets;
using Common;
using System;
using System.Collections.Generic;

namespace Modules.FreverUMA
{
    public class OutfitApplyCommand : ActionCommand<OutfitShortInfo, IEnumerable<WardrobeFullInfo>>
    {
        public readonly IEnumerable<WardrobeFullInfo> StartWardrobes;

        public OutfitApplyCommand(OutfitShortInfo startValue, OutfitShortInfo finalValue, Action<OutfitShortInfo, IEnumerable<WardrobeFullInfo>> action, IEnumerable<WardrobeFullInfo> startWardrobes) : base(startValue, finalValue, action)
        {
            StartWardrobes = startWardrobes;
        }

        protected override void InvokeAction(OutfitShortInfo value)
        {
            Action?.Invoke(value, StartWardrobes);
        }
    }
}