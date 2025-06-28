using System;
using System.IO;
using System.Collections.Generic;

namespace GregoireConverter.GRG
{
	/// <summary>
	/// Class representing GRG2 4-line staff
	/// </summary>
	public class GRG2Staff : IGRG2Segment
	{
		public GRG2.SegmentType Type { get {return GRG2.SegmentType.STAFF;} }
		public ushort SegmentLength { get; private set; }
		public ushort Width { get; private set; }
		public byte Justify { get; private set; }
		public GRG2Initial Initial { get; private set; }
		public List<GRG2Neume> Neumes { get; private set; }

		public GRG2Staff ()
		{
			Neumes = new List<GRG2Neume>();
			Initial = null;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GregoireConverter.GRG.GRG2Staff"/> class
		/// loading it's content from given stream.
		/// </summary>
		/// <param name="stream">Source stream.</param>
		public GRG2Staff (Stream stream)
		{
			Neumes = new List<GRG2Neume>();
			Initial = null;
			var count = ReadStream(stream);

			if (count == 0)
				throw new EndOfStreamException("Unexpected end of Staff structure");
		}

		/// <summary>
		/// Reads the Staff segment from stream.
		/// </summary>
		/// <returns>Number of bytes from last read.</returns>
		/// <param name="stream">Source stream.</param>
		public int ReadStream(Stream stream)
		{
			int count;

			//	Read the length of the segment. It is hardcoded in the gregoire binary so it's not likely to change.
			ushort segmentLength, expectedSegmentLength = 0x0003;
			count = StreamHelper.ReadWORD(stream, out segmentLength);
			if (count == 0)
				throw new EndOfStreamException("Failed to read the length of the staff segment");
			if (segmentLength != expectedSegmentLength)
				Logger.LogWarning("Unexpected length of the segment. The length should be {0} and is {1}",
					expectedSegmentLength.ToString(), segmentLength.ToString());
			else
				Logger.LogVerbose("Read the length of the staff segment: {0:D3}", segmentLength);
			this.SegmentLength = segmentLength;

			//	Read staff's width
			ushort width;
			count = StreamHelper.ReadWORD(stream, out width);
			if (count == 0)
				throw new EndOfStreamException("Failed to read staff's width");
			Logger.LogDebug("Read staff's width: {0:D2}mm", width);
			this.Width = width;

			//	Read staffs's justification flag
			byte justify;
			count = StreamHelper.ReadByte(stream, out justify);
			if (count == 0)
				throw new EndOfStreamException("Failed to read document's font family");
			Logger.LogDebug("Read staffs's justification flag: {0}", justify);
			this.Justify = justify;

			return count;
		}

		/// <summary>
		/// Reads the initial from given stream and adds it to this staff.
		/// </summary>
		/// <returns>The initial.</returns>
		/// <param name="stream">Input stream.</param>
		public GRG2Initial ReadInitial(Stream stream)
		{
			var initial = new GRG2Initial(stream);
			this.Initial = initial;
			return initial;
		}

		/// <summary>
		/// Reads the neume from given stream and adds it to this staff.
		/// </summary>
		/// <returns>The neume.</returns>
		/// <param name="stream">Input stream.</param>
		public GRG2Neume ReadNeume(Stream stream)
		{
			var neume = new GRG2Neume(stream);
			this.Neumes.Add(neume);
			return neume;
		}

		public void AssignRhytmics()
		{
			foreach (var neume in Neumes)
			{
				neume.Rhythmics.Clear();
			}

			for (var it = 0; it < Neumes.Count; it++)
			{
				if (!Neumes[it].IsRhythmic)
					continue;

				//	Find closest neume
				GRG2Neume closest = null;
				//	Look backwards
				int i = it-1;
				while (closest == null && i > 0)
				{
					if (!Neumes[i].IsRhythmic)
						closest = Neumes[i];
					i--;
				}
				//	Look forward
				i = it+1;
				while (i < Neumes.Count)
				{
					if (!Neumes[i].IsRhythmic)
					{
						var closestDist = Neumes[it].PositionX - closest.PositionX;
						var iDist = Neumes[i].PositionX - Neumes[it].PositionX;
						if (iDist < 3 && iDist < closestDist)
							closest = Neumes[i];
						break;
					}
					i++;
				}

				if (closest != null)
				{
					closest.Rhythmics.Add(Neumes[it]);
					Neumes.RemoveAt(it--);
				}
			}
		}
	}
}

