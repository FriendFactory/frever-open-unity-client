using System;
using System.Collections.Generic;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "DefaultSubCategoryColors.asset", menuName = "Friend Factory/Configs/Defaul sub category colors", order = 4)]
    public class DefaultSubCategoryColors : ScriptableObject
    {
        [Serializable]
        private class DefaultColor
        {
            public string SharedColor;
            public int Index;
        }

        [SerializeField]
        private List<DefaultColor> DefaultColors = new List<DefaultColor>();

        public int GetDefaultColorIndex(string sharedColor)
        {
            var founded = DefaultColors.Find(x => x.SharedColor == sharedColor);
            if (founded == null) return -1;
            return founded.Index;
        }
    }
}