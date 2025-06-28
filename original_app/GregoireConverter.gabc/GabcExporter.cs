using System;
using GregoireConverter.GRG;
using System.Resources;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Runtime.Remoting.Messaging;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel;

namespace GregoireConverter.gabc
{
	public class GabcExporter : IGRGExporter
	{
		public bool CreatesSingleFile => false;
		public static readonly string GabcTemplateTex = "GregoireConverter.gabc.Resources.gabcTemplate.tex";
		public static readonly string BaseFileNameMarker = "<@!BASE_FILE_NAME!@>";
		public static readonly string DocumentFontSizeMarker = "<@!DOCUMENT_FONT_SIZE!@>";
		public static readonly string OrientationMarker = "<@!ORIENTATION!@>";
		public static readonly string StaffWidthMarker = "<@!STAFF_WIDTH!@>";

		public GabcExporter ()
		{
		}

		public int Save(GRG2 gregoireDocument, string baseDir, string baseFileName, bool overwrite = false)
		{
			//	Check arguments
			if (gregoireDocument == null)
				throw new ArgumentNullException("gregoireDocument", "Argument cannot be null!");
			if (string.IsNullOrWhiteSpace(baseDir))
				throw new ArgumentNullException("baseDir", "Argument cannot be null!");
			if (string.IsNullOrWhiteSpace(baseFileName))
				throw new ArgumentNullException("baseFileName", "Argument cannot be null!");

			//	Ensure that the destination directory is in place
			var dir = new DirectoryInfo(baseDir);
			if (!dir.Exists)
			{
				dir.Create();
			}

			//	Set files names
			var outputTexFile = baseFileName + ".tex";
			var outputGabcFile = baseFileName + ".gabc";

			//	Read TeX template file and save result TeX file
			var assembly = Assembly.GetExecutingAssembly();

			using (var stream = assembly.GetManifestResourceStream(GabcTemplateTex))
			using (var reader = new StreamReader(stream))
			{
				var texTemplate = reader.ReadToEnd();
				using (var outTex = new StreamWriter(Path.Combine(baseDir, outputTexFile)))
				{
					outTex.Write(FillTexTemplate(gregoireDocument, texTemplate, baseFileName));
				}
			}

			//	Generate gabc output file
			var header = new GabcHeader(baseFileName).AddInitial(gregoireDocument.Documents.First());
			var content = GenerateGabcPart(gregoireDocument.Documents.First());

			using (var outGabc = new StreamWriter(Path.Combine(baseDir, outputGabcFile)))
			{
				outGabc.Write(header.ToString());
				outGabc.Write(content.ToString());
			}

			return 0;
		}

		public string FillTexTemplate(GRG2 gregoire, string template, string baseFileName)
		{
			var result = template.Replace(BaseFileNameMarker, baseFileName);

			//	Document properties
			var document = gregoire.Documents.First();
			result = result.Replace(DocumentFontSizeMarker, string.Format("{0:D}", document.FontSize));

			var orientation = "portrait";
			var maxWidth = (int)(1.1*document.Staffs.Max((s) => s.Width)); // gabc staffs are about 10% longer then Gregoire's
			if (maxWidth > 190) // 190mm width staff won't fit in portrait mode
			{
				orientation = "landscape";
			}
			result = result.Replace(OrientationMarker, orientation).Replace(StaffWidthMarker, maxWidth.ToString());

			return result;
		}

		public string GenerateGabcPart(GRG2Document document)
		{
			var result = new StringBuilder();
			GRG2NeumeForGabc lastClef = null;

			for(int i = 0; i < document.Staffs.Count; i++)
			{
				ConvertStaff(document.Staffs[i], i, ref result, ref lastClef);
			}

			return result.ToString();
		}

		public void ConvertStaff(GRG2Staff staff, int staffIdx, ref StringBuilder result, ref GRG2NeumeForGabc lastClef)
		{
			var initialLetter = (staff.Initial != null)? staff.Initial.InitialCaption : string.Empty;

			var textBuf = string.Empty;
			var neumeBuf = string.Empty;

			var neumes = staff.Neumes.OrderBy(n => n.PositionY).OrderBy(n => n.PositionX)
				.Select((n) => new GRG2NeumeForGabc(n)).ToList();

			//	Translate neumes
			for (var it = 0; it < neumes.Count; it++)
			{
				var currentNeume = neumes[it];

				var separator = (it > 0)? GetSeparator(neumes[it - 1], currentNeume) : null;
				//	Captions are not mergeable, so if this neume has next caption, buffers must be empty
				if ((!string.IsNullOrWhiteSpace(textBuf) && !string.IsNullOrWhiteSpace(currentNeume.Caption)))
				{
					FlushBuffers(ref textBuf, ref neumeBuf, ref result);
				}
				//	If the distance is too big to merge those neumes, then also flush.
				else if (separator == null)
				{
					FlushBuffers(ref textBuf, ref neumeBuf, ref result);
				}

				//	Check if this is clef
				if (currentNeume.IsClef)
				{
					if (lastClef != null
						&& lastClef.GetClefIndicator(false)
						.Equals(currentNeume.GetClefIndicator(false)))
					{
						// This is clef and it is the same as the previous one, so it should be skipped.
						continue;
					}

					FlushBuffers(ref textBuf, ref neumeBuf, ref result);

					//	Check if next symbol is bemol. If yes, then combine clef with bemol.
					if (neumes[it + 1].IsBemol
						&& (neumes[it+1].PositionY == currentNeume.PositionY + 6))
					{
						it++;
						neumeBuf += currentNeume.GetClefIndicator(true);
					}
					else
					{
						neumeBuf += currentNeume.GetClefIndicator(false);
					}
					lastClef = currentNeume;

					// Clef should be the first sign of the file
					FlushBuffers(ref textBuf, ref neumeBuf, ref result);
					continue;
				}

				// Custos at the end is added automatically
				if (currentNeume.IsCustos && it == neumes.Count - 1)
				{
					continue;
				}

				// Vertical lines are treated a bit differently;
				if (currentNeume.IsDivisio)
				{
					FlushBuffers(ref textBuf, ref neumeBuf, ref result);
					textBuf += (!string.IsNullOrWhiteSpace(currentNeume.Caption))? currentNeume.Caption : " ";
					try
					{
						neumeBuf = currentNeume.TranslateNeume();
					}
					catch (ArgumentOutOfRangeException ex)
					{
						Logger.LogError(ex);
						Logger.LogWarning("Skipping this neume");
					}

					if (!string.IsNullOrWhiteSpace(neumeBuf))
					{
						FlushBuffers(ref textBuf, ref neumeBuf, ref result);
						textBuf += " ";
					}
					continue;
				}

				//	Check if there is some text for this neume
				if (!string.IsNullOrWhiteSpace(currentNeume.Caption))
				{
					// Initial have to be first letter of text
					if (!string.IsNullOrWhiteSpace(initialLetter))
					{
						textBuf += initialLetter;
						initialLetter = string.Empty;
					}

					// Special case - star in Caption. Stars should be bound to divisio in gabc
					var match = Regex.Match(currentNeume.Caption, @"([*\ ]+)$");
					if (match.Success && neumes[it + 1].IsDivisio)
					{
						currentNeume.Caption = currentNeume.Caption
							.Remove(currentNeume.Caption.Length - match.Groups[1].Length);

						neumes[it + 1].Caption = " *" + neumes[it + 1].Caption;
					}

					textBuf += currentNeume.Caption;
				}

				try
				{
					var neume = currentNeume.TranslateNeume();
					if (!string.IsNullOrWhiteSpace(neume))
						neumeBuf += separator + neume;
				}
				catch (ArgumentOutOfRangeException ex)
				{
					Logger.LogError(ex);
					Logger.LogWarning("Skipping this neume!");
				}
			}

			FlushBuffers(ref textBuf, ref neumeBuf, ref result);

			//	End the line end set justification flag
			result.AppendLine((staff.Justify > 0)? "(z)\n" : "(Z)\n");
		}

		private void FlushBuffers(ref string textBuf, ref string neumeBuf, ref StringBuilder sb)
		{
			if (!(string.IsNullOrWhiteSpace(textBuf) && string.IsNullOrWhiteSpace(neumeBuf))) 
			{
				sb.AppendFormat("{0}({1})", textBuf.TrimEnd(), neumeBuf);
				
				textBuf = (textBuf.EndsWith(" "))? " " : string.Empty;
				neumeBuf = string.Empty;
			}
		}

		private string GetSeparator(GRG2NeumeForGabc previous, GRG2NeumeForGabc current)
		{
			var distance = current.PositionX - (previous.PositionX + previous.SizeX);

			if (distance < 1)
				return "!";
			if (distance < 3)
				return "/";
			if (distance < 5)
				return "//";
			if (distance < 7)
				return " ";

			return null;
		}
	}
}

