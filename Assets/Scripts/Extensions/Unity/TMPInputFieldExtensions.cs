using TMPro;

namespace Extensions
{
    public static class TMPInputFieldExtensions
    {
        public static int GetStringIndexFromCaretPosition(this TMP_InputField inputField, int caretPosition)
        {
            // Clamp values between 0 and character count.
            ClampCaretPos(inputField, ref caretPosition);

            return inputField.textComponent.textInfo.characterInfo[caretPosition].index;
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private static void ClampCaretPos(TMP_InputField inputField, ref int pos)
        {
            if (pos < 0)
                pos = 0;
            else if (pos > inputField.textComponent.textInfo.characterCount - 1)
                pos = inputField.textComponent.textInfo.characterCount - 1;
        }
    }
}