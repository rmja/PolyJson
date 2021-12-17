using PolyJson.Converters;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyJson
{
    public class PolyJsonConverterAttribute : JsonConverterAttribute
    {
        public string DiscriminatorPropertyName { get; set; }

        public PolyJsonConverterAttribute(string distriminatorPropertyName)
        {
            DiscriminatorPropertyName = distriminatorPropertyName ?? throw new ArgumentNullException(nameof(distriminatorPropertyName), "The discrimitator property name must be specified.");
        }

        public override JsonConverter CreateConverter(Type typeToConvert)
        {
            // Instantiate converter
            var converterType = typeof(PolyJsonConverter<>).MakeGenericType(typeToConvert);
            var converter = (IPolyJsonConverter)Activator.CreateInstance(converterType);

            // Configure the converter
            converter.DiscriminatorPropertyName = JsonEncodedText.Encode(DiscriminatorPropertyName);

            var subTypes = (PolyJsonConverter.SubTypeAttribute[])typeToConvert.GetCustomAttributes(typeof(PolyJsonConverter.SubTypeAttribute), inherit: false);
            foreach (var attribute in subTypes)
            {
                converter.SubTypes.Add(JsonEncodedText.Encode(attribute.DiscriminatorValue), attribute.SubType);
            }

            return (JsonConverter)converter;
        }
    }
}
