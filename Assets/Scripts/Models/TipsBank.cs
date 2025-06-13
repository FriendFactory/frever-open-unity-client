using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Models
{
    [CreateAssetMenu(fileName = "TipsBank.asset", menuName = "Friend Factory/Tips Bank", order = 1)]
    public class TipsBank : ScriptableObject
    {
        [SerializeField] private List<string> _tips;

        private int _lastIndex;

        public string GetRandomTip()
        {
            var index = Roll();

            return _tips[index];
        }

        private int Roll()
        {
            var index = Random.Range(0, _tips.Count);
            while (index == _lastIndex)
            {
                index = Random.Range(0, _tips.Count);
            }

            _lastIndex = index;
            return index;
        }
    }
}