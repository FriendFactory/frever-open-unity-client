using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal sealed class CrewManagementTab : MonoBehaviour
{
    [SerializeField] private Toggle _toggle;
    
    [SerializeField] private Image _image;
    [SerializeField] private TMP_Text _label;

    [Space]
    [SerializeField] private Color _activeColor;
    [SerializeField] private Color _disabledColor;

    private void OnEnable()
    {
        _toggle.onValueChanged.AddListener(OnToggleValueChanged);
        OnToggleValueChanged(_toggle.isOn);
    }

    private void OnToggleValueChanged(bool isOn)
    {
        var color = isOn ? _activeColor : _disabledColor;

        _image.color = color;
        _label.color = color;
    }
}
