using System;

using ReactiveServices.MessageBus;

namespace ReactiveServices.ComputationalUnit.Settings
{
    public class DispatcherId : Id<DispatcherId>
    {
        private const string LifeSignalSubscriptionIdPrefix = "LifeSignalSubscriptionForDispatcher";
        public SubscriptionId LifeSignalSubscriptionId
        {
            get
            {
                return SubscriptionId.FromString(String.Format("{0}_{1}", LifeSignalSubscriptionIdPrefix, Value));
            }
        }

        private const string PoisonPillRequestsSubscriptionIdPrefix = "PoisonPillSubscriptionForDispatcher";
        public SubscriptionId PoisonPillSubscriptionId
        {
            get
            {
                return SubscriptionId.FromString(String.Format("{0}_{1}", PoisonPillRequestsSubscriptionIdPrefix, Value));
            }
        }

        private const string DetachDispatcherSubscriptionIdPrevix = "DetachDispatcherSubscriptionForDispatcher";
        public SubscriptionId DetachDispatcherSubscriptionId
        {
            get
            {
                return SubscriptionId.FromString(String.Format("{0}_{1}", DetachDispatcherSubscriptionIdPrevix, Value));
            }
        }

        public TopicId DetachDispatcherTopicId
        {
            get
            {
                return TopicId.FromString(Value);
            }
        }
    }
}
