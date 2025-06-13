using Common.Abstract;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.RatingFeed
{
    internal sealed class RatingFeedProgressPanel: BaseContextPanel<RatingFeedProgress>
    {
        [SerializeField] private RatingFeedProgressBar _progressBar;
        [SerializeField] private TMP_Text _progressText;
        
        protected override void OnInitialized()
        {
            ContextData.ProgressChanged += OnProgressChanged;
            ContextData.Completed += OnCompleted;
            
            _progressBar.Initialize(ContextData);
            
            UpdateProgressText();
        }
        
        protected override void BeforeCleanUp()
        {
            ContextData.ProgressChanged -= OnProgressChanged;
            ContextData.Completed -= OnCompleted;
            
            _progressBar.CleanUp();
        }

        private void OnProgressChanged(int currentProgressIndex)
        {
            UpdateProgressText();
        }

        private void OnCompleted() => UpdateProgressText();

        private void UpdateProgressText()
        {
            _progressText.text = $"{ContextData.CurrentProgressIndex} of {ContextData.RatingsCount}";
        }
    }
}