using System;
using System.Text;
using NUnit.Framework;
using FluentAssertions;
using System.Xml;
using ReactiveServices.ComputationalUnit.Settings;
using ReactiveServices.Configuration;

namespace ReactiveServices.ComputationalUnit.Dispatching.Tests.UnitTests
{
    [TestFixture]
    public class DispatcherSettingsTests
    {
        [Test][Category("stable")][Category("fast")]
        public void TestWriteToXmlAndReadFromXmlOnDispatcherSettings()
        {
            DependencyResolver.Reset();
            DependencyResolver.Initialize();
            
            var dispatcherSettings = DummyDispatcherSettings();

            var firstWrite = new XmlDocument();
            firstWrite.AppendChild(firstWrite.CreateElement("DispatcherSettings"));
            dispatcherSettings.WriteTo(firstWrite.DocumentElement);

            var anotherDispatcherSettings = new DispatcherSettings();
            anotherDispatcherSettings.ReadFrom(firstWrite.DocumentElement);

            var secondWrite = new XmlDocument();
            secondWrite.AppendChild(secondWrite.CreateElement("DispatcherSettings"));
            anotherDispatcherSettings.WriteTo(secondWrite.DocumentElement);

            var firstString = new StringBuilder();
            var firstText = XmlWriter.Create(firstString);
            firstWrite.Save(firstText);

            var secondString = new StringBuilder();
            var secondText = XmlWriter.Create(secondString);
            secondWrite.Save(secondText);

            secondString.ToString().Should().BeEquivalentTo(firstString.ToString());
        }

        private static DispatcherSettings DummyDispatcherSettings()
        {
            var dispatcherSettings = new DispatcherSettings
            {
                DispatcherId = DispatcherId.FromString("MyDispatcherId"),
                IntervalForCheckingUnfinishedJobs = TimeSpan.FromMilliseconds(999),
                MaximumNumberOfProcessingJobs = 9
            };
            dispatcherSettings.JobConfigurations.Add(JobConfigurationTests.DummyJobConfiguration());
            dispatcherSettings.JobConfigurations.Add(JobConfigurationTests.DummyJobConfiguration());
            dispatcherSettings.JobConfigurations.Add(JobConfigurationTests.DummyJobConfiguration());
            return dispatcherSettings;
        }
    }
}
