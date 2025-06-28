using System;
using System.Text;

namespace GregoireConverter.gabc
{
	public class NeumeFormat
	{
		public int SizeX;
		public int SizeY;
		public NeumePart[] Parts;

		public NeumeFormat ()
		{
		}

		public NeumeFormat (int sizeX, int sizeY, params NeumePart[] parts)
		{
			SizeX = sizeX;
			SizeY = sizeY;
			Parts = parts;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			foreach (var part in Parts)
			{
				sb.Append(part.Before);
				sb.Append(part.Height);
				sb.Append(part.After);
			}
			return sb.ToString();
		}
	}
}

