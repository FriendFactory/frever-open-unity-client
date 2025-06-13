using System;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "CharacterManagerConfig.asset", menuName = "Friend Factory/Configs/Character Manager Config", order = 4)]
    public class CharacterManagerConfig : ScriptableObject
    {
        public int MaxCharactersCount = 6;
        public long FemaleId = 2;
        public long NonBinaryId = 3;
        public string DefaultMaleRecipeName = "DefaultMaleCharacter";
        public string DefaultFemaleRecipeName = "DefaultFemaleCharacter";
        public string DefaultNonBinaryRecipeName = "DefaultNonBinaryCharacter";
        public SlotClipping[] SlotsClippingMatrix;

        [Serializable]
        public sealed class SlotClipping
        {
            public string Slot;
            public string[] ClippingSlots;
        }
    }
}