// XELF.Imaging ©2018 XELF
// https://github.com/xelfia/XELF.Imaging
// MIT License

namespace XELF.Imaging {
	using System.IO;
	using UnityEngine;

	public static class CRC32 {
		private static readonly uint[] table = new uint[256];
		private const uint magic = 0xedb88320u;
		static CRC32() {
			for (uint i = 0; i < 256; i++) {
				uint value = i;
				for (int j = 0; j < 8; j++) {
					var b = value & 1;
					value >>= 1;
					if (b != 0)
						value ^= magic;
				}
				table[i] = value;
			}
		}
		public static uint Compute(BinaryReader reader, long start, long count) {
			var old = reader.BaseStream.Position;
			try {
				reader.BaseStream.Position = start;
				reader.BaseStream.Flush();
				var crc = 0xffffffffu;
				var end = start + count;
				for (var i = start; i < end; i++) {
					crc = table[(crc ^ reader.ReadByte()) & 0xff] ^ (crc >> 8);
				}
				return ~crc;
			} finally {
				reader.BaseStream.Position = old;
				reader.BaseStream.Flush();
			}
		}
		public static uint Compute(byte[] data, long start, long count) {
			var crc = 0xffffffffu;
			var end = start + count;
			for (var i = start; i < end; i++) {
				crc = table[(crc ^ data[i]) & 0xff] ^ (crc >> 8);
			}
			return ~crc;
		}
	}
}
