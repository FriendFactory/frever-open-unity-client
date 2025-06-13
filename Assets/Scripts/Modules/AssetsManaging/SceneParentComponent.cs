using UnityEngine;

namespace Modules.AssetsManaging
{
    public class SceneParentComponent : MonoBehaviour
    {
        public void SetActive(bool value)
        {
            gameObject.SetActive(value);
        }
    }
}
