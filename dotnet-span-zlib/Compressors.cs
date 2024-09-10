using System;
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
            fixed(byte* src = &MemoryMarshal.GetReference(source))
            {
                fixed(byte* dst = &MemoryMarshal.GetReference(destination))
                {
                    ZStream s = new((nint)src, source.Length, (nint)dst, destination.Length);
                    ZLibErrorCode errorCode;
                    try
                    {
                        errorCode = ZLibInterop.DeflateInit2_(
                            ref s, _compressionLevel, CompressionMethod.Deflated,
                            _windowBits, _memoryLevel, CompressionStrategy.DefaultStrategy
                        );
                        if (errorCode != ZLibErrorCode.Ok) throw new ZlibUnexpectedException("Initialization Failed", errorCode);

                        errorCode = ZLibInterop.Deflate(ref s, ZLibFlushCode.SyncFlush);
                        if (errorCode != ZLibErrorCode.Ok) throw new ZlibUnexpectedException("Unexpected error: " + Marshal.PtrToStringUTF8(s.msg), errorCode);

                        errorCode = ZLibInterop.Deflate(ref s, ZLibFlushCode.Finish);
                        return errorCode switch
                        {
                            ZLibErrorCode.Ok or ZLibErrorCode.StreamEnd => destination.Length - (int)s.availOut,
                            ZLibErrorCode.BufError => throw new ZlibBufferOverflowException("Destianation buffer overflow"),
                            _ => throw new ZlibUnexpectedException("Unexpected error: " + Marshal.PtrToStringUTF8(s.msg), errorCode),
                        };
                    }
                    finally
                    {
                        ZLibInterop.DeflateEnd(ref s);
                    }
                }
            }
        }
        internal unsafe int InternalInflate(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            fixed (byte* src = &MemoryMarshal.GetReference(source))
            {
                fixed (byte* dst = &MemoryMarshal.GetReference(destination))
                {
                    ZStream s = new((nint)src, source.Length, (nint)dst, destination.Length);
                    ZLibErrorCode errorCode;
                    try
                    {
                        errorCode = ZLibInterop.InflateInit2_(ref s, _windowBits);
                        if (errorCode != ZLibErrorCode.Ok) throw new ZlibUnexpectedException("Initialization Failed", errorCode);

                        errorCode = ZLibInterop.Inflate(ref s, ZLibFlushCode.SyncFlush);
                        if (errorCode == ZLibErrorCode.StreamEnd) return destination.Length - (int)s.availOut;
                        else if (errorCode != ZLibErrorCode.Ok) throw new ZlibUnexpectedException("Unexpected error: " + Marshal.PtrToStringUTF8(s.msg), errorCode);

                        errorCode = ZLibInterop.Inflate(ref s, ZLibFlushCode.Finish);
                        return errorCode switch
                        {
                            ZLibErrorCode.Ok or ZLibErrorCode.StreamEnd => destination.Length - (int)s.availOut,
                            ZLibErrorCode.BufError => throw new ZlibBufferOverflowException("Destianation buffer overflow"),
                            _ => throw new ZlibUnexpectedException("Unexpected error: " + Marshal.PtrToStringUTF8(s.msg), errorCode),
                        };
                    }
                    finally
                    {
                        ZLibInterop.InflateEnd(ref s);
                    }
                }
            }
        }
    }
    /// <summary>
    /// Deflate Compression Algorithm - With zlib header
    /// </summary>
    public class ZLibCompressor : DeflateCompressor
    {
        /// <summary>
        /// Deflate Compression Algorithm - With zlib header
        /// </summary>
        public ZLibCompressor() : base(ZLibInterop.ZLib_DefaultWindowBits) { }
    }
    /// <summary>
    /// Deflate Compression Algorithm - With gzib header
    /// </summary>
    public class GZipCompressor : DeflateCompressor
    {
        /// <summary>
        /// Deflate Compression Algorithm - With zlib header
        /// </summary>
        public GZipCompressor() : base(ZLibInterop.GZip_DefaultWindowBits) { }
    }

    /// <summary>
    /// Exception throwed when destination buffer overflows.
    /// </summary>
    public class ZlibBufferOverflowException : Exception
    {
        /// <summary>
        /// Exception throwed when destination buffer overflows.
        /// </summary>
        public ZlibBufferOverflowException(string message) : base(message) { }
    }
    /// <summary>
    /// Internal Unexpected Error.
    /// </summary>
    internal class ZlibUnexpectedException : Exception
    {
        public ZlibUnexpectedException(string message, ZLibErrorCode code) : base(message + ": " + code)
        {
            Code = code;   
        }
        public ZLibErrorCode Code;
    }
}
