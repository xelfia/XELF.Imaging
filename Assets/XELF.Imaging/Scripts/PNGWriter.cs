// XELF.Imaging ©2018 XELF
// https://github.com/xelfia/XELF.Imaging
// MIT License

namespace XELF.Imaging {
	using System;
	using System.IO;
	using UnityEngine;
	using static Deflate;
	using static PNG;
	using static System.Array;
	using static Zlib;

	public sealed partial class PNGWriter : IDisposable {
		private MemoryStream stream;
		private BinaryWriter writer;

		private int width, height;
		private byte bitsPerPixel;
		private ColorType ColorType { get; set; }
		private CompressionMethod Compression { get; set; }
		private FilterMethod Filter { get; set; }
		private Interlace Interlace { get; set; }

		public void Dispose() {
			writer?.Dispose();
			stream?.Dispose();
		}
		public void WriteTo(Stream destination) {
			stream.Position = 0;
			stream.CopyTo(destination);
		}
		public static PNGWriter CreateA8FirstSlice(int width, int height, int depth, byte[] colors) {
			var result = new PNGWriter {
				stream = new MemoryStream(),
				width = width,
				height = height,
				bitsPerPixel = 8,
				ColorType = ColorType.Alpha,
			};
			result.writer = new BinaryWriter(result.stream);
			result.WriteSignature();
			result.WriteIHDR();
			result.WriteIDAT(colors, 0, colors.LongLength / depth);
			result.WriteIEND();
			return result;
		}
		// APNG
		public static PNGWriter CreateIndexed8(int width, int height, int depth, byte[] colors, Color[] palette, Vector4[] materials) {
			var result = new PNGWriter {
				stream = new MemoryStream(),
				width = width,
				height = height,
				bitsPerPixel = 8,
				ColorType = ColorType.Indexed,
			};
			result.writer = new BinaryWriter(result.stream);
			var frameSize = width * height;
			result.WriteSignature();
			result.WriteIHDR();
			if (palette != null) {
				result.WritePLTE(palette);
				result.WritetRNS(palette);
				//result.WritesPLT_RGBA(palette);
			}
			if (materials != null)
				result.WritesPLT_MATL(materials);
			result.WriteacTL((uint)depth);
			uint sequence = 0;
			var frameControl = new FrameControl {
				width = (uint)width,
				height = (uint)height,
				delayNum = 1,
				delayDen = 60,
			};
			frameControl.sequence = sequence++;
			result.WritefcTL(frameControl);
			if (depth > 0)
				result.WriteIDAT(colors, 0, frameSize);
			for (int f = 1; f < depth; f++) {
				frameControl.sequence = sequence++;
				result.WritefcTL(frameControl);
				result.WritefdAT(sequence++, colors, f * frameSize, frameSize);
			}
			result.WriteIEND();
			return result;
		}
		public static PNGWriter WriteA8(int width, int height, byte[] colors) {
			var result = new PNGWriter {
				stream = new MemoryStream(),
				width = width,
				height = height,
				bitsPerPixel = 8,
				ColorType = 0,
			};
			result.writer = new BinaryWriter(result.stream);
			result.WriteSignature();
			result.WriteIHDR();
			result.WriteIDAT(colors, 0, colors.LongLength);
			result.WriteIEND();
			return result;
		}
		void WriteSignature()
			=> writer.Write(Signature);

		void WritePLTE(Color[] palette) {
			var start = BeginChunk((uint)(3 * palette.Length), ChunkName.PLTE);
			for (int i = 0; i < palette.Length; i++) {
				var color = (Color32)palette[i];
				writer.Write(color.r);
				writer.Write(color.g);
				writer.Write(color.b);
			}
			EndChunk(start);
		}
		void WritetRNS(Color[] palette) {
			var start = BeginChunk((uint)(palette.Length), ChunkName.tRNS);
			for (int i = 0; i < palette.Length; i++) {
				var color = (Color32)palette[i];
				writer.Write(color.a);
			}
			EndChunk(start);
		}
		void WritesPLT_RGBA(Color[] palette) {
			var start = BeginChunk((uint)(5 + 1 + 6 * palette.Length), ChunkName.sPLT);
			writer.Write((byte)'R');
			writer.Write((byte)'G');
			writer.Write((byte)'B');
			writer.Write((byte)'A');
			writer.Write((byte)0);
			writer.Write((byte)8);
			for (int i = 0; i < palette.Length; i++) {
				var color = (Color32)palette[i];
				writer.Write(color.r);
				writer.Write(color.g);
				writer.Write(color.b);
				writer.Write(color.a);
				WriteSwapped((ushort)0); //  Frequency
			}
			EndChunk(start);
		}
		void WritesPLT_MATL(Vector4[] materials) {
			var start = BeginChunk((uint)(5 + 1 + 6 * materials.Length), ChunkName.sPLT);
			writer.Write((byte)'M');
			writer.Write((byte)'A');
			writer.Write((byte)'T');
			writer.Write((byte)'L');
			writer.Write((byte)0); // null-terminator
			writer.Write((byte)8); // sample depth
			for (int i = 0; i < materials.Length; i++) {
				var color = (Color32)new Color(materials[i].x, materials[i].y, materials[i].z, materials[i].w);
				writer.Write(color.r);
				writer.Write(color.g);
				writer.Write(color.b);
				writer.Write(color.a);
				WriteSwapped((ushort)0); //  Frequency
			}
			EndChunk(start);
		}
		void WriteIHDR() {
			var start = BeginChunk(ChunkSize.IHDR, ChunkName.IHDR);
			WriteSwapped((uint)width);
			WriteSwapped((uint)height);
			writer.Write(bitsPerPixel);
			writer.Write((byte)ColorType);
			writer.Write((byte)Compression);
			writer.Write((byte)Filter);
			writer.Write((byte)Interlace);
			EndChunk(start);
		}
		void WriteacTL(uint frames, uint repeats = 0) {
			var startPosition = BeginChunk(ChunkSize.acTL, ChunkName.acTL);
			WriteSwapped(frames);
			WriteSwapped(repeats);
			EndChunk(startPosition);
		}
		void WritefcTL(FrameControl data) {
			var startPosition = BeginChunk(FrameControl.Size, ChunkName.fcTL);
			Write(data);
			EndChunk(startPosition);
		}
		// get filtered bytes with Y-axis inversed
		byte[] GetFiltered(byte[] source, long start, long count) {
			var stride = 1 + width;
			var result = new byte[stride * height];
			for (int y = 0; y < height; y++) {
				result[y * stride] = (byte)FilterMethodZeroType.None;
				Copy(source, start + (height - 1 - y) * width, result, y * stride + 1, width);
			}
			return result;
		}
		void WriteIDAT(byte[] source, long start, long count) {
			var filtered = GetFiltered(source, start, count);
			var startPosition = BeginChunk(0u, ChunkName.IDAT);
			WriteZlib(filtered, 0, filtered.LongLength);
			EndChunkWithResize(startPosition);
		}
		void WritefdAT(uint sequence, byte[] source, long start, long count) {
			var filtered = GetFiltered(source, start, count);
			var startPosition = BeginChunk(0u, ChunkName.fdAT);
			WriteSwapped(sequence);
			WriteZlib(filtered, 0, filtered.LongLength);
			EndChunkWithResize(startPosition);
		}
		void WriteIEND()
			=> EndChunk(BeginChunk(ChunkSize.IEND, ChunkName.IEND));

		void EndChunk(long start) =>
			WriteCRC32(start, stream.Position);
		void EndChunkWithResize(long start) {
			var beforeEnd = stream.Position;
			EndChunk(start);
			var end = stream.Position;
			stream.Position = start - 4;
			stream.Flush();
			WriteSwapped((uint)(beforeEnd - start - 4));
			stream.Position = end;
			stream.Flush();
		}
		long BeginChunk(ChunkSize size, ChunkName name)
			=> BeginChunk((uint)size, name);
		long BeginChunk(uint length, ChunkName name) {
			WriteSwapped(length);
			var start = stream.Position;
			writer.Write((uint)name);
			return start;
		}

		void WriteSwapped(ushort value) =>
			writer.Write((ushort)((value >> 8) + (value << 8)));
		void WriteSwapped(uint value) {
			writer.Write(
				(value << 24) +
				((value << 8) & 0x00ff0000) +
				((value >> 8) & 0x0000ff00) +
				(value >> 24));
		}
		void WriteCRC32(long start, long end) {
			var crc = CRC32.Compute(stream.GetBuffer(), start, end - start);
			WriteSwapped(crc);
		}
		void Write(FrameControl data) {
			WriteSwapped(data.sequence);
			WriteSwapped(data.width);
			WriteSwapped(data.height);
			WriteSwapped(data.xOffset);
			WriteSwapped(data.yOffset);
			WriteSwapped(data.delayNum);
			WriteSwapped(data.delayDen);
			writer.Write(data.disposeOp);
			writer.Write(data.blendOp);
		}
	}
	// Uncomressed only subset of Zlib and Deflate
	public sealed partial class PNGWriter : IDisposable {
		void Write(DeflateBlockHeader header) {
			writer.Write((byte)header.Flags);
			writer.Write(header.Length);
			writer.Write(header.LengthComplement);
		}

		uint WriteZlib(byte[] data, long start, long count) {
			var contentStart = stream.Position;
#if XELF_PNGREADER_DISABLE_COMPRESSION
			Write(new ZlibHeader(ZlibCompressionMethodAndFlags.Deflate, ZlibDictAndLevel.LevelDefault));
			Write(DeflateBlockHeader.Uncomressed((ushort)data.Length));

			// if ((header.FLG & ZlibFlags.Dict) != 0) WriteAdler32(...));

			writer.Write(data, (int)start, (int)count);
#else
			Write(new ZlibHeader(ZlibCompressionMethodAndFlags.DeflateWindow32768
				| ZlibCompressionMethodAndFlags.Deflate, ZlibDictAndLevel.LevelSlow));

			using (var deflate = new System.IO.Compression.DeflateStream(
				stream, System.IO.Compression.CompressionLevel.Optimal, true)) {
				deflate.Write(data, (int)start, (int)count);
				deflate.Close();
			}
#endif
			writer.Flush();
			WriteAdler32(data, start, count);
			writer.Flush();
			return (uint)(stream.Position - contentStart);
		}
		void Write(ZlibHeader header) {
			writer.Write((byte)header.CMF);
			writer.Write((byte)header.FLG);
		}
		void WriteAdler32(byte[] data, long start, long count) {
			var x = Adler32.Compute(data, start, count);
			//Debug.Log($"Adler32:{x:X}");
			WriteSwapped(x);
		}
	}
}
