namespace XELF.DDS.Formats {
	public struct Header { // byte[128]
		public Magic Magic; // 0x20534444
		public uint Size; // 124;
		public Flags Flags;
		public uint Height;
		public uint Width;
		public uint PitchOrLinearSize;
		public uint Depth;
		public uint MipMapCount;
		public uint Reserved0;
		public uint Reserved1;
		public uint Reserved2;
		public uint Reserved3;
		public uint Reserved4;
		public uint Reserved5;
		public uint Reserved6;
		public uint Reserved7;
		public uint Reserved8;
		public uint Reserved9;
		public uint Reserved10;
		public uint Reserved11;
		public uint PfSize; // 32
		public PfFlags PfFlags;
		public FourCC FourCC;
		public uint RGBBitCount;
		public uint RBitMask;
		public uint GBitMask;
		public uint BBitMask;
		public uint ABitMask;
		public Caps1 Caps1;
		public Caps2 Caps2;
		public uint ReservedCaps3;
		public uint ReservedCaps4;
		public uint ReservedTail;
	}
	public enum Magic {
		DDS = 0x20534444,
	}
	public struct HeaderDX10 { // byte[20]
		public FormatDX10 Format;
		public Dimension Dimension;
		public uint MiscFlag1; // 0
		public uint ArraySize;
		public uint MiscFlag2; // 0
	}
	public enum Dimension {
		Dimension1D = 2,
		Dimension2D = 3,
		Dimension3D = 4,
	}
	public enum Flags {
		Caps = 0x00000001,
		Height = 0x00000002,
		Width = 0x00000004,
		Pitch = 0x00000008,
		PixelFormat = 0x00001000,
		MipMapCount = 0x00020000,
		LinearSize = 0x00080000,
		Depth = 0x00800000,
	}
	public enum PfFlags {
		ABitMask = 0x00000001,
		AlphaOnly = 0x00000002,
		FourCC = 0x00000004,
		PaletteIndexed4 = 0x00000008,
		PaletteIndexed8 = 0x00000020,
		RGBBitMask = 0x00000040,
		Luminance = 0x00020000,
		BumpDuDv = 0x00080000,
	}
	public enum Caps1 {
		AlphaContained = 0x00000002,
		Complex = 0x00000008,
		Texture = 0x00001000,
		MipMap = 0x00400000,
	}
	public enum Caps2 {
		CubeMap = 0x00000200,
		CubeMapPositiveX = 0x00000400,
		CubeMapNegativeX = 0x00000800,
		CubeMapPositiveY = 0x00001000,
		CubeMapNegativeY = 0x00002000,
		CubeMapPositiveZ = 0x00004000,
		CubeMapNegativeZ = 0x00008000,
		Volume = 0x00400000,
	}
	public enum FourCC {
		DX10 = 0x30315844,
		//DXT1, DXT2, DXT3, DXT4, DXT5, BC4U, BC4S, BC5U, BC5S,
		A16B16G16R16_UNorm = 0x00000024,
		R16G16B16A16_SNorm = 0x0000006e,
		R16_Float = 0x0000006f,
		A8 = 28,

		// ATI2, 3DC1, 3DC2, ATC, ATCI, ETC1, PTC2, PTC4,
	}

	public enum FormatDX10 {
		Unknown = 0,
		R8_UNorm = 61,
		R8_UInt = 62,
		A8_UNorm = 65,
	}

	public struct Headers {
		public Header Header;
		public HeaderDX10 HeaderDX10;
	}
}
