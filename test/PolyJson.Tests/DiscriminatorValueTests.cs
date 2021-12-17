using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace PolyJson.Tests
{
    public class DiscriminatorValueTests
    {
        [Fact]
        public void CanGetDiscriminatorValueFromSubType()
        {
            Assert.Equal("sub", DiscriminatorValue.Get(typeof(SubType)));
            Assert.Equal("sub", DiscriminatorValue.Get(typeof(SubType))); // Cached
        }

        [Fact]
        public void SerializationProducesCorrectDiscriminator()
        {
            // Given

            // When
            var json = JsonSerializer.Serialize(new SubType());

            // Then
            Assert.Equal(@"{""_t"":""sub""}", json);
        }

        [PolyJsonConverter("_t")]
        [PolyJsonConverter.SubType(typeof(SubType), "sub")]
        public class BaseType
        {
            [JsonPropertyName("_t")]
            public string Discriminator => DiscriminatorValue.Get(GetType());
        }

        public class SubType : BaseType
        {

        }
    }
}
