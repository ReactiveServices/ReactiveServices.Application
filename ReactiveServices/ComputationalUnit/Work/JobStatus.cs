using ReactiveServices.MessageBus;

namespace ReactiveServices.ComputationalUnit.Work
{
    public enum JobStatus
    {
        Pending,
        Processing,
        Succeeded,
        Failed
    }

    public static class JobStatusExtensions 
    {
        public static TopicId TopicId(this JobStatus status)
        {
            return MessageBus.TopicId.FromString(status.ToString());
        }
    }
}
