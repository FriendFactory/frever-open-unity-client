using UIManaging.SnackBarSystem;
using UIManaging.SnackBarSystem.Configurations;
using UIManaging.SnackBarSystem.SnackBars;
using UnityEngine;
using UnityEngine.UI;

internal sealed class PurchaseFailedSnackBar : SnackBar<PurchaseFailedSnackBarConfiguration>
{
    [SerializeField] Button _okButton;
    public override SnackBarType Type => SnackBarType.PurchaseFailed;
    
    private void Awake()
    {
        _okButton.onClick.AddListener(RequestHide);
    }
    
    private void OnDestroy()
    {
        _okButton.onClick.RemoveListener(RequestHide);
    }
    
    protected override void OnConfigure(PurchaseFailedSnackBarConfiguration configuration)
    {
        _okButton.onClick.AddListener(RequestHide);
    }

    protected override void OnSwipeUp() { }
}
