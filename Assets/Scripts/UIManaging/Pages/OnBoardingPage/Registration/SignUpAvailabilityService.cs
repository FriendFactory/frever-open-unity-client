using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Bridge;

namespace UIManaging.Pages.OnBoardingPage.Registration
{
    /// <summary>
    /// Temporary solution for preventing registration of new users if our servers can't scale up and handle new users
    /// This object checks on s3 bucket file if moke-file(tag) is existed on bucket. If exists - then we should block registartion
    /// </summary>
    internal sealed class SignUpAvailabilityService
    {
        private const string BUCKET_URL = "https://ff-publicfiles.s3.eu-central-1.amazonaws.com";
        private bool? _canSignUp;
        
        private readonly IBridge _bridge;

        public SignUpAvailabilityService(IBridge bridge)
        {
            _bridge = bridge;
        }

        public async Task<bool> CanSignUp()
        {
            if (_canSignUp.HasValue) return _canSignUp.Value;

            var environment = _bridge.Environment.ToString();
            var fileUrl = $"{BUCKET_URL}/Registration/{environment}/block_registration";
            
            using (var client = new HttpClient())
            {
                var resp = await client.GetAsync(fileUrl);
                _canSignUp = !resp.IsSuccessStatusCode;
            }

            return _canSignUp.Value;
        }
    }
}