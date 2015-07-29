using ReactiveServices.ComputationalUnit.Settings;
using ReactiveServices.MessageBus;
using System.Runtime.Serialization;

namespace ReactiveServices.ComputationalUnit.Dispatching
{
    public enum PoisonPillEffect
    {
        /// <summary>
        /// Try to cancel the executing jobs, do not wait then to finish and terminate the process
        /// </summary>
        Abort,
        /// <summary>
        /// Try to cancel the executing jobs, wait then to finish and terminate the process
        /// </summary>
        Cancel,
        /// <summary>
        /// Wait executing jobs to finish and terminate the process
        /// </summary>
        Wait,
        /// <summary>
        /// Terminate the process imediatly, no gracefull handling is tried and the supervisor is not detached, meaning it can be respawned
        /// </summary>
        Kill
    }

    [DataContract]
    public class PoisonPill : Message
    {
        [DataMember]
        public DispatcherId DispatcherId { get; set; }

        [DataMember]
        public PoisonPillEffect EffectOnCurrentWork { get; set; }
    }
}
