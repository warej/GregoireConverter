using System;
using GregoireConverter.GRG;
using System.IO;

namespace GregoireConverter.GRG
{
	public class Color
	{
		public byte R { get; private set; }
		public byte G { get; private set; }
		public byte B { get; private set; }
		public byte A { get; private set; }

		public Color (byte r, byte g, byte b, byte a)
		{
			this.R = r;
			this.G = g;
			this.B = b;
			this.A = a;
		}

		public Color (byte[] bytes)
		{
			this.R = bytes[0];
			this.G = bytes[1];
			this.B = bytes[2];
			this.A = bytes[3];
		}

		public static Color FromStream (Stream stream)
		{
			int count;
			var bytes = new byte[0x04];
			count = stream.Read(bytes, 0, 0x04);
			if (count < 0x04)
				throw new EndOfStreamException(
					string.Format("Failed to read color. {0} bytes read.", count));

			return new Color(bytes);
		}

		public override string ToString()
		{
			return string.Format("#{0:x2}{1:x2}{2:x2}{3:x2}", R, G, B, A);
		}
	}
}

