using System;
using System.Diagnostics;
using System.Xml;
using PostSharp.Patterns.Diagnostics;
using ReactiveServices.Configuration.TypeResolution;

namespace ReactiveServices.ComputationalUnit.Settings
{
    [Log(AttributeExclude = true)]
    [LogException(AttributeExclude = true)]
    public class RuntimeType
    {
        public Type Type { get; set; }

        public void WriteTo(XmlElement runtimeTypeElement)
        {
            //Type
            Debug.Assert(runtimeTypeElement.OwnerDocument != null, "runtimeTypeElement.OwnerDocument != null");

            var typeElement = runtimeTypeElement.OwnerDocument.CreateElement("Type");

            var typeFullNameAttribute = runtimeTypeElement.OwnerDocument.CreateAttribute("FullName");
            typeFullNameAttribute.Value = Type.FullName;
            typeElement.Attributes.Append(typeFullNameAttribute);

            runtimeTypeElement.AppendChild(typeElement);
        }

        public void ReadFrom(XmlElement runtimeTypeElement)
        {
            //Type
            var typeElement = (XmlElement)runtimeTypeElement.GetElementsByTagName("Type")[0];
            var typeFullName = typeElement.GetAttribute("FullName");

            try
            {
                Type = TypeResolver.Resolve(typeFullName);
            }
            catch
            {
                Type = null;
            }
            if (Type == null)
                throw new ArgumentException(String.Format("The type '{0}' could not be found!'", typeFullName));
        }

        public static RuntimeType From(Type type)
        {
            return new RuntimeType { Type = type };
        }

        public static bool operator ==(RuntimeType type1, RuntimeType type2) 
        {
            if (ReferenceEquals(type1, null) && ReferenceEquals(type2, null)) return true;
            if (ReferenceEquals(type1, null) || ReferenceEquals(type2, null)) return false;
            return type1.Type == type2.Type;
        }

        public static bool operator !=(RuntimeType type1, RuntimeType type2)
        {
            if (ReferenceEquals(type1, null) && ReferenceEquals(type2, null)) return false;
            if (ReferenceEquals(type1, null) || ReferenceEquals(type2, null)) return true;
            return type1.Type != type2.Type;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj, this);
        }
    }
}
