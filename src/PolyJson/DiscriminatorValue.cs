using System;
using System.Collections.Generic;
using System.Reflection;

namespace PolyJson
{
    public static class DiscriminatorValue<T>
    {
        public static string Value { get; } = DiscriminatorValue.Get(typeof(T));
    }

    public static class DiscriminatorValue
    {
        private static Dictionary<Type, string> _cache = new();
        private static readonly object _lock = new();

        public static string Get(Type type)
        {
            if (_cache.TryGetValue(type, out var value))
            {
                return value;
            }
            Ensure(type);
            return _cache[type];
        }

        private static void Ensure(Type type)
        {
            var attributedType = GetAttributedType(type);
            if (attributedType is null)
            {
                throw new InvalidOperationException($"The type {type} or any of its base class are attributed with the PolyJsonConverter attribute");
            }

            lock (_lock)
            {
                // Test cache once more within the lock before cloning
                if (_cache.ContainsKey(type))
                {
                    return;
                }

                var cloned = new Dictionary<Type, string>(_cache);
                foreach (var subType in attributedType.GetCustomAttributes<PolyJsonConverter.SubTypeAttribute>())
                {
                    cloned.Add(subType.SubType, subType.DiscriminatorValue);
                }
                _cache = cloned;
            }

            if (!_cache.ContainsKey(type))
            {
                throw new ArgumentException($"The type {type} is not a subtype of {attributedType}");
            }
        }

        private static Type GetAttributedType(Type type)
        {
            while (type is not null && type.GetCustomAttribute<PolyJsonConverterAttribute>() is null)
            {
                type = type.BaseType;
            }
            return type;
        }
    }
}
