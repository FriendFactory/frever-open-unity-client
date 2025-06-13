using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankDescriptionItem : MonoBehaviour
{
    [SerializeField] private Image _badgeImage;
    [SerializeField] private TMP_Text _header;
    [SerializeField] private TMP_Text _description;

    [SerializeField] private LocalizedString _descriptionLocalizedString;
    
    public void Init(Sprite badge, string name, int unlockScore)
    {
        _badgeImage.sprite = badge;
        _header.text = name;
        _description.text = string.Format(_descriptionLocalizedString, unlockScore);
    }
}
