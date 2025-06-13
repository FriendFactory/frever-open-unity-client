using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankBadgeItemView : MonoBehaviour
{
    [SerializeField] 
    private TMP_Text _unlockValueText;
    [SerializeField] 
    private GameObject _unlockTextPanel;
    [SerializeField] 
    private Image _badgeImage;

    // Start is called before the first frame update
    public void Initialize(bool isUnlocked, int expToUnlock, Sprite rankBadgeSprite)
    {
        _unlockTextPanel.gameObject.SetActive(!isUnlocked);
        _unlockValueText.text = expToUnlock.ToString();
        _badgeImage.sprite = rankBadgeSprite;
        gameObject.SetActive(true);
    }
}
