using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.Networking.Proximity;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Chamook.NotifyMe.Common;
// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641
using Chamook.NotifyMe.DataModel;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Chamook.Pushalot;
using Chamook.Pushalot.Models;

namespace Chamook.NotifyMe
{
    /// <summary>
    /// A page that displays details for a single item within a group.
    /// </summary>
    public sealed partial class ItemPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();

        private Guid _itemId;

        public ItemPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        } 

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
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
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            _itemId = (Guid) e.NavigationParameter;
            var item = await NotificationDataSource.GetNotificationAsync((Guid)e.NavigationParameter);
            this.DefaultViewModel["Item"] = item;

            //disable the write to nfc button
            btnSendToNfc.IsEnabled = false;
            
            //subscribe to wait for a writeable tag
            var proximityDevice = ProximityDevice.GetDefault();
            if (proximityDevice != null)
                proximityDevice.SubscribeForMessage("WriteableTag", WriteableTagDetected);
        }

        private async void WriteableTagDetected(ProximityDevice sender, ProximityMessage message)
        {
            //if we detect a tag, enable the button to write to it
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => btnSendToNfc.IsEnabled = true);

        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/>.</param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
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

        private async void BtnSendNotification_OnClick(object sender, RoutedEventArgs e)
        {
            await Manager.SendNotification(await NotificationDataSource.GetNotificationAsync(_itemId));
        }

        private void BtnDeleteNotification_OnClick(object sender, RoutedEventArgs e)
        {
            NotificationDataSource.RemoveNotification(_itemId);
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
            else
            {
                Frame.Navigate(typeof (PivotPage));
            }
        }

        private void BtnSendToNfc_OnClick(object sender, RoutedEventArgs e)
        {
            var proximityDevice = ProximityDevice.GetDefault();
            if (proximityDevice != null)
            {
                var command = String.Format("nId={0}\tWindowsPhone\t{1}", _itemId, CurrentApp.AppId.ToString("B"));
                var byteArray = Encoding.Unicode.GetBytes(command);
                proximityDevice.PublishBinaryMessage("LaunchApp:WriteTag", byteArray.AsBuffer(), MessageTransmittedHandler);
            }
        }

        private async void MessageTransmittedHandler(ProximityDevice sender, long messageId)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                var md = new MessageDialog("Tag written!", "Hurray");
                md.ShowAsync();
            });
        }

        private SecondaryTile CreateTileFromItem(PushNotification notification)
        {
            return new SecondaryTile(notification.Id.ToString("N"), notification.Title,
                String.Format("nId={0}", notification.Id), new Uri("ms-appx:///Assets/secondTile.png"), TileSize.Default);
        }

        private async void BtnPinToStart_OnClick(object sender, RoutedEventArgs e)
        {
            var notification = await NotificationDataSource.GetNotificationAsync(_itemId);
            var tile = CreateTileFromItem(notification);
            await tile.RequestCreateAsync();
        }
    }
}