using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolyJson.Converters
{
    internal class PolyJsonConverter<T> : JsonConverter<T>, IPolyJsonConverter
    {
        public JsonEncodedText DiscriminatorPropertyName { get; set; }
        public Type? DefaultType { get; set; }
        public Dictionary<JsonEncodedText, Type> SubTypes { get; set; } = new();

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Create a reader copy that we can use to find the discriminator value without advancing the original reader
            var nestedReader = reader;

            while (nestedReader.Read())
            {
                if (nestedReader.TokenType == JsonTokenType.PropertyName &&
                    nestedReader.ValueTextEquals(DiscriminatorPropertyName.EncodedUtf8Bytes))
                {
                    // Advance the reader to the property value
                    nestedReader.Read();

                    // Resolve the type from the discriminator value
                    var subType = GetSubType(ref nestedReader);

                    // Perform the actual deserialization with the original reader
                    return (T)JsonSerializer.Deserialize(ref reader, subType, options)!;
                }
                else if (nestedReader.TokenType == JsonTokenType.StartObject || nestedReader.TokenType == JsonTokenType.StartArray)
                {
                    // Skip until TokenType is EndObject/EndArray
                    // Skip() always throws if IsFinalBlock == false, even when it could actually skip.
                    // We therefore invoke TrySkip(), and then call Skip() if it in fact coult not skip just to throw the exception
                    // For reference, see:
                    // https://stackoverflow.com/questions/63038334/how-do-i-handle-partial-json-in-a-jsonconverter-while-using-deserializeasync-on
                    // https://github.com/dotnet/runtime/blob/main/src/libraries/System.Text.Json/src/System/Text/Json/Reader/Utf8JsonReader.cs#L303-L310
                    if (!nestedReader.TrySkip())
                    {
                        nestedReader.Skip();
                    }
                }
            }

            if (DefaultType is not null)
            {
                return (T)JsonSerializer.Deserialize(ref reader, DefaultType, options)!;
            }

            throw new JsonException($"Unable to find discriminator property '{DiscriminatorPropertyName}'");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize<object>(writer, value!, options);
        }

        private Type GetSubType(ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Expected string discriminator value, got '{reader.TokenType}'");
            }

            foreach (var (subValue, subType) in SubTypes)
            {
                if (reader.ValueTextEquals(subValue.EncodedUtf8Bytes))
                {
                    return subType;
                }
            }

            throw new JsonException($"'{reader.GetString()}' is not a valid discriminator value");
        }
    }
}
