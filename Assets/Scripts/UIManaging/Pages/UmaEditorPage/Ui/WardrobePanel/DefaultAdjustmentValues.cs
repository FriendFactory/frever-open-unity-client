using System.Collections.Generic;
using System;
using UnityEngine;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    [CreateAssetMenu(fileName = "DefaultAdjustmentValues.asset", menuName = "Friend Factory/Configs/Default adjustment values", order = 4)]
    public class DefaultAdjustmentValues : ScriptableObject
    {
        public List<DefaultDNAValue> DefaultValues = new List<DefaultDNAValue>();
    }

    [Serializable]
    public class DefaultDNAValue
    {
        public string Name;
        public byte Value;
    }
}