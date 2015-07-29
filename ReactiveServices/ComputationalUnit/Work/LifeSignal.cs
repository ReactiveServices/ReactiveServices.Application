using System;
using System.Runtime.Serialization;
using ReactiveServices.MessageBus;

namespace ReactiveServices.ComputationalUnit.Work
{
    [DataContract]
    class LifeSignal : Message
    {
        [DataMember]
        public TimeSpan Validity { get; private set; }
        [DataMember]
        public Id SourceId { get; set; }

        public LifeSignal()
        {
            Validity = TimeSpan.FromSeconds(1);
        }

        public bool IsExpired()
        {
            return DateTime.Now - CreationDate > Validity;
        }
    }
}
