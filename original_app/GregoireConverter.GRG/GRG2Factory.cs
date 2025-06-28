using System;
using System.IO;

namespace GregoireConverter.GRG
{
	/// <summary>
	/// Factory of gregoriant chants represented as a Gregoire objects
	/// </summary>
	public class GRG2Factory
	{
		public GRG2Factory ()
		{
		}

		/// <summary>
		/// [Null] Loads Gregoire object from file.
		/// </summary>
		/// <returns>If path points to a valid file, the Gregoire object is returned. null value means loading failure.</returns>
		/// <param name="path">Gregoire file's path.</param>
		public static GRG2 FromFile(string path)
		{
			var grg = new GRG2();

			try
			{
				grg.ReadFile(path);
				return grg;
			}
			catch (Exception e)
			{
				Logger.LogError(e);
				return null;
			}
		}
	}
}

