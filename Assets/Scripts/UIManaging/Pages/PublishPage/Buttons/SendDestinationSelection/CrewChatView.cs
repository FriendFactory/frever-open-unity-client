using Modules.Crew;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.PublishPage.Buttons.SendDestinationSelection
{
    internal sealed class CrewChatView : MonoBehaviour
    {
        [Inject] private CrewService _crewService;
        [SerializeField] private RawImage _rawImage;
        
        public void Display()
        {
            gameObject.SetActive(true);
            _rawImage.texture = _crewService.GetCrewBall();
        }

        private void OnDestroy()
        {
            if (_rawImage.texture == null) return;
            Destroy(_rawImage.texture);
            _rawImage.texture = null;
        }
    }
}