using Microsoft.VisualStudio.TestPlatform.Utilities;
using NUnit.Framework.Internal;
using System;
using System.IO.Compression;
using System.Text;

namespace ConMaster.Compression.Tests
{
    public class CompressionTests
    {
        readonly byte[][] data = new byte[4][];
        [SetUp]
        public void Setup()
        {
            ReadOnlySpan<byte> text =
            """
                In a small village nestled between rolling hills and a sparkling river, there lived a curious cat named Whiskers. Whiskers loved to explore every nook and cranny of the village, from the bustling marketplace to the quiet, hidden gardens. One sunny afternoon, while chasing a particularly elusive butterfly, Whiskers stumbled upon a mysterious, ancient-looking key buried in the soft earth. Little did Whiskers know, this key would unlock a series of adventures that would change the village forever.

                Whiskers decided to show the key to his best friend, a wise old owl named Oliver. Oliver, with his vast knowledge of the village’s history, recognized the key immediately. “This is the Key of Secrets,” Oliver hooted. “It is said to open a hidden door in the old castle on the hill, leading to untold treasures and forgotten stories.” Intrigued and excited, Whiskers and Oliver set off on a journey to the castle, their hearts filled with anticipation.

                As they approached the castle, they encountered various challenges. The path was overgrown with thorny bushes, and the castle’s entrance was guarded by a riddle-loving fox named Felix. Felix agreed to let them pass only if they could solve his riddle: “I speak without a mouth and hear without ears. I have no body, but I come alive with wind. What am I?” After pondering for a moment, Whiskers exclaimed, “An echo!” Felix, impressed by their cleverness, allowed them to enter the castle.

                Inside the castle, they found a labyrinth of corridors and rooms, each filled with ancient artifacts and dusty tomes. Following Oliver’s guidance, they finally reached a grand door adorned with intricate carvings. With a deep breath, Whiskers inserted the key into the lock and turned it. The door creaked open, revealing a hidden chamber filled with glittering treasures and scrolls detailing the village’s forgotten history. Whiskers and Oliver knew that their discovery would bring new knowledge and prosperity to their beloved village, and they couldn’t wait to share their adventure with everyone.
            """u8;
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new byte[text.Length + 255];
            }
            text.CopyTo(data[0]);
            Assert.Pass();
        }
        [Test]
        public void _LIB_LOAD()
        {
            using (DeflateStream a = new DeflateStream(new MemoryStream(), CompressionLevel.SmallestSize))
            {
                a.Write(data[0]);
                a.CopyTo(new MemoryStream());
            }
            Assert.Pass();
        }
        [Test]
        public void Deflate_Compression_StackAlloc()
        {
            DeflateCompressor compressor = new()
            {
                CompressionLevel = CompressionLevel.SmallestSize,
                MemoryLevel = 9
            };
            Span<byte> output1 = stackalloc byte[data.Length];
            Span<byte> output2 = stackalloc byte[data.Length];
            output1 = compressor.CompressToSpan(data[0], output1);
            output2 = compressor.DecompressToSpan(output1, output2);
            SpanCheck(output1, output2);
        }
        [TestCase((uint)123456789)]
        public void Crc32(uint testCase)
        {
            uint hash = DeflateCompressor.Crc32(data[0], testCase);
            Assert.Pass();
        }
        [Test]
        public void GZip_Compression()
        {
            GZipCompressor compressor = new()
            {
                CompressionLevel = CompressionLevel.Optimal,
                MemoryLevel = 1
            };
            Span<byte> output1 = data[1];
            Span<byte> output2 = data[2];
            output1 = compressor.CompressToSpan(data[0], output1);
            output2 = compressor.DecompressToSpan(output1, output2);
            SpanCheck(output1, output2);
        }
        [Test]
        public void ZLib_Compression()
        {
            ZLibCompressor compressor = new()
            {
                CompressionLevel = CompressionLevel.Optimal,
                MemoryLevel = 1
            };
            Span<byte> output1 = data[1];
            Span<byte> output2 = data[2];
            output1 = compressor.CompressToSpan(data[0], output1);
            output2 = compressor.DecompressToSpan(output1, output2);
            SpanCheck(output1, output2);
        }

        [TestCase(CompressionLevel.Fastest)]
        [TestCase(CompressionLevel.Optimal)]
        [TestCase(CompressionLevel.NoCompression)]
        [TestCase(CompressionLevel.SmallestSize)]
        public void DeflateCompressionTest(CompressionLevel level)
        {
            DeflateCompressor compressor = new()
            {
                CompressionLevel = level,
                MemoryLevel = 9
            };
            Span<byte> output1 = data[1];
            Span<byte> output2 = data[2];
            output1 = compressor.CompressToSpan(data[0], output1);
            output2 = compressor.DecompressToSpan(output1, output2);
            SpanCheck(output1, output2);
        }
        private void SpanCheck(ReadOnlySpan<byte> output, ReadOnlySpan<byte> results)
        {
            if (results.Length != data.Length) Assert.Fail("Decompressed bytes after compression fails.");
            if (!results.SequenceEqual(data[0])) Assert.Fail("Decompression results do not match original source");
            if (output.Length > results.Length) Assert.Fail("compression output has to be lower then decompressed results");
            Assert.Pass("Success");
        }
    }
}