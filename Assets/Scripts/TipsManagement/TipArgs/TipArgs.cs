using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TipsManagment.Args
{
    public class TipArgs
    {
        public TipId Id;
        public TipType Type;
        public string Text;
        public string TargetElementName;
        public int ItemIndex;
        public int TriggerWaitTime;
        public int Duration;
        public Vector2 Offset;
        public RelativePosition ForcePosition;
        public Transform PageTransform;
        public int PromptAgain;
        public ITipTarget Target;

        public TipArgs(TipPreset preset, Transform pageTransform, TipType type, ITipTarget target = null)
        {
            Id = preset.Settings.Id;
            Type = type;
            Text = preset.Settings.HintText;
            Duration = preset.Settings.Duration;
            TargetElementName = preset.TargetElementName;
            ItemIndex = preset.ItemIndex;
            TriggerWaitTime = preset.Settings.TriggerWaitTime;
            PageTransform = pageTransform;
            Offset = preset.Position;
            ForcePosition = preset.ForcePosition;
            PromptAgain = preset.Settings.PromptAgain;
            Target = target;
        }
    }

    public enum TipType
    {
        Page = 0,
        Direct = 1,
        PageParallel = 2,
    }
}