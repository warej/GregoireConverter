using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GregoireConverter.GRG;

namespace GregoireConverter.gabc
{
	public class GabcHeader
	{
		/// <summary>
		/// More info about header attributes: http://gregorio-project.github.io/gabc/#header
		/// </summary>
		public enum AttributeType
		{
			name,
			gabc__copyright,
			score__copyright,
			office__part,
			occasion,
			meter,
			commentary,
			arranger,
			author,
			date,
			manuscript,
			manuscript__reference,
			manuscript__storage__place,
			book,
			language,
			transcriber,
			transcription__date,
			mode,
			initial__style,
			user__notes,
			annotation,
			generated__by
		}

		public IList<KeyValuePair<AttributeType, string>> Attributes;
		public string Name { get; private set; }

		public GabcHeader (string name)
		{
			Attributes = new List<KeyValuePair<AttributeType, string>>();
			Name = name;
			this.Add(AttributeType.name, name)
				.Add(AttributeType.generated__by, "GregoireConverter")
				.Add(AttributeType.transcription__date, string.Format("{0:G}", DateTime.Now));
		}

		public GabcHeader Add(AttributeType type, string value)
		{
			Attributes.Add(new KeyValuePair<AttributeType, string>(type, value));
			return this;
		}

		public GabcHeader AddInitial(GRG2Document doc)
		{
			var initial = doc.Staffs.First().Initial;
			if (initial != null)
			{
				this.Add(AttributeType.initial__style, "1");

				if (!string.IsNullOrWhiteSpace(initial.AntiphonCaption))
					this.Add(AttributeType.annotation, initial.AntiphonCaption);
				
				if (!string.IsNullOrWhiteSpace(initial.ModusCaption))
				{
					//	Modus is a second attribute of type "annotation", so the first must be present
					if (!Attributes.Any(pair => pair.Key == AttributeType.annotation))
						this.Add(AttributeType.annotation, "");

					this.Add(AttributeType.annotation, initial.ModusCaption);
				}
			}
			else
				this.Add(AttributeType.initial__style, "0");

			return this;
		}

		public override string ToString()
		{
			var header = string.Format(
				"% !TEX TS-program = LuaLaTeX+se\n" +
				"% !TEX root = {0}.tex\n" +
				"\n" +
				"",
				Name);
			
			foreach (var attr in Attributes)
			{
				header += string.Format("{0}:{1};\n", attr.Key.ToString().Replace("__", "-"), attr.Value);
			}

			return header + "\n%%\n\n";
		}
	}
}

