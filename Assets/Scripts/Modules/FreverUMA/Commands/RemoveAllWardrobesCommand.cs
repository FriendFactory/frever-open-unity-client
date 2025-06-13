using Bridge.Models.ClientServer.Assets;
using Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.FreverUMA
{
    public class RemoveAllWardrobesCommand : ActionCommand<IEnumerable<WardrobeFullInfo>, object>
    {
        public RemoveAllWardrobesCommand(IEnumerable<WardrobeFullInfo> startValue, IEnumerable<WardrobeFullInfo> finalValue, Action<IEnumerable<WardrobeFullInfo>, object> action) : base(startValue, finalValue, action)
        {
        }
    }
}