// XELF.Imaging ©2018 XELF
// https://github.com/xelfia/XELF.Imaging
// MIT License

using System.IO;

namespace XELF.DDS {
	using Formats;
#if false
	public static partial class DDSReader {
		public static void	Read(BinaryReader reader) {
			if (reader.ReadUInt32() != (uint) Magic.DDS) {
				throw new System.FormatException();
			}
			//Headers headers = new Headers();
			//TODO:
		}
	}
#endif
	public static partial class DDSWriter {
		public static void WriteL8Volume(BinaryWriter writer, uint width, uint height, uint depth, byte[] content) {
			Headers headers = new Headers();
			headers.Header.Magic = Magic.DDS;
			headers.Header.Size = 124;
			headers.Header.Flags
				= Flags.Caps | Flags.Height | Flags.Width
				| Flags.Pitch | Flags.PixelFormat | Flags.Depth;
			headers.Header.Height = height;
			headers.Header.Width = width;
			headers.Header.PitchOrLinearSize = Pitch(width, 8);
			headers.Header.Depth = depth;
			headers.Header.MipMapCount = 1;
			headers.Header.PfSize = 32;
			headers.Header.PfFlags = PfFlags.RGBBitMask | PfFlags.Luminance;
			headers.Header.FourCC = 0;
			headers.Header.RGBBitCount = 8;
			headers.Header.RBitMask = 0xFFu;
			headers.Header.Caps1 = Caps1.Texture | Caps1.Complex;
			headers.Header.Caps2 = Caps2.Volume;
			WriteHeader(writer, ref headers.Header);
			WriteBodyWithPadding(writer, width, content);
		}
		public static void WriteA8Volume(BinaryWriter writer, uint width, uint height, uint depth, byte[] content) {
			Headers headers = new Headers();
			headers.Header.Magic = Magic.DDS;
			headers.Header.Size = 124;
			headers.Header.Flags
				= Flags.Caps | Flags.Height | Flags.Width
				| Flags.Pitch | Flags.PixelFormat | Flags.Depth;
			headers.Header.Height = height;
			headers.Header.Width = width;
			headers.Header.PitchOrLinearSize = Pitch(width, 8);
			headers.Header.Depth = depth;
			headers.Header.MipMapCount = 1;
			headers.Header.PfSize = 32;
			headers.Header.PfFlags = PfFlags.ABitMask | PfFlags.AlphaOnly;
			headers.Header.FourCC = 0;
			headers.Header.RGBBitCount = 8;
			headers.Header.ABitMask = 0xFFu;
			headers.Header.Caps1 = Caps1.AlphaContained | Caps1.Texture | Caps1.Complex;
			headers.Header.Caps2 = Caps2.Volume;
			WriteHeader(writer, ref headers.Header);
			WriteBodyWithPadding(writer, width, content);
		}
		public static void WriteDX10Volume(BinaryWriter writer, uint width, uint height, uint depth, byte[] content) {
			Headers headers = new Headers();
			headers.Header.Magic = Magic.DDS;
			headers.Header.Size = 124;
			headers.Header.Flags
				= Flags.Caps | Flags.Height | Flags.Width
				| Flags.Pitch | Flags.Depth | Flags.PixelFormat;
			headers.Header.Height = height;
			headers.Header.Width = width;
			headers.Header.PitchOrLinearSize = Pitch(width, 8);
			headers.Header.Depth = depth;
			headers.Header.MipMapCount = 1;
			headers.Header.PfSize = 32;
			headers.Header.PfFlags = PfFlags.FourCC;
			headers.Header.FourCC = FourCC.DX10;
			headers.Header.Caps1 = Caps1.Texture | Caps1.Complex;
			headers.Header.Caps2 = Caps2.Volume;
			headers.HeaderDX10.Format = FormatDX10.R8_UNorm;
			headers.HeaderDX10.Dimension = Dimension.Dimension3D;
			WriteHeader(writer, ref headers.Header);
			WriteBodyWithPadding(writer, width, content);
		}
		static uint Pitch(uint width, uint bitCount) {
			return (((width * ((bitCount + 7) & 0xfffffff8u)) >> 3) + 3) & 0xfffffffcu;
		}
		static readonly byte[] padding = new byte[3];
		static void WriteBodyWithPadding(BinaryWriter writer, uint width, byte[] content) {
			uint width1 = width - 1;
			uint paddingWidth = Pitch(width, 8) - width;
			for (uint i = width1; i < content.Length; i += width) {
				writer.Write(content, (int)(i - width1), (int)width);
				writer.Write(padding, 0, (int)paddingWidth);
			}
		}

		static void WriterHeader(BinaryWriter writer, ref Headers headers) {
			WriteHeader(writer, ref headers.Header);
			if (headers.Header.FourCC == FourCC.DX10)
				WriteHeaderDX10(writer, ref headers.HeaderDX10);
		}
		static void WriteHeader(BinaryWriter writer, ref Header header) {
			writer.Write((uint)header.Magic);
			writer.Write(header.Size);
			writer.Write((uint)header.Flags);
			writer.Write(header.Height);
			writer.Write(header.Width);
			writer.Write(header.PitchOrLinearSize);
			writer.Write(header.Depth);
			writer.Write(header.MipMapCount);
			writer.Write(header.Reserved0);
			writer.Write(header.Reserved1);
			writer.Write(header.Reserved2);
			writer.Write(header.Reserved3);
			writer.Write(header.Reserved4);
			writer.Write(header.Reserved5);
			writer.Write(header.Reserved6);
			writer.Write(header.Reserved7);
			writer.Write(header.Reserved8);
			writer.Write(header.Reserved9);
			writer.Write(header.Reserved10);
			writer.Write(header.PfSize);
			writer.Write((uint)header.PfFlags);
			writer.Write((uint)header.FourCC);
			writer.Write(header.RGBBitCount);
			writer.Write(header.RBitMask);
			writer.Write(header.GBitMask);
			writer.Write(header.BBitMask);
			writer.Write(header.ABitMask);
			writer.Write((uint)header.Caps1);
			writer.Write((uint)header.Caps2);
			writer.Write(header.ReservedCaps3);
			writer.Write(header.ReservedCaps4);
			writer.Write(header.ReservedTail);
		}
		static void WriteHeaderDX10(BinaryWriter writer, ref HeaderDX10 header) {
			writer.Write((uint)header.Format);
			writer.Write((uint)header.Dimension);
			writer.Write(header.MiscFlag1);
			writer.Write(header.ArraySize);
			writer.Write(header.MiscFlag2);
		}
	}
}
