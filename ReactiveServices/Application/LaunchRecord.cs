using System;
using ReactiveServices.ComputationalUnit.Settings;

namespace ReactiveServices.Application
{
    class LaunchRecord
    {
        public DispatcherId DispatcherId { get; internal set; }
        public DateTime RequestTime { get; internal set; }
        public DateTime ConfirmationTime { get; internal set; }
        public bool IsConfirmed
        {
            get
            {
                return ConfirmationTime != default(DateTime);
            }
        }
    }
}
