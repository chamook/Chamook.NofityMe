using System.Collections.Generic;
using Chamook.Pushalot.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Chamook.Pushalot
{
    public static class Manager
    {
        private static readonly Uri Endpoint = new Uri("https://pushalot.com/api/sendmessage");

        public static async Task<bool> SendNotification(PushNotification message)
        {
            var values = BuildPostData(message);

            using (var client = new HttpClient())
            {
                HttpContent content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(Endpoint, content);

                if (response.IsSuccessStatusCode)
                    return true;

                return false;
            }
        }

        private static IEnumerable<KeyValuePair<string, string>> BuildPostData(PushNotification message)
        {
            var values = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("AuthorizationToken", message.Recipient.Key),
                new KeyValuePair<string, string>("Title", message.Title),
                new KeyValuePair<string, string>("Body", message.Message)
            };

            return values;
        }
    }
}


//public String Title { get; set; }
//        public String Message { get; set; }
//        public String LinkTitle { get; set; }
//        public String Link { get; set; }
//        public String Image { get; set; }
//        public NotificationPriority Importance { get; set; }
//        public int LifeTime { get; set; }
//        public Sender sender { get; set; }
//        public Recipient recipient { get; set; }