using System;

namespace PolyJson
{
    public class PolyJsonConverter
    {
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
        public class SubTypeAttribute : Attribute
        {
            public Type SubType { get; set; }
            public string DiscriminatorValue { get; set; }

            public SubTypeAttribute(Type subType, string discriminatorValue)
            {
                SubType = subType;
                DiscriminatorValue = discriminatorValue;
            }
        }
    }
}
