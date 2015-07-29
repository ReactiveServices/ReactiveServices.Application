using NLog;
using System;
using System.Threading;
using PostSharp.Patterns.Diagnostics;
using ReactiveServices.Extensions;

namespace ReactiveServices.Application
{
    public class Bootstrap
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [Log]
        [LogException]
        public static void Run(string bootstrapConfig = "Bootstrap.config")
        {
            AppDomain.CurrentDomain.LogExceptions();
            try
            {
                Console.WriteLine(String.Format("Loading Reactive Services from file {0}...", bootstrapConfig));

                Coordinator.Start(bootstrapConfig);

                Console.WriteLine("Reactive Services running...");
            }
            catch (Exception e)
            {
                Log.Error(String.Format("Exception: {0}", e));
                Console.ReadKey();
            }
        }
    }
}
