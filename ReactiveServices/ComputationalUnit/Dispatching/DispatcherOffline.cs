using ReactiveServices.ComputationalUnit.Settings;
using ReactiveServices.MessageBus;
using System.Runtime.Serialization;

namespace ReactiveServices.ComputationalUnit.Dispatching
{
    [DataContract]
    public class DispatcherOffline : Message
    {
        [DataMember]
        public DispatcherId DispatcherId { get; set; }
    }
}
