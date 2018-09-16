// XELF.Imaging ©2018 XELF
// https://github.com/xelfia/XELF.Imaging
// MIT License

namespace XELF.Imaging {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using UnityEngine;
	using static PNG;
	using static System.Array;
	using static Zlib;
#if !XELF_PNGREADER_DISABLE_COMPRESSION
	using System.IO.Compression;
#endif
	public sealed partial class PNGReader : IDisposable {
		public int Width { get; private set; }
		public int Height { get; private set; }
		public int Depth { get; private set; } // APNG frames
		public Color32[] Palette { get; private set; }
		public Color32[] Materials { get; private set; }
		public List<byte[]> Frames = new List<byte[]>();
		public byte[] FramesMerged {
			get {
				var length = Frames.Sum(f => f.Length);
				var result = new byte[length];
				long d = 0;
				for (int i = 0; i < Frames.Count; i++) {
					var count = Frames[i].LongLength;
					Copy(Frames[i], 0, result, d, count);
					d += count;
				}
				return result;
			}
		}
		private MemoryStream stream;
		private BinaryReader reader;

		public void ParseChunks() {
			while (stream.Position < stream.Length) {
				var size = ReadSwappedUInt32();
				var start = stream.Position;
				var name = (ChunkName)reader.ReadUInt32();
				var content = reader.ReadBytes((int)size);
				var crc32 = ReadSwappedUInt32();
				var computed = CRC32.Compute(reader, start, 4 + size);
				if (crc32 != computed)
					throw new IOException($"Incorrect CRC32 | file:{crc32:X} ≠ computed:{computed:X} @ start = {start}, " +
						$"{(char)(byte)name}{(char)(byte)((uint)name >> 8)}{(char)(byte)((uint)name >> 16)}{(char)(byte)((uint)name >> 24)} [{size}]");

				var parentStream = stream;
				var parent = reader;
				using (reader = new BinaryReader(stream = new MemoryStream(content))) {
					switch (name) {
						case ChunkName.IEND:
							return;
						case ChunkName.IHDR:
							ReadIHDR();
							break;
						case ChunkName.IDAT:
							ReadIDAT(content);
							break;
						case ChunkName.fdAT:
							ReadfdAT(content);
							break;
						case ChunkName.PLTE:
							ReadPLTE(content);
							break;
						case ChunkName.tRNS:
							ReadtRNS(content);
							break;
						case ChunkName.sPLT:
							ReadsPLT(content);
							break;
					}
				}
				reader = parent;
				stream = parentStream;
			}
		}
		public static PNGReader Open(byte[] source) {
			var result = new PNGReader {
				stream = new MemoryStream(source)
			};
			result.reader = new BinaryReader(result.stream);
			if (result.reader.ReadUInt64() != Signature)
				throw new FormatException();

			result.ParseChunks();

			return result;
		}
		public void ReadIHDR() {
			Width = (int)ReadSwappedUInt32();
			Height = (int)ReadSwappedUInt32();
			var bitsPerPixel = reader.ReadByte();
			if (bitsPerPixel != 8)
				throw new FormatException($"Unsupported sample depth: {bitsPerPixel}");
			var colorType = (ColorType)reader.ReadByte();
			if (colorType != ColorType.Indexed)
				throw new FormatException($"Unsupported color type: {colorType}");
		}
		public void ReadIDAT(byte[] content) {
			Depth++;
			Frames.Add(ReadZlib(content, 0, content.Length));
		}
		public void ReadfdAT(byte[] content) {
			Depth++;
			ReadSwappedUInt32(); // sequence
			Frames.Add(ReadZlib(content, 4, content.Length - 4));
		}
		public void ReadPLTE(byte[] content) {
			Palette = new Color32[content.Length / 3];
			for (int i = 0; i < Palette.Length; i++) {
				Palette[i] = new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), 255);
			}
		}
		public void ReadtRNS(byte[] content) {
			if (Palette == null)
				Palette = new Color32[content.Length];
			for (int i = 0; i < Palette.Length; i++) {
				Palette[i].a = reader.ReadByte();
			}
		}
		// Reads a null-terminated-string (in Latin-1 : ISO/IEC-8859-1) and returns without "\0"
		public string ReadNullTerminatedString(int maximumBytes) {
			var bytes = new byte[maximumBytes];
			for (int i = 0; i < maximumBytes; i++) {
				bytes[i] = reader.ReadByte();
				if (bytes[i] == 0)
					return System.Text.Encoding.GetEncoding(28591).GetString(bytes, 0, i);
			}
			return System.Text.Encoding.GetEncoding(28591).GetString(bytes, 0, bytes.Length);
		}
		public void ReadsPLT(byte[] content) {
			var text = ReadNullTerminatedString(80);
			if (text != "MATL")
				return;
			var sampleDepth = reader.ReadByte();
			if (sampleDepth != 8)
				return;
			if (Materials == null)
				Materials = new Color32[(content.Length - stream.Position) / 6];
			for (int i = 0; i < Materials.Length; i++) {
				Materials[i] = new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
				ReadSwappedUInt16(); // drops the value of frequency
			}
		}
		public byte[] ReadZlib(byte[] content, int start, int count) {
			var zlibHeader = new ZlibHeader {
				CMF = (ZlibCompressionMethodAndFlags)reader.ReadByte(),
				FLG = (ZlibFlags)reader.ReadByte(),
			};
			if (zlibHeader.Method != ZlibCompressionMethodAndFlags.Deflate)
				throw new IOException($"Zlib compression method unsupported: {zlibHeader.CMF}");
			if ((zlibHeader.FLG & ZlibFlags.Dict) != 0)
				throw new IOException("Zlib Dict unsupported");

#if XELF_PNGREADER_DISABLE_COMPRESSION

			var blockHeader = new DeflateBlockHeader {
				Flags = (DeflateBlockHeaderFlags)reader.ReadByte(),
				Length = reader.ReadUInt16(),
				LengthComplement = reader.ReadUInt16(),
			};
			if (blockHeader.Flags != DeflateBlockHeaderFlags.Final)
				throw new IOException($"Unsupported Deflate Flags: {blockHeader.Flags}");
			if (blockHeader.Length != (ushort)~blockHeader.LengthComplement)
				throw new IOException($"Deflate Block Length corrupted: {blockHeader.Length} {~blockHeader.LengthComplement}");
			var bytes = reader.ReadBytes(blockHeader.Length);
#else

			uint adler32;
			var bytes = new byte[(Width + 1) * Height];
			using (var m = new MemoryStream(content, start + 2, count - 6)) {
				var start2 = 0;
				using (var deflate = new DeflateStream(m, CompressionMode.Decompress, true)) {
					do {
						var read = deflate.Read(bytes, start2, bytes.Length - start2);
						if (read == 0)
							break;
						start2 += read;
					} while (start2 < bytes.Length);
					deflate.Close();
					//Debug.Log($"{bytes.Length} {count}");
				}
#endif
				stream.Position = start + count - 4;
				//Debug.Log($"s={start} c={count} p={stream.Position} l={content.Length}");
				adler32 = ReadSwappedUInt32();

			}
			var computed = Adler32.Compute(bytes, 0, bytes.Length);
			if (adler32 != computed) {
				//Debug.Log($"Incorrect Adler32 | file:{adler32:X} computed:{computed:X}");
				throw new IOException($"Incorrect Adler32 | file:{adler32:X} computed:{computed:X}");
			}
			var data = GetUnfiltered(bytes);
			return data;
		}
		public byte[] GetUnfiltered(byte[] source) {
			var stride = Width + 1;
			if (source.Length != stride * Height)
				throw new IOException(
					$"Unexpected filtered image data size | file:{source.Length} expected:{stride * Height}");
			var result = new byte[Width * Height];
			for (int y = 0; y < Height; y++) {
				var filter = source[y * stride];
				if (filter != 0)
					throw new FormatException($"Unsupported filter: {filter}");
				Copy(source, y * stride + 1, result, (Height - y - 1) * Width, Width);
			}
			return result;
		}

		public uint ReadSwappedUInt32() {
			var value = reader.ReadUInt32();
			return (value << 24)
				+ ((value << 8) & 0x00ff0000)
				+ ((value >> 8) & 0x0000ff00)
				+ (value >> 24);
		}
		public ushort ReadSwappedUInt16() {
			var value = reader.ReadUInt16();
			return (ushort)((value << 8) + (value >> 8));
		}

		public void Dispose() {
			stream?.Dispose();
			reader?.Dispose();
		}
	}
}
