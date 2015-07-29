using System;
using System.IO;
using ReactiveServices.ComputationalUnit.Settings;
using ReactiveServices.Configuration;

namespace ReactiveServices.Application
{
    public static class Coordinator
    {
        private static Supervisor Supervisor;

        public static void Start(string bootstrapFileName)
        {
            if (!File.Exists(bootstrapFileName))
                throw new ArgumentException("Bootstrap file could not be found!");

            DependencyResolver.Reset();
            DependencyResolver.Initialize();

            Supervisor = DependencyResolver.Get<Supervisor>();

            var bootstrapSettings = new BootstrapSettings();
            using (var stream = new FileStream(bootstrapFileName, FileMode.Open, FileAccess.Read))
            {
                bootstrapSettings.LoadFrom(stream);
            }

            Supervisor.Initialize(bootstrapSettings);
            Supervisor.IsComputationalUnitRestorationEnabled = true;
            Supervisor.Bootstrap();
        }

        public static void Stop()
        {
            Supervisor.Dispose();
        }
    }
}
