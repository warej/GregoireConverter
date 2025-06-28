using System;
using System.IO;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace GregoireConverter.GRG
{
	/// <summary>
	/// Class representing GRG2 document
	/// </summary>
	public class GRG2Document : IGRG2Segment
	{
		public GRG2.SegmentType Type { get {return GRG2.SegmentType.DOCUMENT;} }
		public ushort SegmentLength { get; private set; }
		public ushort FontSize { get; private set; }
		public string FontFamily { get; private set; }
		public Color StaffColor { get; private set; }
		public Color NeumesColor { get; private set; }
		public Color FontColor { get; private set; }
		public short SpaceUnderStaff { get; private set; }

		public List<GRG2Staff> Staffs { get; private set; }

		public GRG2Document ()
		{
			Staffs = new List<GRG2Staff>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GregoireConverter.GRG.GRG2Document"/> class
		/// loading it's content from given stream.
		/// </summary>
		/// <param name="stream">Source stream.</param>
		public GRG2Document (Stream stream)
		{
			Staffs = new List<GRG2Staff>();
			var count = ReadStream(stream);

			if (count == 0)
				throw new EndOfStreamException("Unexpected end of Document");
		}

		/// <summary>
		/// Reads the Document segment from stream.
		/// </summary>
		/// <returns>Number of bytes from last read.</returns>
		/// <param name="stream">Source stream.</param>
		public int ReadStream(Stream stream)
		{
			int count;

			//	Read the length of the segment. It is hardcoded in the gregoire binary so it's not likely to change.
			ushort segmentLength, expectedSegmentLength = 0x0020;
			count = StreamHelper.ReadWORD(stream, out segmentLength);
			if (count == 0)
				throw new EndOfStreamException("Failed to read the length of the document segment");
			if (segmentLength != expectedSegmentLength)
				Logger.LogWarning("Unexpected length of the segment. The length should be {0} and is {1}",
					expectedSegmentLength.ToString(), segmentLength.ToString());
			else
				Logger.LogVerbose("Read the length of the document segment: {0:D3}", segmentLength);
			this.SegmentLength = segmentLength;

			//	Read document's font size
			ushort fontSize;
			count = StreamHelper.ReadWORD(stream, out fontSize);
			if (count == 0)
				throw new EndOfStreamException("Failed to read document's font size");
			Logger.LogDebug("Read document's font size: {0:D2}", fontSize);
			this.FontSize = fontSize;

			//	Read document's font family
			string fontFamily;
			count = StreamHelper.ReadString(stream, 0x10, out fontFamily);
			if (count == 0)
				throw new EndOfStreamException("Failed to read document's font family");
			Logger.LogDebug("Read document's font family: {0}", fontFamily);
			this.FontFamily = fontFamily;

			//	Read document's staff color
			Color staffColor;
			count = StreamHelper.ReadRGBA(stream, out staffColor);
			if (count == 0)
				throw new EndOfStreamException("Failed to read staff color");
			Logger.LogDebug("Read document's staff color: {0}", staffColor);
			this.StaffColor = staffColor;

			//	Read document's neumes color
			Color neumesColor;
			count = StreamHelper.ReadRGBA(stream, out neumesColor);
			if (count == 0)
				throw new EndOfStreamException("Failed to read neumes color");
			Logger.LogDebug("Read document's neumes color: {0}", neumesColor);
			this.NeumesColor = neumesColor;

			//	Read document's font color
			Color fontColor;
			count = StreamHelper.ReadRGBA(stream, out fontColor);
			if (count == 0)
				throw new EndOfStreamException("Failed to read font color");
			Logger.LogDebug("Read document's font color: {0}", fontColor);
			this.FontColor = fontColor;

			//	Read document's font size
			ushort spaceUnderStaff;
			count = StreamHelper.ReadWORD(stream, out spaceUnderStaff);
			if (count == 0)
				throw new EndOfStreamException("Failed to read the distance between staff and text");
			Logger.LogDebug("Read the distance between staff and text: {0:D}", spaceUnderStaff);
			this.SpaceUnderStaff = (short)spaceUnderStaff;

			return count;
		}

		/// <summary>
		/// Reads the staff from given stream and adds it to this document.
		/// </summary>
		/// <returns>The staff.</returns>
		/// <param name="stream">Input stream.</param>
		public GRG2Staff ReadStaff(Stream stream)
		{
			var staff = new GRG2Staff(stream);
			this.Staffs.Add(staff);
			return staff;
		}
	}
}

