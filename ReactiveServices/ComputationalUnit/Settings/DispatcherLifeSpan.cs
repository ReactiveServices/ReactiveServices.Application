using System;

namespace ReactiveServices.ComputationalUnit.Settings
{
    public enum DispatcherLifeSpanMode
    {
        Perpetual,
        UntilFirstJobIsCompleted,
        UntilTimedOut,
        UntilFirstJobIsCompletedOrTimedOut
    }

    public struct DispatcherLifeSpan
    {
        public DispatcherLifeSpanMode Mode;
        public TimeSpan Timeout;
    }
}
