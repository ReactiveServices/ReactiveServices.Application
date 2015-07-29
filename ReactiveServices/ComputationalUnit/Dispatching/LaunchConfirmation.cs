using System.Runtime.Serialization;
using ReactiveServices.ComputationalUnit.Settings;
using ReactiveServices.MessageBus;

namespace ReactiveServices.ComputationalUnit.Dispatching
{
    [DataContract]
    public class LaunchConfirmation : Message
    {
        [DataMember]
        public DispatcherId DispatcherId { get; set; }
    }
}
