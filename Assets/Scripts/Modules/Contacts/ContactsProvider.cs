using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Bridge;
using Bridge.Services.UserProfile.PhoneLookup;
using JetBrains.Annotations;
using UnityEngine;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;
using Zenject;

namespace Modules.Contacts
{
    [UsedImplicitly]
    public class ContactsProvider: IAddressBookContactsProvider 
    {
        private const string SAVED_REGISTERED_CONTACTS = "SavedRegisteredContactsAmount";
        private static readonly CultureInfo CULTURE = CultureInfo.CurrentCulture;
        
        [Inject] private IBridge _bridge;
        
        private Action<Error> _onFail;
        private Action<ContactsItemModel[]> _onComplete;
        private bool _onlyFreverContacts;

        public bool HasLoaded { get; set; }
        public ContactsItemModel[] Contacts { get; private set; }

        public void ReadContacts(bool onlyFreverContacts = false, Action<ContactsItemModel[]> onComplete = null, Action<Error> onFail = null)
        {
            _onFail = onFail;
            _onlyFreverContacts = onlyFreverContacts;
            _onComplete = onComplete;
            
            AddressBook.ReadContacts(OnReadContactsFinish);
        }
        
        public void ReadContactsWithPermission(bool onlyFreverContacts = false, Action<ContactsItemModel[]> onComplete = null, Action<Error> onFail = null)
        {
            _onFail = onFail;
            _onlyFreverContacts = onlyFreverContacts;
            _onComplete = onComplete;
            
            AddressBook.ReadContactsWithUserPermission(OnReadContactsFinish);
        }

        private void OnReadContactsFinish(AddressBookReadContactsResult result, Error error)
        {
            if (error != null)
            {
                _onFail.Invoke(error);
                return;
            }

            DownloadContactsWithAccounts(result.Contacts);
        }
        
        private async void DownloadContactsWithAccounts(IAddressBookContact[] contacts)
        {
            try
            {
                var phoneNumbers = contacts.Select(x => x.PhoneNumbers.FirstOrDefault()).ToArray();
                var result = await _bridge.LookupForFriends(phoneNumbers);

                var matchingContacts = GetFreverContacts(contacts, result.MatchedProfiles);

                var nonMatchingContacts = new List<IAddressBookContact>();
                if (!_onlyFreverContacts) nonMatchingContacts = GetNonFreverContacts(contacts, result.MatchedProfiles);

                PlayerPrefs.SetInt(GetSaveRegisteredContactsKey(), matchingContacts.Count);

                SetupContactItemModels(matchingContacts, nonMatchingContacts.ToArray());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        private Dictionary<long, IAddressBookContact> GetFreverContacts(IAddressBookContact[] contacts, IEnumerable<PhoneLookupInfo> matchedProfiles)
        {
            var matchingContacts = new Dictionary<long, IAddressBookContact>();
            foreach (var matchedProfile in matchedProfiles)
            {
                var isLocalUser = matchedProfile.GroupId == _bridge.Profile.GroupId;
                if (isLocalUser) continue;
                
                var matchingContactInfo = contacts.First(contact => contact.PhoneNumbers.FirstOrDefault() == matchedProfile.ProvidedPhoneNumber);
                matchingContacts[matchedProfile.GroupId] = matchingContactInfo;
            }

            return matchingContacts;
        }
        
        private List<IAddressBookContact> GetNonFreverContacts(IEnumerable<IAddressBookContact> contacts, IEnumerable<PhoneLookupInfo> matchedProfiles)
        {
            return contacts.Where(contact => matchedProfiles.All(profile =>
                                 {
                                     var (_, profileDigits) = ProcessPhoneNumber(profile.ProvidedPhoneNumber);
                                     var (_, digits) = ProcessPhoneNumber(contact.PhoneNumbers.FirstOrDefault());
                                     return profileDigits != digits;
                                 }))
                                .ToList();
        }
        
        private void SetupContactItemModels(Dictionary<long, IAddressBookContact> matchingContacts, IList<IAddressBookContact> nonMatchingContacts)
        {
            var allContactItemModels = new List<ContactsItemModel>();
            if (!matchingContacts.IsNullOrEmpty())
            {
                var matchingItemModels = GetMatchingContactItemModels(matchingContacts);
                allContactItemModels.AddRange(matchingItemModels);
            }

            if (!nonMatchingContacts.IsNullOrEmpty())
            {
                var nonMatchingItemModels = GetNonMatchingContactItemModels(nonMatchingContacts);
                allContactItemModels.AddRange(nonMatchingItemModels);
            }
            
            HasLoaded = true;
            var itemModels = allContactItemModels.ToArray();
            Contacts = itemModels;
            _onComplete?.Invoke(itemModels);
        }
        
        private IOrderedEnumerable<ContactsItemModel> GetNonMatchingContactItemModels(IEnumerable<IAddressBookContact> nonMatchingContacts)
        {
            var stringComparer = StringComparer.Create(CULTURE, false);
            var contactsItemModels = nonMatchingContacts
                                    .Select(ContactsModelFactory.GetContactWithoutAccount)
                                    .Where(contactItemModel => !string.IsNullOrEmpty(contactItemModel.PhoneNumber))
                                    .Cast<ContactsItemModel>()
                                    .ToList();

            return contactsItemModels
                  .OrderBy(x => x.Name, stringComparer)
                  .ThenBy(x=>x.LastName, stringComparer);
        }

        private IOrderedEnumerable<ContactsItemModel> GetMatchingContactItemModels(Dictionary<long, IAddressBookContact> matchingContacts)
        {
            var stringComparer = StringComparer.Create(CULTURE, false);
            var contactsItemModels = matchingContacts
                                    .Select(matchingContact => ContactsModelFactory.GetContactWithAccount(matchingContact.Value, matchingContact.Key))
                                    .Cast<ContactsItemModel>()
                                    .ToList();

            return contactsItemModels
                  .OrderBy(x => x.Name, stringComparer)
                  .ThenBy(x=>x.LastName, stringComparer);
        }

        private string GetSaveRegisteredContactsKey()
        {
            return $"{SAVED_REGISTERED_CONTACTS}_profile_{_bridge.Profile.GroupId}";
        }
        
        private (string, string) ProcessPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber)) return ("", "");

            // https://stackoverflow.com/a/47949522
            var pattern = @"(297|93|244|1264|358|355|376|971|54|374|1684|1268|61|43|994|257|32|229|226|880|359|973|1242|387|590|375|501|1441|591|55|1246|673|975|267|236|1|61|41|56|86|225|237|243|242|682|57|269|238|506|53|5999|61|1345|357|420|49|253|1767|45|1809|1829|1849|213|593|20|291|212|34|372|251|358|679|500|33|298|691|241|44|995|44|233|350|224|590|220|245|240|30|1473|299|502|594|1671|592|852|504|385|509|36|62|44|91|246|353|98|964|354|972|39|1876|44|962|81|76|77|254|996|855|686|1869|82|383|965|856|961|231|218|1758|423|94|266|370|352|371|853|590|212|377|373|261|960|52|692|389|223|356|95|382|976|1670|258|222|1664|596|230|265|60|262|264|687|227|672|234|505|683|31|47|977|674|64|968|92|507|64|51|63|680|675|48|1787|1939|850|351|595|970|689|974|262|40|7|250|966|249|221|65|500|4779|677|232|503|378|252|508|381|211|239|597|421|386|46|268|1721|248|963|1649|235|228|66|992|690|993|670|676|1868|216|90|688|886|255|256|380|598|1|998|3906698|379|1784|58|1284|1340|84|678|681|685|967|27|260|263)(9[976]\d|8[987530]\d|6[987]\d|5[90]\d|42\d|3[875]\d|2[98654321]\d|9[8543210]|8[6421]|6[6543210]|5[87654321]|4[987654310]|3[9643210]|2[70]|7|1)";
            var input = phoneNumber.Replace(" ", "").TrimStart('+', '0');
            var match = Regex.Match(input, pattern);
            var countryCode = match.Success ? match.Groups[1].Value : "";
            var digits = string.IsNullOrEmpty(countryCode) ? phoneNumber : phoneNumber.Substring(countryCode.Length + 1);

            return (countryCode, digits);
        }
    }
}