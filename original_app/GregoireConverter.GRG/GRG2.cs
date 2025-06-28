using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;

namespace GregoireConverter.GRG
{
	public class GRG2
	{
		public const int EMPTY_GRG2_FILE_SIZE = 0x28;

		public enum SegmentType : ushort
		{
			DOCUMENT	= 0xf1ff,
			STAFF		= 0xf2ff,
			INITIAL		= 0xf3ff,
			NEUME		= 0xf4ff
		}

		#region properties
		public IList<GRG2Document> Documents { get; private set; }
		#endregion

		#region public methods
		public GRG2 ()
		{
			Documents = new List<GRG2Document>();
		}

		/// <summary>
		/// Read GRG2 object from given file.
		/// </summary>
		/// <param name="path">Path to the GRG2 file.</param>
		public void ReadFile(string path)
		{
			if (!File.Exists(path))
			{
				throw new FileNotFoundException("Failed to read GRG2 file {0}, because it does not exist.", path);
			}

			Logger.LogInfo("Reading GRG2 file: {0}", path);
			using (var fs = new FileStream(path, FileMode.Open))
			{
				ReadStream(fs);
			}
			Logger.LogInfo("Reading GRG2 file success!");
		}

		/// <summary>
		/// Read GRG2 file from given stream.
		/// </summary>
		/// <param name="stream">Byte stream in th GRG2 format (e.g. FileStream).</param>
		public void ReadStream(Stream stream)
		{
			int count;

			//	Initial stream length check
			Logger.LogVerbose("Stream has {0} bytes.", stream.Length.ToString ());
			if (stream.Length < EMPTY_GRG2_FILE_SIZE)
				throw new FileLoadException("File is shorter then empty GRG2 file.");

			//	Read stream's header
			string header;
			count = StreamHelper.ReadString(stream, 0x04, out header);
			if (count == 0)
				throw new EndOfStreamException("The stream is empty!");

			Logger.LogVerbose("File header: {0}", header);
			if (!header.Equals("GRG2"))
				throw new NotSupportedException(string.Format("'{0}' is not a GRG2 format header", header));

			//	Read the rest of the stream
			var lastRead = 0x04;
			while (lastRead > 0)
			{
				lastRead = ReadSegment(stream);
			}

			foreach (var doc in Documents)
			{
				foreach (var staff in doc.Staffs)
				{
					staff.Neumes.Sort((GRG2Neume x, GRG2Neume y) => x.PositionX.CompareTo(y.PositionX));
					staff.AssignRhytmics();
				}
			}
		}
		#endregion

		#region private methods
		/// <summary>
		/// Reads the segment of GRG2 file.
		/// </summary>
		/// <returns>Number of bytes that were read last time. 0 means end of stream</returns>
		/// <param name="stream">Source bytes stream</param>
		private int ReadSegment(Stream stream)
		{
			int count;
			IGRG2Segment segment;

			//	Read segment type
			ushort segmentType;
			count = StreamHelper.ReadWORD(stream, out segmentType);

			//	End of the stream
			if (count == 0)
				return count;

			//	Load the segment values depending on it's type
			switch ((SegmentType)segmentType)
			{
			case SegmentType.DOCUMENT:
				Documents.Add(new GRG2Document(stream));
				break;
			case SegmentType.STAFF:
				segment = Documents.Last();
				(segment as GRG2Document).ReadStaff(stream);
				break;
			case SegmentType.INITIAL:
				segment = Documents.Last();
				segment = (segment as GRG2Document).Staffs.Last();
				(segment as GRG2Staff).ReadInitial(stream);
				break;
			case SegmentType.NEUME:
				segment = Documents.Last();
				segment = (segment as GRG2Document).Staffs.Last();
				(segment as GRG2Staff).ReadNeume(stream);
				break;
			default:
				throw new ArgumentException(
					string.Format("Segment of type {0} was not recognized as a GRG2 file segment.", segmentType));
			}

			return count;
		}
		#endregion
	}
}

