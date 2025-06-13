using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Modules.Contacts;
using UnityEngine;

namespace UIManaging.Pages.OnBoardingPage.UI.Pages
{
    internal class OnboardingContactsListModel
    {
        public IReadOnlyList<OnboardingContactsItemModel> Items => _items;

        private readonly IBridge _bridge;

        private List<OnboardingContactsItemModel> _items;

        private OnboardingContactsListModel(IBridge bridge)
        {
            _bridge = bridge;
        }

        private async Task<OnboardingContactsListModel> InitializeAsync(IEnumerable<ContactWithAccountItemModel> contacts)
        {
            _items = new List<OnboardingContactsItemModel>();

            var profileTasks = contacts.Select(AddProfileAsync);

            await Task.WhenAll(profileTasks);
            
            return this;

            async Task AddProfileAsync(ContactWithAccountItemModel contact)
            {
                var result = await _bridge.GetProfile(contact.GroupId);

                if (result.IsError)
                {
                    Debug.LogError($"[{GetType().Name}] Failed to get profile");
                    return;
                }
                
                _items.Add(new OnboardingContactsItemModel(contact.Name, result.Profile));
            }
        }

        public static async Task<OnboardingContactsListModel> CreateAsync(IBridge bridge, IEnumerable<ContactWithAccountItemModel> contacts)
        {
            var instance = new OnboardingContactsListModel(bridge);

            return await instance.InitializeAsync(contacts);
        }
    }
}