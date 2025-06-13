using Bridge.Models.ClientServer.Assets;
using Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.FreverUMA
{
    public class ResetCharacterCommand : ActionCommand<KeyValuePair<string, WardrobeFullInfo[]>, object>
    {
        public ResetCharacterCommand(KeyValuePair<string, WardrobeFullInfo[]> startValue, KeyValuePair<string, WardrobeFullInfo[]> finalValue, 
                                        Action<KeyValuePair<string, WardrobeFullInfo[]>, object> action) : base(startValue, finalValue, action)
        {
        }
    }
}