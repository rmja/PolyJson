# PolyJson

Attribute based, polymorphic support for System.Text.Json (and optionally Newtonsoft.Json). It supports both serialization and deserialization and is reasonably fast.

## Nuget Packages

| Package name                      | Description				    | Badge |
|-----------------------------------|-------------------------------|-------|
| `PolyJson`                 | Basic types and System.Text.Json support				    | [![PolyJson](https://img.shields.io/nuget/vpre/PolyJson.svg)](https://www.nuget.org/packages/PolyJson) |
| `PolyJson.NewtonsoftJson`              | Optional Newtonsoft.Json support				    | [![PolyJson.NewtonsoftJson](https://img.shields.io/nuget/vpre/PolyJson.NewtonsoftJson.svg)](https://www.nuget.org/packages/PolyJson.NewtonsoftJson) |

## Usage

Decorate the base class with the `PolyJsonConverter` attribute and register the subtypes:

```C#
[PolyJsonConverter("_t")]
//[Newtonsoft.Json.JsonConverter(typeof(PolyJsonNewtonsoftJsonConverter))] // Optional
[PolyJsonConverter.SubType(typeof(Dog), "dog")]
[PolyJsonConverter.SubType(typeof(Cat), "cat")]
public abstract class Animal
{
    [JsonPropertyName("_t")]
    //[Newtonsoft.Json.JsonProperty("_t")] // Optional
    public string Discriminator => DiscriminatorValue.Get(GetType());
    public int Id { get; set; }
}
```

The `PolyJsonConverter` specifies the discriminator field, in this case `_t`. And all possible sub types are registered with their discriminator value.
The `Newtonsoft.Json` support is optional, but uses the same configured subtypes as that configured for `System.Text.Json`.
A sub type does not have any attributes and can for example be:

```C#
public class Dog : Animal
{
    public bool CanBark { get; set; }
}
```
