using System;
using GregoireConverter.GRG;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace GregoireConverter.GRG
{
	/// <summary>
	/// Class representing GRG2 neume
	/// </summary>
	public class GRG2Neume : IGRG2Segment
	{
		public GRG2.SegmentType Type { get {return GRG2.SegmentType.NEUME;} }
		public ushort SegmentLength { get; protected set; }
		public ushort Id { get; protected set; }
		public ushort PositionY { get; protected set; }
		public ushort PositionX { get; protected set; }
		public ushort UnknownValue4 { get; protected set; }
		public ushort UnknownValue5 { get; protected set; }
		public string Caption { get; set; }

		public bool IsClef { get { return (Id == 6 || Id == 7); } }

		public bool IsBemol { get { return Id == 5; } }

		public bool IsCustos { get { return Id == 47; } }

		public bool IsDivisio { get { return new[]{ 1, 2, 3, 4, 211, 212, 213, 214, 215 }.Contains(Id); } }

		private int[] _rhythmicIds = new [] { 23, 80, 81, 82 };
		public bool IsRhythmic
		{
			get
			{
				if (_rhythmicIds.Contains(Id))
					return true;
				
				return false;
			}
		}

		public List<GRG2Neume> Rhythmics = new List<GRG2Neume>();

		public string GetClefIndicator(bool withBemol)
		{
			string result;

			if (Id == 6) // Clé Do
				result = "c";
			else if (Id == 7)	// Clé Fa
				result = "f";
			else
				throw new ArgumentOutOfRangeException(string.Format("Id {0} does not point to a key", Id));

			if (withBemol)
				result += "b";

			int height = (-PositionY / 12) + 5;
			if (height < 1 || height > 4)
				throw new ArgumentOutOfRangeException(string.Format("{0} is not valid key height", height));

			return string.Format("{0}{1:D}", result, height);
		}

		public GRG2Neume()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GregoireConverter.GRG.GRG2Neume"/> class
		/// loading it's content from given stream.
		/// </summary>
		/// <param name="stream">Source stream.</param>
		public GRG2Neume (Stream stream)
		{
			var count = ReadStream(stream);

			if (count == 0)
				throw new EndOfStreamException("Unexpected end of Staff structure");
		}

		/// <summary>
		/// Reads the neumue segment from stream.
		/// </summary>
		/// <returns>Number of bytes from last read.</returns>
		/// <param name="stream">Source stream.</param>
		public int ReadStream(Stream stream)
		{
			int count;

			//	Read the length of the segment. It is hardcoded in the gregoire binary so it's not likely to change.
			ushort segmentLength, expectedSegmentLength = 0x001A;
			count = StreamHelper.ReadWORD(stream, out segmentLength);
			if (count == 0)
				throw new EndOfStreamException("Failed to read the length of the neume segment");
			if (segmentLength != expectedSegmentLength)
				Logger.LogWarning("Unexpected length of the segment. The length should be {0} and is {1}",
					expectedSegmentLength.ToString(), segmentLength.ToString());
			else
				Logger.LogVerbose("Read the length of the neume segment: {0:D3}", segmentLength);
			this.SegmentLength = segmentLength;

			//	Read neume ID
			ushort id;
			count = StreamHelper.ReadWORD(stream, out id);
			if (count == 0)
				throw new EndOfStreamException("Failed to read neume's id");
			Logger.LogDebug("Read neume's id: {0:D3}", id);
			this.Id = id;

			//	Read neume position y
			ushort posY;
			count = StreamHelper.ReadWORD(stream, out posY);
			if (count == 0)
				throw new EndOfStreamException("Failed to read neume's position Y");
			Logger.LogDebug("Read neume's position Y: {0:D3}", posY);
			this.PositionY = posY;

			//	Read neume position X
			ushort posX;
			count = StreamHelper.ReadWORD(stream, out posX);
			if (count == 0)
				throw new EndOfStreamException("Failed to read neume's position X");
			Logger.LogDebug("Read neume's position X: {0:D3}", posX);
			this.PositionX = posX;

			//	Read not yet recognized value 4
			ushort unknownValue4;
			count = StreamHelper.ReadWORD(stream, out unknownValue4);
			if (count == 0)
				throw new EndOfStreamException("Failed to read neume's unknown value 4");
			Logger.LogDebug("Read neume's unknown value 4: {0:D3}", unknownValue4);
			this.UnknownValue4 = unknownValue4;

			//	Read neume ID
			ushort unknownValue5;
			count = StreamHelper.ReadWORD(stream, out unknownValue5);
			if (count == 0)
				throw new EndOfStreamException("Failed to read neume's unknown value 5");
			Logger.LogDebug("Read neume's unknown value 5: {0:D3}", unknownValue5);
			this.UnknownValue5 = unknownValue5;

			//	Read neume caption
			string cap;
			count = StreamHelper.ReadString(stream, 0x10, out cap);
			if (count == 0)
				throw new EndOfStreamException("Failed to read neume's caption");
			if (!string.IsNullOrWhiteSpace(cap))
				Logger.LogDebug("Read neume's caption: {0}", cap);
			else
				Logger.LogVerbose("Read neume's caption: {0}", cap);
			this.Caption = cap;

			return count;
		}

		public override string ToString()
		{
			return string.Format("[NeumeForGabc:({0})'{1}'@{2},{3}]", Id, Caption, PositionX, PositionY);
		}
	}
}

