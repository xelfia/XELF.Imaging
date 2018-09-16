// ©2018 XELF

namespace XELF.Imaging {
	public static class PNG {
		public const ulong Signature = 0x0a1a0a0d474e5089ul;
		public enum ChunkName : uint {
			IHDR = 0x52444849, // PNG: Header
			PLTE = 0x45544c50, // PNG: Palette (RGB)
			tRNS = 0x534e5274, // PNG: Transparency (A)
			sPLT = 0x544c5073, // PNG: Suggested Palette
			IDAT = 0x54414449, // PNG: Data
			IEND = 0x444e4549, // PNG: End
			acTL = 0x4c546361, // APNG: Animation Control
			fcTL = 0x4c546366, // APNG: Frame Control
			fdAT = 0x54416466, // APNG: Frame Data
		}
		public enum ChunkSize : uint {
			IHDR = 13,
			IEND = 0,
			acTL = 8,
		}
		public enum ColorType : byte {
			Grayscale = 0, // 1,2,4,8,16 bpp: wihout PLTE
			PaletteEnabled = 1, // Disabled1 (palette enabled flag)
			RGB = 2, // 8,16 bpp
			Indexed = 3, // 1,2,4,8 bpp
			Alpha = 4, // 8, 16 bpp
			Disabled5 = 5,
			RGBA = 6, // 8,16 bpp
			Disabled7 = 7,
		}
		public enum CompressionMethod : byte {
			Zero,
		}
		public enum FilterMethod : byte {
			Zero,
		}
		public enum FilterMethodZeroType : byte {
			None, Sub, Up, Average, Paeth,
		}
		public enum Interlace : byte {
			None, Adam7,
		}
		public struct FrameControl { // APNG: byte[26]
			public const uint Size = 26;

			public uint sequence;
			public uint width, height;
			public uint xOffset, yOffset;
			public ushort delayNum, delayDen;
			public byte disposeOp, blendOp;
		}
	}
}
