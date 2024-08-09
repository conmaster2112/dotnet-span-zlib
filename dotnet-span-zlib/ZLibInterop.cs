﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ConMaster.Compression
{
    /// <summary>
    /// <p>ZLib can accept any integer value between 0 and 9 (inclusive) as a valid compression level parameter:
    /// 1 gives best speed, 9 gives best compression, 0 gives no compression at all (the input data is simply copied a block at a time).
    /// <code>CompressionLevel.DefaultCompression</code> = -1 requests a default compromise between speed and compression
    /// (currently equivalent to level 6).</p>
    ///
    /// <p><strong>How to choose a compression level:</strong></p>
    ///
    /// <p>The names <code>NoCompression</code>, <code>BestSpeed</code>, <code>DefaultCompression</code>, <code>BestCompression</code> are taken over from
    /// the corresponding ZLib definitions, which map to our public NoCompression, Fastest, Optimal, and SmallestSize respectively.</p>
    /// <p><em>Optimal Compression:</em></p>
    /// <p><code>ZLibNative.CompressionLevel compressionLevel = ZLibNative.CompressionLevel.DefaultCompression;</code> <br />
    ///    <code>int windowBits = 15;  // or -15 if no headers required</code> <br />
    ///    <code>int memLevel = 8;</code> <br />
    ///    <code>ZLibNative.CompressionStrategy strategy = ZLibNative.CompressionStrategy.DefaultStrategy;</code> </p>
    ///
    ///<p><em>Fastest compression:</em></p>
    ///<p><code>ZLibNative.CompressionLevel compressionLevel = ZLibNative.CompressionLevel.BestSpeed;</code> <br />
    ///   <code>int windowBits = 15;  // or -15 if no headers required</code> <br />
    ///   <code>int memLevel = 8; </code> <br />
    ///   <code>ZLibNative.CompressionStrategy strategy = ZLibNative.CompressionStrategy.DefaultStrategy;</code> </p>
    ///
    /// <p><em>No compression (even faster, useful for data that cannot be compressed such some image formats):</em></p>
    /// <p><code>ZLibNative.CompressionLevel compressionLevel = ZLibNative.CompressionLevel.NoCompression;</code> <br />
    ///    <code>int windowBits = 15;  // or -15 if no headers required</code> <br />
    ///    <code>int memLevel = 7;</code> <br />
    ///    <code>ZLibNative.CompressionStrategy strategy = ZLibNative.CompressionStrategy.DefaultStrategy;</code> </p>
    ///
    /// <p><em>Smallest Size Compression:</em></p>
    /// <p><code>ZLibNative.CompressionLevel compressionLevel = ZLibNative.CompressionLevel.BestCompression;</code> <br />
    ///    <code>int windowBits = 15;  // or -15 if no headers required</code> <br />
    ///    <code>int memLevel = 8;</code> <br />
    ///    <code>ZLibNative.CompressionStrategy strategy = ZLibNative.CompressionStrategy.DefaultStrategy;</code> </p>
    /// </summary>
    internal enum CompressionLevel : int
    {
        NoCompression = 0,
        BestSpeed = 1,
        DefaultCompression = -1,
        BestCompression = 9
    }

    /// <summary>
    /// <p><strong>From the ZLib manual:</strong></p>
    /// <p><code>CompressionStrategy</code> is used to tune the compression algorithm.<br />
    /// Use the value <code>DefaultStrategy</code> for normal data, <code>Filtered</code> for data produced by a filter (or predictor),
    /// <code>HuffmanOnly</code> to force Huffman encoding only (no string match), or <code>Rle</code> to limit match distances to one
    /// (run-length encoding). Filtered data consists mostly of small values with a somewhat random distribution. In this case, the
    /// compression algorithm is tuned to compress them better. The effect of <code>Filtered</code> is to force more Huffman coding and]
    /// less string matching; it is somewhat intermediate between <code>DefaultStrategy</code> and <code>HuffmanOnly</code>.
    /// <code>Rle</code> is designed to be almost as fast as <code>HuffmanOnly</code>, but give better compression for PNG image data.
    /// The strategy parameter only affects the compression ratio but not the correctness of the compressed output even if it is not set
    /// appropriately. <code>Fixed</code> prevents the use of dynamic Huffman codes, allowing for a simpler decoder for special applications.</p>
    ///
    /// <p><strong>For .NET Framework use:</strong></p>
    /// <p>We have investigated compression scenarios for a bunch of different frequently occurring compression data and found that in all
    /// cases we investigated so far, <code>DefaultStrategy</code> provided best results</p>
    /// <p>See also: How to choose a compression level (in comments to <code>CompressionLevel</code>.</p>
    /// </summary>
    internal enum CompressionStrategy : int
    {
        DefaultStrategy = 0
    }

    /// <summary>
    /// In version 1.2.3, ZLib provides on the <code>Deflated</code>-<code>CompressionMethod</code>.
    /// </summary>
    internal enum CompressionMethod : int
    {
        Deflated = 8
    }

    internal enum ZLibErrorCode : int
    {
        Ok = 0,
        StreamEnd = 1,
        StreamError = -2,
        DataError = -3,
        MemError = -4,
        BufError = -5,
        VersionError = -6
    }
    internal enum ZLibFlushCode : int
    {
        NoFlush = 0,
        SyncFlush = 2,
        Finish = 4,
        Block = 5
    }
    /// <summary>
    /// ZLib stream descriptor data structure
    /// Do not construct instances of <code>ZStream</code> explicitly.
    /// Always use <code>ZLibNative.DeflateInit2_</code> or <code>ZLibNative.InflateInit2_</code> instead.
    /// Those methods will wrap this structure into a <code>SafeHandle</code> and thus make sure that it is always disposed correctly.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct ZStream
    {
        public unsafe ZStream(ReadOnlySpan<byte> source, Span<byte> output)
        {
            nextIn = (nint)Unsafe.AsPointer(ref MemoryMarshal.GetReference(source));
            availIn = (uint)source.Length;
            nextOut = (nint)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output));
            availOut = (uint)output.Length;
        }
        internal nint nextIn;  //Bytef    *next_in;  /* next input byte */
        internal nint nextOut; //Bytef    *next_out; /* next output byte should be put there */

        internal IntPtr msg;     //char     *msg;      /* last error message, NULL if no error */

        private readonly IntPtr internalState;    //internal state that is not visible to managed code

        internal uint availIn;   //uInt     avail_in;  /* number of bytes available at next_in */
        internal uint availOut;  //uInt     avail_out; /* remaining free space at next_out */
    }
    internal partial class ZLibInterop
    {
        public const string LIB_NAME = "System.IO.Compression.Native";
        /// <summary>
        /// <p><strong>From the ZLib manual:</strong></p>
        /// <p>ZLib's <code>windowBits</code> parameter is the base two logarithm of the window size (the size of the history buffer).
        /// It should be in the range 8..15 for this version of the library. Larger values of this parameter result in better compression
        /// at the expense of memory usage. The default value is 15 if deflateInit is used instead.<br /></p>
        /// <strong>Note</strong>:
        /// <code>windowBits</code> can also be -8..-15 for raw deflate. In this case, -windowBits determines the window size.
        /// <code>Deflate</code> will then generate raw deflate data with no ZLib header or trailer, and will not compute an adler32 check value.<br />
        /// <p>See also: How to choose a compression level (in comments to <code>CompressionLevel</code>.</p>
        /// </summary>
        public const int Deflate_DefaultWindowBits = -15; // Legal values are 8..15 and -8..-15. 15 is the window size,
                                                          // negative val causes deflate to produce raw deflate data (no zlib header).

        /// <summary>
        /// <p><strong>From the ZLib manual:</strong></p>
        /// <p>ZLib's <code>windowBits</code> parameter is the base two logarithm of the window size (the size of the history buffer).
        /// It should be in the range 8..15 for this version of the library. Larger values of this parameter result in better compression
        /// at the expense of memory usage. The default value is 15 if deflateInit is used instead.<br /></p>
        /// </summary>
        public const int ZLib_DefaultWindowBits = 15;

        /// <summary>
        /// <p>Zlib's <code>windowBits</code> parameter is the base two logarithm of the window size (the size of the history buffer).
        /// For GZip header encoding, <code>windowBits</code> should be equal to a value between 8..15 (to specify Window Size) added to
        /// 16. The range of values for GZip encoding is therefore 24..31.
        /// <strong>Note</strong>:
        /// The GZip header will have no file name, no extra data, no comment, no modification time (set to zero), no header crc, and
        /// the operating system will be set based on the OS that the ZLib library was compiled to. <code>ZStream.adler</code>
        /// is a crc32 instead of an adler32.</p>
        /// </summary>
        public const int GZip_DefaultWindowBits = 31;

        /// <summary>
        /// <p><strong>From the ZLib manual:</strong></p>
        /// <p>The <code>memLevel</code> parameter specifies how much memory should be allocated for the internal compression state.
        /// <code>memLevel</code> = 1 uses minimum memory but is slow and reduces compression ratio; <code>memLevel</code> = 9 uses maximum
        /// memory for optimal speed. The default value is 8.</p>
        /// <p>See also: How to choose a compression level (in comments to <code>CompressionLevel</code>.</p>
        /// </summary>
        public const int Deflate_DefaultMemLevel = 8;     // Memory usage by deflate. Legal range: [1..9]. 8 is ZLib default.
                                                          // More is faster and better compression with more memory usage.
        public const int Deflate_NoCompressionMemLevel = 7;

        public const byte GZip_Header_ID1 = 31;
        public const byte GZip_Header_ID2 = 139;


        [DllImport(LIB_NAME, EntryPoint = "CompressionNative_DeflateInit2_")]
        internal static extern ZLibErrorCode DeflateInit2_(
            ref ZStream stream,
            CompressionLevel level,
            CompressionMethod method,
            int windowBits,
            int memLevel,
            CompressionStrategy strategy);

        [DllImport(LIB_NAME, EntryPoint = "CompressionNative_Deflate")]
        internal static extern ZLibErrorCode Deflate(ref ZStream stream, ZLibFlushCode flush);

        [DllImport(LIB_NAME, EntryPoint = "CompressionNative_DeflateEnd")]
        internal static extern ZLibErrorCode DeflateEnd(ref ZStream stream);

        [DllImport(LIB_NAME, EntryPoint = "CompressionNative_InflateInit2_")]
        internal static extern ZLibErrorCode InflateInit2_(ref ZStream stream, int windowBits);

        [DllImport(LIB_NAME, EntryPoint = "CompressionNative_Inflate")]
        internal static extern ZLibErrorCode Inflate(ref ZStream stream, ZLibFlushCode flush);

        [DllImport(LIB_NAME, EntryPoint = "CompressionNative_InflateEnd")]
        internal static extern ZLibErrorCode InflateEnd(ref ZStream stream);

        [DllImport(LIB_NAME, EntryPoint = "CompressionNative_Crc32")]
        internal static extern uint CRC_32(uint crc, ref byte buffer, int len);
    }

}
