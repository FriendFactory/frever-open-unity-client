using System;
using System.Collections.Generic;
using System.Linq;
using Bridge;
using UnityEngine;

namespace Modules.WardrobeManaging
{
    [CreateAssetMenu(fileName = "CategoryData", menuName = "Friend Factory/Wardrobe/Category Data", order = 1)]
    public sealed class WardrobeCategoryData: ScriptableObject
    {
        public long CategoryId => GetCategoryData(_environment).CategoryId;
        public long SubCategoryId => GetCategoryData(_environment).SubCategoryId;
        public bool ShowAlone;
        public bool StartCategory;

        [SerializeField] private List<EnvironmentCategoryData> _categoryDatas;

        private static FFEnvironment _environment = FFEnvironment.Production;
        
        public static void SetEnvironment(FFEnvironment environment)
        {
            _environment = environment;
        }
        
        private EnvironmentCategoryData GetCategoryData(FFEnvironment ffEnvironment)
        {
            var output = _categoryDatas.FirstOrDefault(x => x.Environment == ffEnvironment);
            if (output != null) return output;
            
            Debug.Log($"No wardrobe category Id for {ffEnvironment}. Providing Prod data");
            return _categoryDatas.First(x => x.Environment == FFEnvironment.Production);
        }
    }

    [Serializable]
    internal sealed class EnvironmentCategoryData
    {
        public long CategoryId;
        public long SubCategoryId;
        public FFEnvironment Environment;
    }
}