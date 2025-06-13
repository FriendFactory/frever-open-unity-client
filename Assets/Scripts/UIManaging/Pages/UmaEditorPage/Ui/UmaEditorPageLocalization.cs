using I2.Loc;
using UnityEngine;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    public class UmaEditorPageLocalization : MonoBehaviour
    {
        public LocalizedString ResetPopupTitle = "Options";
        public LocalizedString ResetPopupDesignNewLookOption = "Design a whole new look";
        public LocalizedString ResetPopupGoBackToLastSavedOption = "Go back to last saved outfit";
        [Space]
        public LocalizedString StartOverPopupOkButton = "Ok";
        public LocalizedString StartOverPopupCancelButton = "Cancel";
        public LocalizedString StartOverPopupDesc = "You'll start over with this Frever but all clothes and accessories you've added will be removedUMA_EDITOR_";
        [Space]
        public LocalizedString AssetPurchasedSnackbarTitle = "Asset purchased";
        public LocalizedString AssetPurchasedSnackbarDesc = "Looking fine!";
        [Space]
        public LocalizedString OutfitOptionsHeader = "Options";
        public LocalizedString DeleteOutfitOption = "Delete this outfit";
        public LocalizedString ChangeToThisOutfitOption = "Change to this outfit";
        [Space]
        public LocalizedString ChangeToThisLookOption = "Change to this look";
        public LocalizedString AddToFavouriteOutfitOption = "Add to my Favourite Outfits";
        [Space]
        public LocalizedString ExitPopupTitle = "Exit wardrobe";
        public LocalizedString ExitPopupUnsavedChangesDesc = "You have unsaved changes. Are you sure you want to exit?";
        public LocalizedString ExitPopupCancelButton = "Cancel";
        public LocalizedString ExitPopupExitButton = "Exit";
        [Space]
        public LocalizedString SaveCharacterPopupDesc = "You have unsaved changes. Are you sure you want to exit?";
        public LocalizedString SaveCharacterPopupCancelButton = "Cancel";
        public LocalizedString SaveCharacterPopupSaveButton = "Save";
        [Space]
        public LocalizedString SavingCharacterLoadingTitle = "Saving";
        public LocalizedString UpdatingCharacterLoadingTitle = "Updating Frever";
        [Space]
        public LocalizedString CannotDeleteCharacterUsedInRecordingSnackbarDesc = "This outfit cannot be deleted right now, because it's being used in video recording.";
        [Space]
        public LocalizedString SaveOutfitButton = "Save outfit";
        public LocalizedString SaveButton = "Save";
        public LocalizedString SaveOutfitBuyButton = "Buy";
        public LocalizedString SaveOutfitOnboardingButton = "Continue";
        public LocalizedString SaveOutfitNotEnoughFundsButton = "Not enough funds";
    }
}