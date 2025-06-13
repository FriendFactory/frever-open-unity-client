using UnityEngine;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    public abstract class CollectionView : MonoBehaviour
    {
        [SerializeField]
        protected GameObject _slotPrefab;
        [SerializeField]
        protected GameObject _layout;
    }
}