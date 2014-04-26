using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Chamook.NotifyMe.Common;
// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641
using Chamook.NotifyMe.DataModel;
using Chamook.Pushalot;
using Chamook.Pushalot.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Chamook.NotifyMe
{
    public sealed partial class PivotPage : Page
    {
        private const string FirstGroupName = "Notifications";

        private readonly NavigationHelper navigationHelper;
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        public ObservableCollection<PushNotification> Notifications { get; set; }
        public ObservableCollection<PushRecipient> Recipients { get; set; }


        public PivotPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            Notifications = new ObservableCollection<PushNotification>();
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
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>.
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            await LoadNotifications();
            await LoadRecipients();
        }

        private async Task LoadNotifications()
        {
            try
            {
                Notifications.Clear();
                foreach (var notification in await NotificationDataSource.GetNotificationsAsync())
                {
                    Notifications.Add(notification);
                }
            }
            catch (FileNotFoundException)
            {
                
            }

            if (Notifications.Any())
            {
                ListViewNotifications.Visibility = Visibility.Visible;
                spNoItems.Visibility = Visibility.Collapsed;
            }
            else
            {
                ListViewNotifications.Visibility = Visibility.Collapsed;
                spNoItems.Visibility = Visibility.Visible;
            }
        }

        private async Task LoadRecipients()
        {
            try
            {
                Recipients.Clear();
                foreach (var recipient in await RecipientDataSource.GetRecipientsAsync())
                {
                    Recipients.Add(recipient);
                }
            }
            catch (FileNotFoundException)
            {
                var md = new MessageDialog("no file");
                md.ShowAsync();
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache. Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/>.</param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
        }

        /// <summary>
        /// Adds an item to the list when the app bar button is clicked.
        /// </summary>
        private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (CreateNotification));
        }

        /// <summary>
        /// Invoked when an item within a section is clicked.
        /// </summary>
        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var itemId = ((PushNotification)e.ClickedItem).Id;
            if (!Frame.Navigate(typeof(ItemPage), itemId))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
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

        private void WelcomeButton_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (WelcomePage));
        }

        //private void SpRecipientsList_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        //{
        //    FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        //}

        private async void MenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuFlyoutItem;
            var selectedRecipient = mi.DataContext as PushRecipient;


            if (selectedRecipient != null)
            {
                RecipientDataSource.RemoveRecipient((Guid)selectedRecipient.Id);
            }
            else
            {
                var md = new MessageDialog("I can't seem to find that recipient to delete, how embarrassing.", "Oops");
                await md.ShowAsync();
            }

            await LoadRecipients();
        }
    }
}
