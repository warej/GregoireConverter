using System;
using System.IO;

namespace GregoireConverter.GRG
{
	/// <summary>
	/// Interface represening GRG2Segment
	/// </summary>
	public interface IGRG2Segment
	{
		/// <summary>
		/// Segment type
		/// </summary>
		/// <value>The type of the segment</value>
		GRG2.SegmentType Type { get; }

		/// <summary>
		/// Reads the segment values from stream.
		/// </summary>
		/// <returns>Number of bytes from last read.</returns>
		/// <param name="stream">Source stream.</param>
		int ReadStream(Stream stream);
	}
}

