using System;
using System.Diagnostics;
using System.Reflection;
using System.Xml;
using PostSharp.Patterns.Diagnostics;
using ReactiveServices.Configuration.TypeResolution;

namespace ReactiveServices.ComputationalUnit.Settings
{
    [Log(AttributeExclude = true)]
    [LogException(AttributeExclude = true)]
    public class ReferenceAssembly
    {
        public AssemblyName AssemblyName { get; set; }

        public void WriteTo(XmlElement referenceAssemblyElement)
        {
            Debug.Assert(referenceAssemblyElement.OwnerDocument != null, "runtimeTypeElement.OwnerDocument != null");

            //AssemblyName
            var assemblyNameElement = referenceAssemblyElement.OwnerDocument.CreateAttribute("AssemblyName");
            assemblyNameElement.Value = AssemblyName.FullName;
            referenceAssemblyElement.Attributes.Append(assemblyNameElement);
        }

        public void ReadFrom(XmlElement referenceAssemblyElement)
        {
            //AssemblyName
            var assemblyName = referenceAssemblyElement.GetAttribute("AssemblyName");
            var assembly = AssemblyResolver.Resolve(assemblyName);
            AssemblyName = assembly.GetName();
            if (AssemblyName == null)
                throw new ArgumentException(String.Format("The assembly '{0}' could not be found!'", assemblyName));
        }
    }
}
