using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bridge.Services.ContentModeration;
using JetBrains.Annotations;
using UIManaging.Localization;
using UIManaging.SnackBarSystem;
using Zenject;

namespace Modules.ContentModeration
{
    [UsedImplicitly]
    public sealed class TextContentValidator
    {
        private readonly IContentModerationBridge _bridge;
        private readonly SnackBarHelper _snackBarHelper;

        [Inject] private ErrorMessageLocalization _localization;

        public TextContentValidator(IContentModerationBridge bridge, SnackBarHelper snackBarHelper)
        {
            _bridge = bridge;
            _snackBarHelper = snackBarHelper;
        }

        public async Task<bool> ValidateTextContent(string input, string overrideInappropriateTextResultMessage= null)
        {
            if (string.IsNullOrEmpty(input)) return true;

            var text = GetParsedText(input);

            var moderationResult = await _bridge.ModerateTextContent(text);
            if (moderationResult.IsError)
            {
                ShowInformationSnackBar(_localization.ModerationRequestFailedSnackbarMessage);
                return false;
            }

            if (!moderationResult.PassedModeration)
            {
                ShowInformationSnackBar(overrideInappropriateTextResultMessage ?? _localization.TextModerationFailedSnackbarMessage);
            }

            return moderationResult.PassedModeration;
        }

        private void ShowInformationSnackBar(string message)
        {
            _snackBarHelper.ShowInformationSnackBar(message, 2);
        }

        private static string GetParsedText(string text)
        {
            return Regex.Replace(text, @"\n|\t", " ", RegexOptions.Multiline);
        }
    }
}