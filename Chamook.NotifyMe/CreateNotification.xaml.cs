﻿using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Chamook.NotifyMe.Common;
// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556
using Chamook.NotifyMe.DataModel;
using Chamook.Pushalot.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Chamook.NotifyMe
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateNotification : Page
    {
        private NavigationHelper navigationHelper;
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        public ObservableCollection<PushRecipient> Recipients { get; set; }

        private Guid _newNotificationId = Guid.Empty;

        public CreateNotification()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            Recipients = new ObservableCollection<PushRecipient>();
            DataContext = this;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var notId = e.NavigationParameter as Guid?;
            if (notId != null)
                _newNotificationId = Guid.Parse(notId.ToString());

            await RefreshRecipients();
            RefreshListButtonText();
        }

        private async Task RefreshRecipients()
        {
            Recipients.Clear();
            foreach (var rep in await RecipientDataSource.GetRecipientsAsync())
            {
                Recipients.Add(rep);
            }

            btnLstRecipient.IsEnabled = Recipients.Any();
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void SaveAppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (TxtTitle.Text == "" || TxtMessage.Text == "" || lpfRecipients.SelectedItem ==null)
            {
                var md = new MessageDialog(resourceLoader.GetString("GeneralValidationEmptyFailMessage"), "Add Notification");
                await md.ShowAsync();
                return;
            }

            NotificationDataSource.AddNotification(new PushNotification()
            {
                Id =  _newNotificationId == Guid.Empty ? Guid.NewGuid() : _newNotificationId,
                Title = TxtTitle.Text,
                Message = TxtMessage.Text,
                Recipient = lpfRecipients.SelectedItem as PushRecipient
            });
            Frame.Navigate(typeof (PivotPage));
        }

        private void Add_Recipient_OnClick(object sender, RoutedEventArgs e)
        {
            cdAddRecipient.ShowAsync();
        }

        private async void BtnSaveRecipient_OnClick(object sender, RoutedEventArgs e)
        {
            if (TxtKey.Text == "" || TxtName.Text == "")
            {
                var md = new MessageDialog(resourceLoader.GetString("GeneralValidationEmptyFailMessage"), "Add Recipient");
                await md.ShowAsync();
                return;
            }

            var recipient = new PushRecipient()
            {
                Id = Guid.NewGuid(),
                Key = TxtKey.Text,
                Name = TxtName.Text
            };
            RecipientDataSource.AddRecipient(recipient);
            await RefreshRecipients();

            TxtKey.Text = "";
            TxtName.Text = "";

            lpfRecipients.SelectedItem = Recipients.First(x => x.Id == recipient.Id);
            RefreshListButtonText();

            cdAddRecipient.Hide();
        }

        private void LpfRecipients_OnClosed(object sender, object e)
        {
            RefreshListButtonText();
        }

        private void RefreshListButtonText()
        {
            btnLstRecipient.Content = lpfRecipients.SelectedValue ?? "choose a recipient";
        }
    }
}