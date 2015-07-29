using System;
using System.Numerics;
using Newtonsoft.Json;

namespace ReactiveServices.ComputationalUnit.Dispatching.Tests
{
    public class JavaScriptComplexConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Complex);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException(
                    String.Format("Unexpected token or value when parsing version. Token: {0}, Value: {1}",
                        reader.TokenType, reader.Value));

            double real = 0;
            double imaginary = 0;

            try
            {
                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonToken.PropertyName:
                            var propertyName = reader.Value.ToString();
                            if ((propertyName == "Real") || (propertyName == "Imaginary"))
                            {
                                if (!reader.Read())
                                    throw new JsonSerializationException("Unexpected end when reading Complex.");

                                try
                                {
                                    switch (propertyName)
                                    {
                                        case "Real":
                                            real = Double.Parse(reader.Value.ToString());
                                            break;
                                        case "Imaginary":
                                            imaginary = Double.Parse(reader.Value.ToString());
                                            break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw new JsonSerializationException("Invalid value when reading Complex.", ex);
                                }
                            }
                            break;

                        case JsonToken.EndObject:
                            var c = new Complex(real, imaginary);
                            return c;
                    }
                }

                throw new JsonSerializationException("Unexpected end when reading Complex.");
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException(String.Format("Error parsing version string: {0}", reader.Value), ex);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException("JavaScriptComplexConverter only supports read operations!");
        }
    }
}