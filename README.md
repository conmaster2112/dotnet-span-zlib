Fast, Lite and memory efficient, a library that focuses both on ease of use and also bypasses the use of dotnet streams, excluding streams has the positive effect of full control over memory allocation.
The focus of this library is to use the span structure wherever possible.

 - **This library is not ideal for compressing large files, but rather for fast data processing. (Networking)**
 - Goal of this library is to keep up with Spans, becouse [All About Span: Exploring a new .NET Mainstay](https://learn.microsoft.com/en-us/archive/msdn-magazine/2018/january/csharp-all-about-span-exploring-a-new-net-mainstay)

## Available Classes
 - **DeflateCompressor** Raw Deflate and Inflate operations
 - **ZLibCompressor** ZLib Deflate and Inflate, headers are used
 - **GZipCompressor** GZip ...

## Examples
Using DeflateCompressor
### Basic Span Compression
```csharp
using ConMaster.Compression;

DeflateCompressor compressor = new();


Span<byte> data = buffer.AsSpan(0, recivedBytes);
byte[] compressionBuffer = new byte[recivedBytes];

compressor.Compress(data, compressedData, out int bytesWritten);
            
Span<byte> compressedData = compressedData.AsSpan(0, bytesWritten);

client.Send(compressedData);
```
### Compression Level with Stack Allocation
```csharp
using ConMaster.Compression;
using System.IO.Compression; //CompressionLevel Enum

DeflateCompressor compressor = new()
{
    CompressionLevel = CompressionLevel.Fastest
};

ReadOnlySpan<byte> text = "My Text to Compress Please, Please, Please"u8;
Span<byte> compressedBuffer = stackalloc byte[text.Length];

compressedBuffer = compressor.CompressToSpan(text, compressedBuffer);
            
string myCompressedString = Encoding.UTF8.GetString(compressedBuffer);
```

### Crc32 Hashing
```csharp
uint hash = DeflateCompressor.Crc32(buffer, testCase);
```