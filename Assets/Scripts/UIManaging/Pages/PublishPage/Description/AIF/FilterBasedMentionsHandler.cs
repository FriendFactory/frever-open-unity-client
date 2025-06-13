using System.Collections;
using System.Text.RegularExpressions;
using AdvancedInputFieldPlugin;
using UIManaging.Common.Ui;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.SharingPage.Ui
{
    public class FilterBasedMentionsHandler: DescriptionHandlerBase<(string, string)>
    {
        private const int MENTIONS_LIMIT = 10;
        
        [SerializeField] private MentionsPanel _mentionsPanel;

        [Inject] private SnackBarHelper _snackBarHelper;

        private int _lastCaretPosition;
        
        protected override char StartWith => '@';

        private void OnEnable()
        {
            _mentionsPanel.OnProfileSelected += OnItemSelected;
            _mentionsPanel.OnBackButtonPressed += OnBackButtonPressed;
        }

        private void OnDisable()
        {
            _mentionsPanel.OnProfileSelected -= OnItemSelected;
            _mentionsPanel.OnBackButtonPressed -= OnBackButtonPressed;
        }


        public override void Hide()
        {
            _mentionsPanel.Hide();
        }

        public override bool ValidateConstraints(ref TextEditFrame textEditFrame)
        {
            var mentionsCount = Regex.Matches(InputField.RichText, MentionsPanel.MENTION_REGEX).Count;
            if (mentionsCount >= MENTIONS_LIMIT)
            {
                var text = textEditFrame.text.Remove(textEditFrame.text.Length - 1);
                textEditFrame.text = text;
                
                _snackBarHelper.ShowInformationSnackBar("Mention users reach limit.", 2);
                return false;
            }

            return true;
        }

        public override void Process(Occurrence occurrence)
        {
            base.Process(occurrence);

            _lastCaretPosition = InputField.CaretPosition;
            
            _mentionsPanel.Show();
        }
        
        protected override RichTextBindingData GetRichTextBindingData((string, string) tagData)
        {
            var (tagName, richText) = tagData;
            
            return new RichTextBindingData($"{StartWith}{tagName}", richText);
        }
        
        private void OnBackButtonPressed()
        {
            StartCoroutine(RemoveLastChar());
        }

        private IEnumerator RemoveLastChar()
        {
            yield return new WaitForEndOfFrame();

            if (!string.IsNullOrEmpty(InputField.Text))
            {
                var text = InputField.Text.Remove(InputField.Text.Length - 1);
                InputField.Text = text;
                InputField.CaretPosition = _lastCaretPosition - 1; 
                InputField.ManualSelect();
            }
            
            _mentionsPanel.Hide();
        }
    }
}