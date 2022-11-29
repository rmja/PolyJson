using PolyJson.Converters;
using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit;

namespace PolyJson.Tests
{
    public class PolyJsonConverterTests
    {
        [Fact]
        public async Task CanPartialRead()
        {
            // Given
            var options = new JsonSerializerOptions() { DefaultBufferSize = 16 };

            var models = Enumerable.Range(0, 1000).Select(_ => new DefaultModel() { Name = "name" }).ToArray();
            var json = JsonSerializer.Serialize(models);
            var jsonBytes = Encoding.UTF8.GetBytes(json).AsMemory();

            var stream = new ChunkStream(jsonBytes, chunkSize: options.DefaultBufferSize);

            // When
            var deserialized = await JsonSerializer.DeserializeAsync<Model[]>(stream, options);
        }

        [PolyJsonConverter("Discriminator", DefaultType = typeof(DefaultModel))]
        [PolyJsonConverter.SubType(typeof(SubModel), "sub")]
        abstract class Model
        {
            public LargeNestedModel A { get; set; } = new();
            public LargeNestedModel B { get; set; } = new();
            public LargeNestedModel C { get; set; } = new();
            public string? Name { get; set; }

            // Must be later so that we do not immediately find the discriminator
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? Discriminator { get; set; }
        }

        class DefaultModel : Model
        {

        }

        class LargeNestedModel
        {
            public int SomeLargeNestedModel { get; set; }
            public int ThatWeAreLikelyToSkip { get; set; }
        }

        class SubModel : Model
        {

        }

        class ChunkStream : Stream
        {
            private readonly Memory<byte> _buffer;
            private readonly int _chunkSize;
            private int _position = 0;

            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => throw new NotImplementedException();

            public override long Length => _buffer.Length;

            public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public ChunkStream(Memory<byte> buffer, int chunkSize)
            {
                _buffer = buffer;
                _chunkSize = chunkSize;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                var length = Math.Min(_buffer.Length - _position, Math.Min(count, _chunkSize));
                _buffer.Slice(_position, length).CopyTo(buffer.AsMemory(offset, count));
                _position += length;
                return length;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Flush()
            {
                throw new NotImplementedException();
            }
        }
    }
}
