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
    public class JobConfigurationTests
    {
        [Test]
        [Category("stable")][Category("fast")]
        public void TestWriteToXmlAndReadFromXmlOnJobConfiguration()
        {
            DependencyResolver.Reset();
            DependencyResolver.Initialize();

            var jobConfiguration = DummyJobConfiguration();

            var firstWrite = new XmlDocument();
            firstWrite.AppendChild(firstWrite.CreateElement("JobConfiguration"));
            jobConfiguration.WriteTo(firstWrite.DocumentElement);

            var anotherJobConfiguration = new JobConfiguration();
            anotherJobConfiguration.ReadFrom(firstWrite.DocumentElement);

            var secondWrite = new XmlDocument();
            secondWrite.AppendChild(secondWrite.CreateElement("JobConfiguration"));
            anotherJobConfiguration.WriteTo(secondWrite.DocumentElement);

            var firstString = new StringBuilder();
            var firstText = XmlWriter.Create(firstString);
            firstWrite.Save(firstText);

            var secondString = new StringBuilder();
            var secondText = XmlWriter.Create(secondString);
            secondWrite.Save(secondText);

            secondString.ToString().Should().BeEquivalentTo(firstString.ToString());
        }

        internal static JobConfiguration DummyJobConfiguration()
        {
            var jobConfiguration = new JobConfiguration
            {
                RequestMaxAttempts = 9,
                RequestTimeout = TimeSpan.FromMilliseconds(999),
                JobAndWorkerType =
                {
                    JobType = RuntimeType.From(typeof (DummyJob)),
                    WorkerType = RuntimeType.From(typeof (DummyWorker))
                }
            };
            return jobConfiguration;
        }
    }
}
