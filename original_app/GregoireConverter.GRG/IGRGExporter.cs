using System;

namespace GregoireConverter.GRG
{
	public interface IGRGExporter
	{
		/// <summary>0
		/// Gets a value indicating whether this <see cref="GregoireConverter.GRG.IGRGExporter"/> creates a single file
		/// or multiple files.
		/// </summary>
		/// <value><c>true</c> if creates single file; otherwise, <c>false</c>.</value>
		bool CreatesSingleFile { get; }

		/// <summary>
		/// Exports the specified gregoire document.
		/// </summary>
		/// <param name="gregoireDocument">Document to export</param>
		/// <param name="baseDir">Directory to store the result file(s).
		/// The top level directory will be created if doesn't exist.</param>
		/// <param name="baseFileName">Base for the file name. Result file(s) will have proper extension(s).</param>
		/// <param name="overwrite">True indicates that any existing files should be overwritten.
		/// Throws an exception when false and such file(s) already exist.</param>
		/// <returns>Program's exit value</returns>
		int Save(GRG2 gregoireDocument, string baseDir, string baseFileName, bool overwrite = false);
	}
}

