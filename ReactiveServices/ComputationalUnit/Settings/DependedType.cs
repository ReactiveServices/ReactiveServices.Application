using System.Diagnostics;
using PostSharp.Patterns.Diagnostics;
using ReactiveServices.Configuration;
using System.Xml;
using System;

namespace ReactiveServices.ComputationalUnit.Settings
{
    [Log(AttributeExclude = true)]
    [LogException(AttributeExclude = true)]
    public class DependedType
    {
        public DependedType()
        {
            Lifestyle = Lifestyle.Transient;
        }

        public RuntimeType AbstractType { get; set; }
        public RuntimeType ConcreteType { get; set; }
        public Lifestyle Lifestyle { get; set; }

        public void WriteTo(XmlElement typeResolutionElement)
        {
            //AbstractType
            Debug.Assert(typeResolutionElement.OwnerDocument != null, "typeResolutionElement.OwnerDocument != null");
            var abstractTypeElement = typeResolutionElement.OwnerDocument.CreateElement("AbstractType");
            AbstractType.WriteTo(abstractTypeElement);
            typeResolutionElement.AppendChild(abstractTypeElement);

            //ConcreteType
            Debug.Assert(typeResolutionElement.OwnerDocument != null, "typeResolutionElement.OwnerDocument != null");
            var concreteTypeElement = typeResolutionElement.OwnerDocument.CreateElement("ConcreteType");
            ConcreteType.WriteTo(concreteTypeElement);
            typeResolutionElement.AppendChild(concreteTypeElement);

            //Lifestyle
            Debug.Assert(typeResolutionElement.OwnerDocument != null, "typeResolutionElement.OwnerDocument != null");
            var lifeStyleElement = typeResolutionElement.OwnerDocument.CreateAttribute("Lifestyle");
            lifeStyleElement.Value = Lifestyle.ToString();
            typeResolutionElement.Attributes.Append(lifeStyleElement);
        }

        public void ReadFrom(XmlElement typeResolutionElement)
        {
            //AbstractType
            Debug.Assert(typeResolutionElement != null, "typeResolutionElement != null");
            var abstractTypeElement = (XmlElement)typeResolutionElement.GetElementsByTagName("AbstractType")[0];
            AbstractType = new RuntimeType();
            AbstractType.ReadFrom(abstractTypeElement);

            //ConcreteType
            Debug.Assert(typeResolutionElement != null, "typeResolutionElement != null");
            var concreteTypeElement = (XmlElement)typeResolutionElement.GetElementsByTagName("ConcreteType")[0];
            ConcreteType = new RuntimeType();
            ConcreteType.ReadFrom(concreteTypeElement);

            //Lifestyle
            if (typeResolutionElement.HasAttribute("Lifestyle"))
                Lifestyle = (Lifestyle)Enum.Parse(typeof(Lifestyle), typeResolutionElement.GetAttribute("Lifestyle"));
        }
    }
}
