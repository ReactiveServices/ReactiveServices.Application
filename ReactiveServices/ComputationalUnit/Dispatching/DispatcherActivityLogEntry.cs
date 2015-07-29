using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using NLog.Targets;
using PostSharp.Patterns.Diagnostics;
using ReactiveServices.ComputationalUnit.Settings;
using ReactiveServices.ComputationalUnit.Work;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReactiveServices.ComputationalUnit.Dispatching
{
    [Serializable]
    [Log(AttributeExclude = true)]
    [LogException(AttributeExclude = true)]
    public class DispatcherActivityLogEntry
    {
        public DispatcherActivityLogEntry()
        {
            TimeStamp = DateTime.Now;
        }

        public DispatcherId DispatcherId { get; set; }
        public DateTime TimeStamp { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public DispatcherActivity Activity { get; set; }
        public Job Job { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public JobStatus? OldStatus { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public JobStatus? NewStatus { get; set; }
        public PoisonPill PoisonPill { get; set; }
        public Exception Exception { get; set; }

        internal static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public override string ToString()
        {
            var result = JsonConvert.SerializeObject(this, JsonSerializerSettings);
            return result;
        }
    }

    internal static class MemoryTargetDispatcherLogExtensions
    {
        public static IEnumerable<DispatcherActivityLogEntry> GetDispatcherEvents(this MemoryTarget events, DispatcherId dispatcherId = null)
        {
            var allLogs = events.Logs.ToArray();
            var supposedJsonLogs = allLogs.Select(logEntry =>
            {
                var supposedJsonStart = logEntry.IndexOf('{');
                var supposedJsonLength = logEntry.Length - supposedJsonStart;
                if ((supposedJsonStart > 0) && (supposedJsonLength > 2))
                    return logEntry.Substring(supposedJsonStart, supposedJsonLength);
                return null;
            }).Where(l => l != null && l.IsEnclosedByBraces());

            var validJsonLogs = supposedJsonLogs.Select(l => l.FromString()).Where(l => l != null);
            var dispatcherEntries = validJsonLogs.ToArray();
            if (dispatcherId != null)
            {
                dispatcherEntries = dispatcherEntries.Where(
                    e => e.DispatcherId == dispatcherId
                ).ToArray();
            }
            return dispatcherEntries;
        }

        internal static DispatcherActivityLogEntry FromString(this string jsonLogEntry)
        {
            try
            {
                return jsonLogEntry.IsEnclosedByBraces() ? JsonConvert.DeserializeObject<DispatcherActivityLogEntry>(jsonLogEntry, DispatcherActivityLogEntry.JsonSerializerSettings) : null;
            }
            catch
            {
                return null;
            }
        }

        private static bool IsEnclosedByBraces(this string text)
        {
            return (text.First() == '{') && (text.Last() == '}');
        }

        internal static bool FindMatch(this MemoryTarget events, DispatcherId dispatcherId, DispatcherActivity activity)
        {
            var dispatcherEntries = events.GetDispatcherEvents(dispatcherId);
            return dispatcherEntries.Any(e => e.Activity == activity);
        }

        internal static DispatcherActivityLogEntry Find(this MemoryTarget events, DispatcherId dispatcherId, DispatcherActivity activity)
        {
            var dispatcherEntries = events.GetDispatcherEvents(dispatcherId);
            return dispatcherEntries.SingleOrDefault(e => e.Activity == activity);
        }

        internal static DispatcherActivityLogEntry Find(this MemoryTarget events, DispatcherId dispatcherId, DispatcherActivity activity, JobStatus newStatus)
        {
            var dispatcherEntries = events.GetDispatcherEvents(dispatcherId);

            return dispatcherEntries.SingleOrDefault(
                e => (e.Activity == activity) &&
                     (e.NewStatus == newStatus)
            );
        }
    }

    internal static class LoggerExtensions
    {
        internal static void DispatcherActivity(this Logger log, DispatcherId dispatcherId, DispatcherActivity activity, Job job, JobStatus oldStatus, JobStatus? newStatus = null)
        {
            var logEntry = new DispatcherActivityLogEntry
            {
                DispatcherId = dispatcherId,
                Activity = activity,
                Job = job,
                OldStatus = oldStatus,
                NewStatus = newStatus ?? oldStatus
            };

            log.Info(logEntry);
        }

        internal static void DispatcherActivity(this Logger log, DispatcherId dispatcherId, DispatcherActivity activity, PoisonPill poisonPill = null, Exception exception = null, bool logAsError = true)
        {
            var logEntry = new DispatcherActivityLogEntry
            {
                DispatcherId = dispatcherId,
                Activity = activity,
                PoisonPill = poisonPill,
                Exception = exception
            };

            if (exception == null)
            {
                if (activity == Dispatching.DispatcherActivity.CheckingForUnfinishedJobs)
                    log.Debug(logEntry);
                else
                    log.Info(logEntry);
            }
            else if (logAsError)
                log.Error(logEntry.ToString(), exception);
            else
                log.Info(logEntry.ToString(), exception);
        }
    }
}
