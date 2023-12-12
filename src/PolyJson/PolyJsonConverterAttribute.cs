using PolyJson.Converters;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyJson
{
    public class PolyJsonConverterAttribute : JsonConverterAttribute
    {
        public string DiscriminatorPropertyName { get; set; }

        /// <summary>
        /// The type to use when no discriminator property is found or its value holds a not configured value.
        /// </summary>
        public Type? DefaultType { get; set; }

        /// <summary>
        /// The type to use when no discriminator property is found. <see cref="DefaultType"/> is used if not specified.
        /// </summary>
        public Type? UndefinedType { get; set; }

        /// <summary>
        /// The type to use when the discriminator property is defined but holds a not configured value. <see cref="DefaultType"/> is used if not specified.
        /// </summary>
        public Type? UnknownType { get; set; }

        public PolyJsonConverterAttribute(string distriminatorPropertyName)
        {
            DiscriminatorPropertyName =
                distriminatorPropertyName
                ?? throw new ArgumentNullException(
                    nameof(distriminatorPropertyName),
                    "The discrimitator property name must be specified."
                );
        }

        public override JsonConverter CreateConverter(Type typeToConvert)
        {
            var undefinedType = UndefinedType ?? DefaultType;
            var unknownType = UnknownType ?? DefaultType;

            if (typeToConvert == undefinedType || typeToConvert == unknownType)
            {
                throw new InvalidOperationException(
                    "The undefined or unknown types cannot be the same as the type decorated with the JsonConverter attribute"
                );
            }

            // Instantiate converter
            var converterType = typeof(PolyJsonConverter<>).MakeGenericType(typeToConvert);
            var converter = (IPolyJsonConverter)Activator.CreateInstance(converterType)!;

            // Configure the converter
            converter.DiscriminatorPropertyName = JsonEncodedText.Encode(DiscriminatorPropertyName);
            converter.UndefinedOrDefaultType = undefinedType;
            converter.UnknownOrDefaultType = unknownType;

            var subTypes = (PolyJsonConverter.SubTypeAttribute[])
                typeToConvert.GetCustomAttributes(
                    typeof(PolyJsonConverter.SubTypeAttribute),
                    inherit: false
                );
            foreach (var attribute in subTypes)
            {
                converter.SubTypes.Add(
                    JsonEncodedText.Encode(attribute.DiscriminatorValue),
                    attribute.SubType
                );
            }

            return (JsonConverter)converter;
        }
    }
}
