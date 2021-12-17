using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace PolyJson.Tests
{
    public class SystemTextJsonSerializerTests : SerializerTests
    {
        protected override string Serialize(object value) => JsonSerializer.Serialize(value);

        protected override T Deserialize<T>(string value) => JsonSerializer.Deserialize<T>(value);
    }

    public class NewtonsoftJsonSerializerTests : SerializerTests
    {
        protected override string Serialize(object value) => Newtonsoft.Json.JsonConvert.SerializeObject(value);

        protected override T Deserialize<T>(string value) => Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
    }

    public abstract class SerializerTests
    {
        [Fact]
        public void CanSerializeAndDeserialize()
        {
            // Given
            var dog = new Dog()
            {
                Id = 1,
                CanBark = true
            };

            var cow = new Cat()
            {
                Id = 2,
                Lives = 9
            };
            var animals = new List<Animal> { dog, cow };

            // When
            var json = Serialize(animals);
            var deserializedAnimals = Deserialize<List<Animal>>(json);

            // Then
            Assert.Equal("[{\"CanBark\":true,\"_t\":\"dog\",\"Id\":1},{\"Lives\":9,\"_t\":\"cat\",\"Id\":2}]", json);

            Assert.Equal(2, deserializedAnimals.Count);
            var deserializedDog = Assert.IsType<Dog>(deserializedAnimals[0]);
            Assert.Equal(dog.Id, deserializedDog.Id);
            Assert.Equal(dog.CanBark, deserializedDog.CanBark);
            var deserializedCow = Assert.IsType<Cat>(deserializedAnimals[1]);
            Assert.Equal(cow.Id, deserializedCow.Id);
            Assert.Equal(cow.Lives, deserializedCow.Lives);
        }

        protected abstract string Serialize(object value);

        protected abstract T Deserialize<T>(string value);
    }

    [PolyJsonConverter("_t")]
    [Newtonsoft.Json.JsonConverter(typeof(PolyJsonNewtonsoftJsonConverter))]
    [PolyJsonConverter.SubType(typeof(Dog), "dog")]
    [PolyJsonConverter.SubType(typeof(Cat), "cat")]
    public abstract class Animal
    {
        [JsonPropertyName("_t")]
        [Newtonsoft.Json.JsonProperty("_t")]
        public string Discriminator { get; }
        public int Id { get; set; }

        protected Animal(string discriminator) => Discriminator = discriminator;
    }

    public class Dog : Animal
    {
        public bool CanBark { get; set; }

        public Dog() : base("dog")
        {
        }
    }

    public class Cat : Animal
    {
        public int Lives { get; set; }

        public Cat() : base("cat")
        {
        }
    }
}
