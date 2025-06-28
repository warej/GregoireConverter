using System;
using System.IO;
using System.Text;
using System.Runtime.Remoting.Messaging;

namespace GregoireConverter.GRG
{
	public class StreamHelper
	{
		public StreamHelper ()
		{
		}

		/// <summary>
		/// Reads the ASCII string from given stream.
		/// </summary>
		/// <returns>number of bytes read</returns>
		/// <param name="stream">source stream</param>
		/// <param name="length">number of bytes to read</param>
		/// <param name="result">read string</param>
		public static int ReadString(Stream stream, int length, out string result)
		{
			return ReadString(stream, length, Encoding.UTF7, out result);
		}

		/// <summary>
		/// Reads the ASCII string from given stream.
		/// </summary>
		/// <returns>number of bytes read</returns>
		/// <param name="stream">source stream</param>
		/// <param name="length">Number of bytes to read</param>
		/// <param name="encoding">source string encoding</param>
		/// <param name="result">read string</param>
		public static int ReadString(Stream stream, int length, Encoding encoding, out string result)
		{
			result = string.Empty;

			//	Validate given length
			if (length <= 0)
				throw new ArgumentOutOfRangeException("'length' argument must have positive value");

			var buffer = new byte[length];

			//	Read from the stream
			var count = stream.Read(buffer, 0, length);

			//	Is it the end of the stream
			if (count == 0)
				return count;

			//	Is the stream ended in half or corrupted
			if (count < length)
				throw new EndOfStreamException("Unexpected end of the input stream.");

			//	Decode the string
			int realLength = 0;
			for (; (realLength < buffer.Length) && (buffer[realLength] != 0); realLength++);
			result = encoding.GetString(buffer, 0, realLength);
			return count;
		}

		/// <summary>
		/// Reads one byte from given stream.
		/// </summary>
		/// <returns>number of bytes read</returns>
		/// <param name="stream">source stream</param>
		/// <param name="result">read byte</param>
		public static int ReadByte(Stream stream, out byte result)
		{
			result = byte.MaxValue;
			var buffer = new byte[1];

			//	Read from the stream
			var count = stream.Read(buffer, 0, 0x01);

			//	Is it the end of the stream?
			if (count == 0)
				return count;

			//	Return the result
			result = buffer[0];
			return count;
		}

		/// <summary>
		/// Reads one WORD (2B) from given stream
		/// </summary>
		/// <returns>number of bytes read</returns>
		/// <param name="stream">source stream</param>
		/// <param name="result">read WORD</param>
		public static int ReadWORD(Stream stream, out ushort result)
		{
			result = ushort.MaxValue;
			var WORDLength = 0x02;
			var buffer = new byte[WORDLength];

			//	Read from the stream
			var count = stream.Read(buffer, 0, WORDLength);

			//	Is it the end of the stream?
			if (count == 0)
				return count;

			//	Is the stream ended in half or corrupted
			if (count < WORDLength)
				throw new EndOfStreamException("Unexpected end of the input stream.");

			//	Return the result
			result = BitConverter.ToUInt16(buffer, 0);
			return count;
		}

		/// <summary>
		/// Reads one DWORD (4B) from given stream
		/// </summary>
		/// <returns>number of bytes read</returns>
		/// <param name="stream">source stream</param>
		/// <param name="result">read DWORD</param>
		public static int ReadDWORD(Stream stream, out int result)
		{
			result = int.MaxValue;
			var WORDLength = 0x04;
			var buffer = new byte[WORDLength];

			//	Read from the stream
			var count = stream.Read(buffer, 0, WORDLength);

			//	Is it the end of the stream?
			if (count == 0)
				return count;

			//	Is the stream ended in half or corrupted
			if (count < WORDLength)
				throw new EndOfStreamException("Unexpected end of the input stream.");

			//	Return the result
			result = BitConverter.ToInt32(buffer, 0);
			return count;
		}

		/// <summary>
		/// Reads color in RGBA(4B) from given stream
		/// </summary>
		/// <returns>number of bytes read</returns>
		/// <param name="stream">source stream</param>
		/// <param name="result">read color</param>
		public static int ReadRGBA(Stream stream, out Color result)
		{
			result = null;
			var ColorLength = 0x04;
			var buffer = new byte[ColorLength];

			//	Read from the stream
			var count = stream.Read(buffer, 0, ColorLength);

			//	Is it the end of the stream?
			if (count == 0)
				return count;

			//	Is the stream ended in half or corrupted
			if (count < ColorLength)
				throw new EndOfStreamException("Unexpected end of the input stream.");

			//	Return the result
			result = new Color(buffer);
			return count;
		}
	}
}

