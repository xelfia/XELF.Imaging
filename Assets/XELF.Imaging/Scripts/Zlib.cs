// ©2018 XELF

namespace XELF.Imaging {
	public static class Deflate {
		public enum DeflateBlockHeaderFlags : byte {
			Final = 1,
			Uncompressed = 0,
			Fixed = 2,
			Custom = 4,
			Error = 6,
		}
		public struct DeflateBlockHeader { // byte[5]
			public const uint Size = 5;
			public DeflateBlockHeaderFlags Flags;
			public ushort Length;
			public ushort LengthComplement;

			public static DeflateBlockHeader Uncomressed(ushort length, bool final = true) {
				return new DeflateBlockHeader {
					Flags = final
						? DeflateBlockHeaderFlags.Uncompressed | DeflateBlockHeaderFlags.Final
						: DeflateBlockHeaderFlags.Uncompressed,
					Length = length,
					LengthComplement = (ushort)~length,
				};
			}
		}
	}
	public static partial class Zlib {
		public enum ZlibCompressionMethodAndFlags : byte {
			DeflateWindow32768 = 0x7 << 4,
			Deflate = 8,
		}
		public enum ZlibFlags : byte {
			Dict = 1 << 5,
		}
		public enum ZlibDictAndLevel : byte {
			Dict = 1 << 5,
			LevelFastest = 0 << 6,
			LevelFaster = 1 << 6,
			LevelDefault = 2 << 6,
			LevelSlow = 3 << 6,
		}
		public struct ZlibHeader {
			public const uint Size = 2;

			public ZlibCompressionMethodAndFlags CMF;
			public ZlibFlags FLG;

			public ZlibCompressionMethodAndFlags Method
				=> CMF & (ZlibCompressionMethodAndFlags)0x0F;
			public ZlibCompressionMethodAndFlags DelateWindowFlags
				=> CMF & (ZlibCompressionMethodAndFlags)0xF0;

			public ZlibHeader(ZlibCompressionMethodAndFlags cmf, ZlibDictAndLevel level) {
				CMF = cmf;
				FLG = (ZlibFlags)((int)level | (31 - (((int)cmf << 8) + (int)level) % 31));
			}
		}
		public static class Adler32 {
			public const uint Size = 4;
			private const uint Base = 65521;
			public static uint Compute(byte[] data, long start, long count) {
				uint s1 = 1;
				uint s2 = 0;
				var end = start + count;
				for (long i = start; i < end; i++) {
					s1 = (s1 + data[i]) % Base;
					s2 = (s2 + s1) % Base;
				}
				return (s2 << 16) | s1;
			}
		}
	}
}

