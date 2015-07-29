using ReactiveServices.ComputationalUnit.Settings;
using ReactiveServices.MessageBus;
using System.Runtime.Serialization;

namespace ReactiveServices.ComputationalUnit.Dispatching
{
    [DataContract]
    public class DispatcherOnline : Message
    {
        [DataMember]
        public DispatcherId DispatcherId { get; set; }
    }
}
