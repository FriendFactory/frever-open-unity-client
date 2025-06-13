using System.Threading;
using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.VideoMessage.ImageGeneration
{
    public abstract class BaseImageGenerationPanel : MonoBehaviour
    {
        [SerializeField] private Sprite _logo;
        [SerializeField] private GenerationProgressOverlay _loadingOverlay;
        [SerializeField] private Button _generateButton;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        protected Sprite Logo => _logo;
        protected GenerationProgressOverlay LoadingOverlay => _loadingOverlay;
        protected CancellationTokenSource TokenSource { get; private set; }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected void OnEnable()
        {
            _generateButton.onClick.AddListener(OpenGenerationPopup);
        }

        protected void OnDisable()
        {
            _generateButton.onClick.RemoveListener(OpenGenerationPopup);
        }

        protected void OnDestroy()
        {
            CancelCurrentRequest();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected abstract void OpenGenerationPopup();

        protected CancellationTokenSource CreateNewTokenSource()
        {
            TokenSource?.CancelAndDispose();
            TokenSource = new CancellationTokenSource();
            return TokenSource;
        }

        protected void CancelImageGeneration()
        {
            CancelCurrentRequest();
            LoadingOverlay.Hide();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void CancelCurrentRequest()
        {
            TokenSource?.CancelAndDispose();
            TokenSource = null;
        }
    }
}