using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Zenject;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    public class CharacterEditorRotator : MonoBehaviour
    {
        [SerializeField]
        [FormerlySerializedAs("rotateSpeed")]
        private float _rotateSpeed = 0.25f;

        [Inject] private UMACharacterEditorRoom _editorRoom;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void OnTouchDrag(BaseEventData data) 
        {
            if (_editorRoom.Avatar == null) return;
            var pointerEventData = (PointerEventData) data;
            var character = _editorRoom.Avatar.transform;
            var localAngle = character.localEulerAngles;
            localAngle.y -= _rotateSpeed * pointerEventData.delta.x * Time.deltaTime;
            character.localEulerAngles = localAngle;
        }
    }
}