using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIManaging.Common.InputFields
{
    public class UndeselectableInputField: TMP_InputField
    {
        public bool IsKeyboardVisible => m_SoftKeyboard?.status == TouchScreenKeyboard.Status.Visible;

        public override void OnDeselect(BaseEventData eventData)
        {
            // Intentionally left blank to disable field deselection
        }
    }
}