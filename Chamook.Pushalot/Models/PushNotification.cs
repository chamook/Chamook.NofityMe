using System;

namespace Chamook.Pushalot.Models
{
    public class PushNotification
    {
        public Guid Id { get; set; }
        public String Title { get; set; }
        public String Message { get; set; }
        public String LinkTitle { get; set; }
        public String Link { get; set; }
        public String Image { get; set; }
        public NotificationPriority Importance { get; set; }
        public int LifeTime { get; set; }
        public PushRecipient Recipient { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }

    public enum NotificationPriority
    {
        Low,
        Normal,
        High
    }
}
