using System;
using System.IO;

namespace GregoireConverter.GRG
{
	/// <summary>
	/// Class representing GRG2 initial
	/// </summary>
	public class GRG2Initial : IGRG2Segment
	{
		public GRG2.SegmentType Type { get {return GRG2.SegmentType.INITIAL;} }
		public ushort SegmentLength { get; private set; }
		public int Width { get; private set; }

		public ushort AntiphonFontSize { get; private set; }
		public string AntiphonFontFamily { get; private set; }
		public string AntiphonCaption { get; private set; }

		public ushort ModusFontSize { get; private set; }
		public string ModusFontFamily { get; private set; }
		public string ModusCaption { get; private set; }

		public ushort InitialFontSize { get; private set; }
		public string InitialFontFamily { get; private set; }
		public string InitialCaption { get; private set; }
		
		public GRG2Initial ()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GregoireConverter.GRG.GRG2Initial"/> class
		/// loading it's content from given stream.
		/// </summary>
		/// <param name="stream">Source stream.</param>
		public GRG2Initial(Stream stream)
		{
			var count = ReadStream(stream);

			if (count == 0)
				throw new EndOfStreamException("Unexpected end of Initial structure");
		}

		/// <summary>
		/// Reads the initial segment from stream.
		/// </summary>
		/// <returns>Number of bytes from last read.</returns>
		/// <param name="stream">Source stream.</param>
		public int ReadStream(Stream stream)
		{
			int count;

			//	Read the length of the segment. It is hardcoded in the gregoire binary so it's not likely to change.
			ushort segmentLength, expectedSegmentLength = 0x006A; // 106
			count = StreamHelper.ReadWORD(stream, out segmentLength);
			if (count == 0)
				throw new EndOfStreamException("Failed to read the length of the initial segment");
			if (segmentLength != expectedSegmentLength)
				Logger.LogWarning("Unexpected length of the segment. The length should be {0} and is {1}",
					expectedSegmentLength.ToString(), segmentLength.ToString());
			else
				Logger.LogVerbose("Read the length of the initial segment: {0:D3}", segmentLength);
			this.SegmentLength = segmentLength;

			//	Read initial's width
			int width;
			count = StreamHelper.ReadDWORD(stream, out width);
			if (count == 0)
				throw new EndOfStreamException("Failed to read initial's width");
			Logger.LogInfo("Read initial's width: {0:D2}", width);
			this.Width = width;

			//	Read antiphon
			//	Read initial's antiphon font size
			ushort antFontSize;
			count = StreamHelper.ReadWORD(stream, out antFontSize);
			if (count == 0)
				throw new EndOfStreamException("Failed to read the initial's antiphon font size");
			this.AntiphonFontSize = antFontSize;

			//	Read initial's antiphon font family
			string antFontFamily;
			count = StreamHelper.ReadString(stream, 0x10, out antFontFamily);
			if (count == 0)
				throw new EndOfStreamException("Failed to read initial's antiphon font family");
			this.AntiphonFontFamily = antFontFamily;

			//	Read initial's antiphon caption
			string antCaption;
			count = StreamHelper.ReadString(stream, 0x10, out antCaption);
			if (count == 0)
				throw new EndOfStreamException("Failed to read initial's antiphon caption");
			this.AntiphonCaption = antCaption;
			if (!string.IsNullOrWhiteSpace(antCaption))
			{
				Logger.LogDebug("Read initial's antiphon font size: {0:D}", antFontSize);
				Logger.LogDebug("Read initial's antiphon font family: {0}", antFontFamily);
				Logger.LogDebug("Read initial's antiphon caption: {0}", antCaption);
			}
			else
			{
				Logger.LogVerbose("Read initial's antiphon font size: {0:D}", antFontSize);
				Logger.LogVerbose("Read initial's antiphon font family: {0}", antFontFamily);
				Logger.LogVerbose("Read initial's antiphon caption: {0}", antCaption);
			}

			//	Read modus
			//	Read initial's modus font size
			ushort modusFontSize;
			count = StreamHelper.ReadWORD(stream, out modusFontSize);
			if (count == 0)
				throw new EndOfStreamException("Failed to read the initial's modus font size");
			this.ModusFontSize = modusFontSize;

			//	Read initial's modus font family
			string modusFontFamily;
			count = StreamHelper.ReadString(stream, 0x10, out modusFontFamily);
			if (count == 0)
				throw new EndOfStreamException("Failed to read initial's modus font family");
			this.ModusFontFamily = modusFontFamily;

			//	Read initial's modus caption
			string modusCaption;
			count = StreamHelper.ReadString(stream, 0x10, out modusCaption);
			if (count == 0)
				throw new EndOfStreamException("Failed to read initial's modus caption");
			this.ModusCaption = modusCaption;
			if (!string.IsNullOrWhiteSpace(modusCaption))
			{
				Logger.LogDebug("Read initial's modus font size: {0:D}", modusFontSize);
				Logger.LogDebug("Read initial's modus font family: {0}", modusFontFamily);
				Logger.LogDebug("Read initial's modus caption: {0}", modusCaption);
			}
			else
			{
				Logger.LogVerbose("Read initial's modus font size: {0:D}", modusFontSize);
				Logger.LogVerbose("Read initial's modus font family: {0}", modusFontFamily);
				Logger.LogVerbose("Read initial's modus caption: {0}", modusCaption);
			}

			//	Read initial
			//	Read initial's font size
			ushort initialFontSize;
			count = StreamHelper.ReadWORD(stream, out initialFontSize);
			if (count == 0)
				throw new EndOfStreamException("Failed to read the initial's font size");
			this.InitialFontSize = initialFontSize;

			//	Read initial's font family
			string initialFontFamily;
			count = StreamHelper.ReadString(stream, 0x10, out initialFontFamily);
			if (count == 0)
				throw new EndOfStreamException("Failed to read initial's font family");
			this.InitialFontFamily = initialFontFamily;

			//	Read initial's caption
			string initialCaption;
			count = StreamHelper.ReadString(stream, 0x10, out initialCaption);
			if (count == 0)
				throw new EndOfStreamException("Failed to read initial's caption");
			this.InitialCaption = initialCaption;
			if (!string.IsNullOrWhiteSpace(initialCaption))
			{
				Logger.LogDebug("Read initial's font size: {0:D}", initialFontSize);
				Logger.LogDebug("Read initial's font family: {0}", initialFontFamily);
				Logger.LogDebug("Read initial's caption: {0}", initialCaption);
			}
			else
			{
				Logger.LogVerbose("Read initial's font size: {0:D}", initialFontSize);
				Logger.LogVerbose("Read initial's font family: {0}", initialFontFamily);
				Logger.LogVerbose("Read initial's caption: {0}", initialCaption);
			}

			return count;
		}
	}
}

