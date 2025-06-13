using UnityEngine;

namespace UIManaging.Common.InputFields
{
    [RequireComponent(typeof(IgnoredDeselectableAreaAdvancedInputField))]
    internal sealed class IgnoredDeselectableAreasHandler: MonoBehaviour
    {
        [SerializeField] private RectTransform[] _ignoredRects;
        
        private IgnoredDeselectableAreaAdvancedInputField InputField =>
            _inputField ? _inputField : _inputField = GetComponent<IgnoredDeselectableAreaAdvancedInputField>();

        private IgnoredDeselectableAreaAdvancedInputField _inputField;

        private void OnEnable()
        {
            InputField.AddIgnoreDeselectOnRect(_ignoredRects);
        }
        
        private void OnDisable()
        {
            InputField.RemoveIgnoreDeselectOnRect(_ignoredRects);
        }
    }
}