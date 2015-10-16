using NLog;
using ReactiveServices.ComputationalUnit.Dispatching;
using ReactiveServices.ComputationalUnit.Settings;
using ReactiveServices.Configuration;
using ReactiveServices.Extensions;
using System;
using System.IO;
using PostSharp.Patterns.Diagnostics;

namespace ReactiveServices.ComputationalUnit
{
    public class Program
    {
        private static Dispatcher Dispatcher { get; set; }
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [Log]
        [LogException]
        public static void Main(string[] args)
        {
            if (args.Length == 0 || args.Length > 2)
            {
                Console.WriteLine("Invalid arguments!");
                Console.WriteLine();
                Console.WriteLine("Usage: ComputationalUnit.exe \"SettingsFilePath\"");
                return;
            }

            AppDomain.CurrentDomain.LogExceptions();

            DependencyResolver.Initialize();

            var settingsFileName = args[0];

            Load(settingsFileName);
        }

        [Log]
        [LogException]
        private static void Load(string settingsFileName)
        {
            var settings = new DispatcherSettings();

            try
            {
                //System.Threading.Thread.Sleep(3000);//DEBUG

                if (File.Exists(settingsFileName))
                {
                    using (var stream = new FileStream(settingsFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                        settings.LoadFrom(stream);
                }
                else
                {
                    var errorMessage = String.Format("Invalid script file: {0}", settingsFileName);
                    Log.Info(errorMessage);
                    Console.WriteLine(errorMessage);
                    return;
                }

                settings.Validate();

                Dispatcher = new Dispatcher();
                Dispatcher.Disposed += (s, e) =>
                {
                    Environment.Exit(0);
                };

                Dispatcher.Initialize(settings);
            }
            catch (Exception ex)
            {
                var errorMessage = String.Format("Could not load Computational Unit for {0}!", settings.DispatcherId);
                Log.Error(ex, errorMessage);
                Console.WriteLine(errorMessage);
                Environment.Exit(0);
            }
        }
    }
}
