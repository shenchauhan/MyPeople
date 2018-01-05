using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using Windows.Data.Xml.Dom;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MyPeople
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void CreateContact_Click(object sender, RoutedEventArgs e)
        {
            var contact = await FindContact("joebloggs@contoso.com");

            if (contact == null)
            {
                await CreateAContact();
                contact = await FindContact("joebloggs@contoso.com");
            }

            await TagAContact(contact);
            await PinAContact(contact);
        }

        private async Task<Contact> FindContact(string id)
        {
            ContactStore contactStore = await ContactManager.RequestStoreAsync();
            IReadOnlyList<Contact> contacts = null;
            contacts = await contactStore.FindContactsAsync(id);
            return contacts.FirstOrDefault();
        }

        private async Task CreateAContact()
        {
            var contact = new Contact
            {
                FirstName = "Joe",
                LastName = "Bloggs",
                SourceDisplayPicture = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/shen.png")),
                RemoteId = "joebloggs@contoso.com"
            };

            var email = new ContactEmail
            {
                Address = "joebloggs@contoso.com",
                Kind = ContactEmailKind.Other
            };

            contact.Emails.Add(email);

            var phone = new ContactPhone
            {
                Number = "1234567890",
                Kind = ContactPhoneKind.Mobile
            };

            contact.Phones.Add(phone);

            var store = await ContactManager.RequestStoreAsync(ContactStoreAccessType.AppContactsReadWrite);

            ContactList contactList;
            IReadOnlyList<ContactList> contactLists = await store.FindContactListsAsync();

            if (!contactLists.Any())
            {
                contactList = await store.CreateContactListAsync("MyPeopleContactList");
            }
            else
            {
                contactList = contactLists.First();
            }

            await contactList.SaveContactAsync(contact);
        }

        private async Task TagAContact(Contact contact)
        {
            ContactAnnotationStore annotationStore = await ContactManager.RequestAnnotationStoreAsync(ContactAnnotationStoreAccessType.AppAnnotationsReadWrite);
            ContactAnnotationList annotationList;
            IReadOnlyList<ContactAnnotationList> annotationLists = await annotationStore.FindAnnotationListsAsync();

            if (!annotationLists.Any())
            {
                annotationList = await annotationStore.CreateAnnotationListAsync();
            }
            else
            {
                annotationList = annotationLists.First();
            }

            var annotation = new ContactAnnotation
            {
                ContactId = contact.Id,
                RemoteId = "joebloggs@contoso.com",
                SupportedOperations =
                ContactAnnotationOperations.AudioCall |
                ContactAnnotationOperations.VideoCall |
                ContactAnnotationOperations.ContactProfile |
                ContactAnnotationOperations.Share | ContactAnnotationOperations.Message
            };
            string appId = "d2b5f9e1-1de2-4efb-863b-9eb5737d84b1_75cr2b68sm664!App";
            annotation.ProviderProperties.Add("ContactPanelAppID", appId);
            annotation.ProviderProperties.Add("ContactShareAppID", appId);

            await annotationList.TrySaveAnnotationAsync(annotation);
        }

        private async Task PinAContact(Contact contact)
        {
            PinnedContactManager pinnedContactManager = PinnedContactManager.GetDefault();
            await pinnedContactManager.RequestPinContactAsync(contact, PinnedContactSurface.Taskbar);
        }

        private void ShoulderTap_Click(object sender, RoutedEventArgs e)
        {
            string xmlText = File.ReadAllText(@"Assets\toast.xml");
            XmlDocument xmlContent = new XmlDocument();
            xmlContent.LoadXml(xmlText);

            var notification = new ToastNotification(xmlContent);
            ToastNotificationManager.CreateToastNotifier().Show(notification);
        }
    }
}
