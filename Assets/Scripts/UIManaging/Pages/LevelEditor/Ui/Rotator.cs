using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui
{
    public class Rotator : MonoBehaviour
    {
        [SerializeField] float _rotationSpeed = 200;

        private void Update()
        {
            transform.Rotate(new Vector3(0f, 0f, Time.deltaTime * _rotationSpeed*-1), Space.Self);
        }
    }
}
