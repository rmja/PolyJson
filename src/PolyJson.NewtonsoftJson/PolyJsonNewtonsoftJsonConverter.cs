using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PolyJson
{
    public class PolyJsonNewtonsoftJsonConverter : JsonConverter
    {
        private readonly ConcurrentDictionary<
            Type,
            PolyJsonConverterAttribute
        > _converterAttributeCache = new();
        private readonly ConcurrentDictionary<Type, Dictionary<string, Type>> _mappingsCache =
            new();

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) =>
            objectType.IsClass && GetPolyJsonConverterAttributeOrNull(objectType) is not null;

        public override object? ReadJson(
            JsonReader reader,
            Type objectType,
            object? existingValue,
            JsonSerializer serializer
        )
        {
            object? target = null;

            if (reader.TokenType != JsonToken.Null)
            {
                var json = JObject.Load(reader);
                var subType = GetSubType(objectType, json);
                target = Activator.CreateInstance(subType)!;
                serializer.Populate(json.CreateReader(), target);
            }

            return target;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new InvalidOperationException();
        }

        private PolyJsonConverterAttribute? GetPolyJsonConverterAttributeOrNull(Type type)
        {
            if (_converterAttributeCache.TryGetValue(type, out var attribute))
            {
                return attribute;
            }

            var attributes = (PolyJsonConverterAttribute[])
                type.GetCustomAttributes(typeof(PolyJsonConverterAttribute), inherit: true);
            if (attributes.Length == 0)
            {
                return null;
            }
            attribute = attributes[0];
            _converterAttributeCache.TryAdd(type, attribute);
            return attribute;
        }

        private PolyJsonConverterAttribute GetPolyJsonConverterAttribute(Type type) =>
            GetPolyJsonConverterAttributeOrNull(type)
            ?? throw new InvalidOperationException("PolyJsonConverterAttribute was not found");

        private Type GetSubType(Type baseType, JObject json)
        {
            var mappings = GetMappings(baseType);
            var attribute = GetPolyJsonConverterAttribute(baseType);
            var value = json.Value<string>(attribute.DiscriminatorPropertyName);

            if (value is null)
            {
                var undefinedType = attribute.UndefinedType ?? attribute.DefaultType;
                if (undefinedType is not null)
                {
                    return undefinedType;
                }
                else
                {
                    throw new JsonException(
                        "Discriminator was not found and no default type is specified"
                    );
                }
            }

            if (mappings.TryGetValue(value, out var subType))
            {
                return subType;
            }

            var unknownType = attribute.UnknownType ?? attribute.DefaultType;
            if (unknownType is not null)
            {
                return unknownType;
            }

            throw new JsonException($"'{value}' is not a valid discriminator value");
        }

        private Dictionary<string, Type> GetMappings(Type baseType)
        {
            if (_mappingsCache.TryGetValue(baseType, out var mappings))
            {
                return mappings;
            }

            mappings = new Dictionary<string, Type>();
            var subTypes = (PolyJsonConverter.SubTypeAttribute[])
                baseType.GetCustomAttributes(
                    typeof(PolyJsonConverter.SubTypeAttribute),
                    inherit: false
                );
            foreach (var attribute in subTypes)
            {
                mappings.Add(attribute.DiscriminatorValue, attribute.SubType);
            }
            _mappingsCache.TryAdd(baseType, mappings);
            return mappings;
        }
    }
}
