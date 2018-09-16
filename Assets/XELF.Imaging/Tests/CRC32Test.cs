using NUnit.Framework;
using XELF.Imaging;

public class CRC32Test {
	[Test]
	public void CRC32TestSimplePasses() {
		var IEND = new byte[] { 0x49, 0x45, 0x4e, 0x44 };
		var crc = CRC32.Compute(IEND, 0, 4);
		Assert.AreEqual(crc, 0xAE426082, "IEND chunk's CRC");
	}
}
