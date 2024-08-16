using System;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ConMaster.Compression
{
    /// <summary>
    /// Internal Compressor, Should be internaly inherited only
    /// </summary>
    public abstract class InternalCompressor
    {
        internal InternalCompressor(int _windowBits)
        {
            this._windowBits = _windowBits;
        }
        internal int _memoryLevel = ZLibInterop.Deflate_DefaultMemLevel;
        internal CompressionLevel _compressionLevel;
        internal System.IO.Compression.CompressionLevel _public_Level;
        internal int _windowBits;
        /// <summary>
        /// Compression Level
        /// </summary>
        public System.IO.Compression.CompressionLevel CompressionLevel
        {
            get => _public_Level;
            set
            {
                _public_Level = value;
                switch (value)
                {
                    case System.IO.Compression.CompressionLevel.Optimal:
                        _compressionLevel = Compression.CompressionLevel.DefaultCompression;
                        break;
                    case System.IO.Compression.CompressionLevel.Fastest:
                        _compressionLevel = Compression.CompressionLevel.BestSpeed;
                        break;
                    case System.IO.Compression.CompressionLevel.NoCompression:
                        _compressionLevel = Compression.CompressionLevel.NoCompression;
                        break;
                    case System.IO.Compression.CompressionLevel.SmallestSize:
                        _compressionLevel = Compression.CompressionLevel.BestCompression;
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// Memory Level 
        /// [1..9]
        /// 1 - Lowest memory usage, slow - low compression ratio
        /// 9 - Hight memory usage, fast - hight compression ratio
        /// </summary>
        public byte MemoryLevel { get => (byte)(_memoryLevel & 0xff); set => _memoryLevel = value > 9 ? 9 : value; }

        /// <summary>
        /// Compression Algorithm
        /// </summary>
        /// <param name="source">Source buffer to operate compression on</param>
        /// <param name="destination">Target buffer to write compression results to</param>
        /// <returns>The length of compressed data in destination</returns>
        public abstract int Compress(ReadOnlySpan<byte> source, Span<byte> destination);
        /// <summary>
        /// Compression Algorithm
        /// </summary>
        /// <param name="source">Source buffer to operate compression on</param>
        /// <param name="destination">Target buffer to write compression results to</param>
        /// <returns>Span with length of compressed data, slice from destination span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> CompressToSpan(ReadOnlySpan<byte> source, Span<byte> destination) => destination.Slice(0, Compress(source, destination));
        /// <summary>
        /// Compression Algorithm
        /// </summary>
        /// <param name="source">Source buffer to operate compression on</param>
        /// <param name="destination">Target buffer to write compression results to</param>
        /// <param name="bytesWritten">Number of bytes written to destination span</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Compress(ReadOnlySpan<byte> source, Span<byte> destination, out int bytesWritten) => bytesWritten = Compress(source, destination);
        /// <summary>
        /// Compression Algorithm
        /// </summary>
        /// <param name="source">Source buffer to operate compression on</param>
        /// <param name="destination">Target buffer to write compression results to</param>
        /// <param name="bytesWritten">Number of bytes written to destination span, returns -1 when fails</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCompress(ReadOnlySpan<byte> source, Span<byte> destination, out int bytesWritten)
        {
            try
            {
                bytesWritten = Compress(source, destination);
            }
            catch (Exception)
            {
                bytesWritten = -1;
                return false;
            }
            return true;
        }
        /// <summary>
        /// Dempression Algorithm
        /// </summary>
        /// <param name="source">Source buffer to operate decompression on</param>
        /// <param name="destination">Target buffer to write decompression results to</param>
        /// <returns>The length of decompressed data in destination</returns>
        public abstract int Decompress(ReadOnlySpan<byte> source, Span<byte> destination);
        /// <summary>
        /// Dempression Algorithm
        /// </summary>
        /// <param name="source">Source buffer to operate decompression on</param>
        /// <param name="destination">Target buffer to write decompression results to</param>
        /// <returns>Span with length of decompressed data, slice from destination span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> DecompressToSpan(ReadOnlySpan<byte> source, Span<byte> destination) => destination.Slice(0, Decompress(source, destination));
        /// <summary>
        /// Dempression Algorithm
        /// </summary>
        /// <param name="source">Source buffer to operate decompression on</param>
        /// <param name="destination">Target buffer to write decompression results to</param>
        /// <param name="bytesWritten">Number of bytes written to destination span</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Decompress(ReadOnlySpan<byte> source, Span<byte> destination, out int bytesWritten) => bytesWritten = Decompress(source, destination);
        /// <summary>
        /// Dempression Algorithm
        /// </summary>
        /// <param name="source">Source buffer to operate decompression on</param>
        /// <param name="destination">Target buffer to write decompression results to</param>
        /// <param name="bytesWritten">Number of bytes written to destination span. returns -1 when fails</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDecompress(ReadOnlySpan<byte> source, Span<byte> destination, out int bytesWritten)
        {
            try
            {
                bytesWritten = Decompress(source, destination);
            }
            catch (Exception)
            {
                bytesWritten = -1;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Basic C3C hashing algorithm
        /// </summary>
        /// <param name="source">Source buffer to operate hasing on</param>
        /// <param name="token">Source token to use for hasing</param>
        /// <returns>Hashing results</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Crc32(ReadOnlySpan<byte> source, uint token)
        {
            return ZLibInterop.CRC_32(token, ref MemoryMarshal.GetReference(source), source.Length);
        }

    }
    /// <summary>
    /// Raw Deflate Compression Algorithm - No Headers
    /// </summary>
    public class DeflateCompressor : InternalCompressor
    {
        /// <summary>
        /// Raw Deflate Compression Algorithm - No Headers
        /// </summary>
        public DeflateCompressor() : base(ZLibInterop.Deflate_DefaultWindowBits) { }
        internal DeflateCompressor(int windowBits) : base(windowBits) { }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int Compress(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            return InternalDeflate(source, destination);
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int Decompress(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            return InternalInflate(source, destination);
        }
        internal unsafe int InternalDeflate(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            ZStream s = new(source, destination);
            if (
                    ZLibInterop.DeflateInit2_(
                        ref s, _compressionLevel, CompressionMethod.Deflated,
                        _windowBits, _memoryLevel, CompressionStrategy.DefaultStrategy
                    ) != ZLibErrorCode.Ok
                ) throw new Exception("Initialization Failed");

            if (ZLibInterop.Deflate(ref s, ZLibFlushCode.NoFlush) != ZLibErrorCode.Ok) throw new Exception("Unexpected error: " + Marshal.PtrToStringUTF8(s.msg));
            if ((int)ZLibInterop.Deflate(ref s, ZLibFlushCode.Finish) < 0) throw new Exception("Unexpected error: " + Marshal.PtrToStringUTF8(s.msg));
            if (ZLibInterop.DeflateEnd(ref s) != ZLibErrorCode.Ok) throw new Exception("Unexpected ending: " + Marshal.PtrToStringUTF8(s.msg));
            return destination.Length - (int)s.availOut;
        }
        internal unsafe int InternalInflate(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            ZStream s = new(source, destination);
            if (ZLibInterop.InflateInit2_(ref s, _windowBits) != ZLibErrorCode.Ok)
                throw new Exception("Initialization Failed");

            ZLibErrorCode err = ZLibInterop.Inflate(ref s, ZLibFlushCode.NoFlush);
            switch (err)
            {
                case ZLibErrorCode.Ok:
                case ZLibErrorCode.StreamEnd:
                    break;
                case ZLibErrorCode.BufError:
                    throw new Exception("Output buffer is not large enought");
                default: throw new Exception("Unexpected error: " + Marshal.PtrToStringUTF8(s.msg));
            }
            if (ZLibInterop.InflateEnd(ref s) != ZLibErrorCode.Ok) throw new Exception("Unexpected ending: " + Marshal.PtrToStringUTF8(s.msg));
            return destination.Length - (int)s.availOut;
        }
    }
    /// <summary>
    /// Deflate Compression Algorithm - With zlib header
    /// </summary>
    public class ZLibCompressor() : DeflateCompressor(ZLibInterop.ZLib_DefaultWindowBits) { }
    /// <summary>
    /// Deflate Compression Algorithm - With gzib header
    /// </summary>
    public class GZipCompressor() : DeflateCompressor(ZLibInterop.GZip_DefaultWindowBits) { }
}
