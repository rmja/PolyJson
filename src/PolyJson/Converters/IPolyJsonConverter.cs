using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PolyJson.Converters
{
    internal interface IPolyJsonConverter
    {
        JsonEncodedText DiscriminatorPropertyName { get; set; }
        Type? DefaultType { get; set; }
        Dictionary<JsonEncodedText, Type> SubTypes { get; set; }
    }
}
