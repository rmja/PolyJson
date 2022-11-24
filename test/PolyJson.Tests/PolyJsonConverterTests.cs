using PolyJson.Converters;
using System;
using System.Buffers;
using System.Text;
using System.Text.Json;
using Xunit;

namespace PolyJson.Tests
{
    public class PolyJsonConverterTests
    {
        [Fact]
        public void CanPartialRead()
        {
            // Given
            var converter = new PolyJsonConverter<Model>()
            {
                DiscriminatorPropertyName = JsonEncodedText.Encode("Discriminator"),
                SubTypes =
                {
                    [JsonEncodedText.Encode("sub")] = typeof(SubModel)
                }
            };
            var options = new JsonSerializerOptions() { Converters = { converter } };

            var model = new SubModel() { Discriminator = "sub", Name = "name", Large = new() };
            var json = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(model, options)).AsMemory();

            // When
            var reader = new Utf8JsonReader(new ReadOnlySequence<byte>(json[..16]), isFinalBlock: false, new JsonReaderState());
            var first = converter.Read(ref reader, typeof(Model), options);
            
            reader = new Utf8JsonReader(new ReadOnlySequence<byte>(json).Slice(reader.Position), isFinalBlock: true, reader.CurrentState);
            var second = converter.Read(ref reader, typeof(Model), options);

            // Then
            Assert.Null(first);
            Assert.NotNull(second);
        }

        [PolyJsonConverter("Discriminator")]
        [PolyJsonConverter.SubType(typeof(SubModel), "sub")]
        abstract class Model
        {
            public LargeNestedModel? Large { get; set; }
            public string? Name { get; set; }

            // Must be later so that we do not immediately find the discriminator
            public string? Discriminator { get; set; }
        }

        class LargeNestedModel
        {
            public int SomeLargeNestedModel { get; set; }
            public int ThatWeAreLikelyToSkip { get; set; }
        }

        class SubModel : Model
        {

        }
    }
}
