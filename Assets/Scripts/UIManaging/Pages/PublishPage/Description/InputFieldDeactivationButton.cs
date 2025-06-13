using UIManaging.Common.InputFields;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIManaging.Pages.SharingPage.Ui
{
    [RequireComponent(typeof(Button))]
    internal sealed class InputFieldDeactivationButton: MonoBehaviour
    {
        [SerializeField] private DescriptionPanel _descriptionPanel;
        private IInputFieldAdapter InputFieldAdapter => _descriptionPanel.InputFieldAdapter;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(DeactivateInputField);
        }

        private void DeactivateInputField()
        {
            InputFieldAdapter.DeactivateInputField();

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }
}