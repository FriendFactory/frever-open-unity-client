using TMPro;
using UnityEngine;

namespace UIManaging.Pages.Feed.Remix.Collection
{
    public class CharacterSelectionCategoryHeader: MonoBehaviour
    {
        [SerializeField] private TMP_Text _header;

        public void SetHeader(string header)
        {
            _header.text = header;
        }
    }
}