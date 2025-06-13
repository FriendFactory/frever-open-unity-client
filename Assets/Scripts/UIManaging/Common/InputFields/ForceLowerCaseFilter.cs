using AdvancedInputFieldPlugin;

namespace UIManaging.Common.InputFields
{
    public class ForceLowerCaseFilter: LiveProcessingFilter
    {
        public override TextEditFrame ProcessTextEditUpdate(TextEditFrame textEditFrame, TextEditFrame lastTextEditFrame)
        {
			if(textEditFrame.text == lastTextEditFrame.text) //No text change
			{
				return textEditFrame; //No processing needed, so allow change by returning current frame
			}
            
            textEditFrame.text = textEditFrame.text.ToLower();
            
            return textEditFrame;
        }
    }
}