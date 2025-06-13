using TMPro;
using UnityEngine;

internal sealed class NavBarNotificationBubble : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    public void Show(int value)
    {
        gameObject.SetActive(true);
        _text.text = value < 0 ? string.Empty : value > 99 ? "+99" : value.ToString();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
