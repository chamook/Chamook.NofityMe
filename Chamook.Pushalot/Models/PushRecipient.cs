using System;

namespace Chamook.Pushalot.Models
{
    public sealed class PushRecipient
    {
        public Guid Id { get; set; }
        public String Name { get; set; }
        public String Key { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
