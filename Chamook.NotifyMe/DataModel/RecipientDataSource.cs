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
    public sealed class RecipientDataSource
    {
        private static RecipientDataSource _recipientDataSource = new RecipientDataSource();
        private const string Filename = "RecipientData.json";

        public ObservableCollection<PushRecipient> Recipients = new ObservableCollection<PushRecipient>();

        public RecipientDataSource()
        {
            //Recipients.CollectionChanged += RecipientsOnCollectionChanged;
        }

        private async void RecipientsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            await SaveDataAsync();
        }

        public static async void AddRecipient(PushRecipient recipient)
        {
            _recipientDataSource.Recipients.Add(recipient);
            await _recipientDataSource.SaveDataAsync();
        }

        public static async Task<ObservableCollection<PushRecipient>> GetRecipientsAsync()
        {
            await _recipientDataSource.GetDataAsync();
            return _recipientDataSource.Recipients;
        }

        public static async Task<PushRecipient> GetRecipientAsync(Guid uniqueId)
        {
            await _recipientDataSource.GetDataAsync();
            return _recipientDataSource.Recipients.First(x => x.Id == uniqueId);
        }

        public static async void RemoveRecipient(Guid id)
        {
            var item = _recipientDataSource.Recipients.First(i => i.Id == id);
            _recipientDataSource.Recipients.Remove(item);
            await _recipientDataSource.SaveDataAsync();
        }

        private async Task GetDataAsync()
        {
            if (Recipients.Count != 0)
                return;

            try
            {
                var folder = ApplicationData.Current.RoamingFolder;
                var file = await folder.GetFileAsync(Filename);
                var jsonText = await FileIO.ReadTextAsync(file);
                Recipients = JsonConvert.DeserializeObject<ObservableCollection<PushRecipient>>(jsonText);
            }
            catch (FileNotFoundException)
            {
                SaveDataAsync();
            }
            
        }

        public async Task SaveDataAsync()
        {
            var json = JsonConvert.SerializeObject(Recipients);
            var fileBytes = Encoding.UTF8.GetBytes(json);

            var folder = ApplicationData.Current.RoamingFolder;
            var file = await folder.CreateFileAsync(Filename, CreationCollisionOption.ReplaceExisting);
            using (var s = await file.OpenStreamForWriteAsync())
            {
                s.Write(fileBytes, 0, fileBytes.Length);
            }

        }
    }
}