using Windows.UI.Notifications;
using Chamook.Pushalot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Chamook.NotifyMe.DataModel
{
    public sealed class NotificationDataSource
    {
        private static NotificationDataSource _notificationDataSource = new NotificationDataSource();
        private const string Filename = "NotificationData.json";
        public bool IsSaving;

        public ObservableCollection<PushNotification> Notifications = new ObservableCollection<PushNotification>();

        public NotificationDataSource()
        {
            //Notifications.CollectionChanged += NotificationsOnCollectionChanged;
        }

        private async void NotificationsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            await SaveDataAsync();
        }

        public static async void AddNotification(PushNotification notification)
        {
            _notificationDataSource.Notifications.Add(notification);
            await _notificationDataSource.SaveDataAsync();
        }

        public static async void RemoveNotification(Guid id)
        {
            var item = _notificationDataSource.Notifications.First(i => i.Id == id);
            _notificationDataSource.Notifications.Remove(item);
            await _notificationDataSource.SaveDataAsync();
        }

        public static async Task<ObservableCollection<PushNotification>> GetNotificationsAsync()
        {
            await _notificationDataSource.GetDataAsync();
            return _notificationDataSource.Notifications;
        }

        public static async Task<PushNotification> GetNotificationAsync(Guid uniqueId)
        {
            await _notificationDataSource.GetDataAsync();
            return _notificationDataSource.Notifications.First(x => x.Id == uniqueId);
        }

        private async Task GetDataAsync()
        {
            if (Notifications.Count != 0)
                return;

            var folder = ApplicationData.Current.RoamingFolder;
            var file = await folder.GetFileAsync(Filename);
            var jsonText = await FileIO.ReadTextAsync(file);
            Notifications = JsonConvert.DeserializeObject<ObservableCollection<PushNotification>>(jsonText);
        }

        public async Task SaveDataAsync()
        {
            IsSaving = true;
            var json = JsonConvert.SerializeObject(Notifications);
            var fileBytes = Encoding.UTF8.GetBytes(json);

            var folder = ApplicationData.Current.RoamingFolder;
            var file = await folder.CreateFileAsync(Filename, CreationCollisionOption.ReplaceExisting);
            using (var s = await file.OpenStreamForWriteAsync())
            {
                s.Write(fileBytes, 0, fileBytes.Length);
            }
            IsSaving = false;
        }
    }
}