using System;
using GregoireConverter.GRG;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Linq;
using System.Threading;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace GregoireConverter.gabc
{
	public class GRG2NeumeForGabc : GRG2Neume
	{
		public int SizeX { get { return (_format != null) ? _format.SizeX : 0; } }
		private List<GRG2NeumeForGabc> _rhythmics;

		internal NeumeFormat _format;

		public GRG2NeumeForGabc (GRG2Neume origNeume)
		{
			SegmentLength = origNeume.SegmentLength;
			Id = origNeume.Id;
			PositionX = origNeume.PositionX;
			PositionY = origNeume.PositionY;
			UnknownValue4 = origNeume.UnknownValue4;
			UnknownValue5 = origNeume.UnknownValue5;

			// Reformatting the text under the neume
			var match = Regex.Match(origNeume.Caption, @"([\-\ ]+)$");
			if (match.Success)
			{
				Caption = origNeume.Caption.Remove(origNeume.Caption.Length - match.Groups[1].Length);

				if (match.Groups[1].Value.ToString().Contains(" "))
					Caption += " ";
				else
					Caption += "";
			}
			else
				Caption = origNeume.Caption;

			//	Read size
			if (NeumeFormats.ContainsKey(origNeume.Id))
			{
				_format = NeumeFormats[origNeume.Id];
			}
			else
			{
				Logger.LogError(new ArgumentOutOfRangeException(
					string.Format("Cannot find format for nemue with ID {0}", origNeume.Id)));
				_format = null;
			}

			//	Convert rhytmics
			_rhythmics = new List<GRG2NeumeForGabc>();
			if (origNeume.Rhythmics != null)
			{
				foreach (var r in origNeume.Rhythmics)
				{
					var forGabc = new GRG2NeumeForGabc(r);
					_rhythmics.Add(forGabc);
				}
			}
		}

		public string TranslateNeume()
		{
			if (_format == null)
				return "";

			foreach(var part in _format.Parts)
				ResolvePartFormat(part);
			
			foreach (var rhythmic in _rhythmics) // Point
			{
				ApplyRhythmic(rhythmic);
			}

			return _format.ToString();
		}

		private void ApplyRhythmic(GRG2NeumeForGabc rhythmic)
		{
			try
			{
				// Currently there is no rhythmic sign with more than one part
				var rPart = rhythmic._format.Parts.First();
				if (rhythmic.Id == 23)
				{
					var applyToPart = _format.Parts.Last();

					applyToPart.After += rPart.Before;
				}
				else if (rhythmic.Id == 80) // Ictus
				{
					var applyToPart = rPart.FindClosestPart(rhythmic, this);

					if (NeumePart.Above(rhythmic, rPart, this, applyToPart))
						applyToPart.After += rPart.Before + "1"; // 1 means always above
					else if (NeumePart.Below(rhythmic, rPart, this, applyToPart))
						applyToPart.After += rPart.Before + "0"; // 0 means always below
					else
						applyToPart.After += rPart.Before;
				}
				else if (rhythmic.Id == 81 || rhythmic.Id == 82) // Episemas
				{
					foreach (var part in _format.Parts)
					{
						// Check horizontal overlap
						var subsetX = NeumePart.RangeSubset(
							rhythmic.PositionX + rPart.StartX,
							rhythmic.PositionX + rPart.StartX + rPart.SizeX,
							this.PositionX + part.StartX,
							this.PositionX + part.StartX + part.SizeX);

						// If there is only accidental hover don't take it into account
						if (subsetX < NeumePart.Min(2, part.SizeX / 2)) // Error margin
							continue;

						string toAdd;
						if (NeumePart.Above(rhythmic, rPart, this, part))
							toAdd = rPart.Before + "1";
						else if (NeumePart.Below(rhythmic, rPart, this, part))
							toAdd = rPart.Before + "0";
						else
							toAdd = rPart.Before;

						if (!part.After.Contains(toAdd))
							part.After += toAdd;
					}

					var last = _format.Parts.Last();
					if ((last.After.EndsWith(rPart.Before + "0")
						|| last.After.EndsWith(rPart.Before + "1")
						|| last.After.EndsWith(rPart.Before))
						&& ((PositionX + _format.SizeX) - (rhythmic.PositionX + rPart.StartX + rPart.SizeX) >= -1))
					{
						last.After += "2"; // Force breaking the line
					}
				}
			}
			catch (NullReferenceException)
			{
				// Shit happens
				;
				// To be precise, there can be no format for this neume
			}
		}

		public void ResolvePartFormat(NeumePart part)
		{
			var fmt = part.Height;
			//	Calculate gabc height modifier
			int baseHeight = (72 - PositionY) / 6;
			if (baseHeight < 0 || baseHeight > 12)
				throw new ArgumentOutOfRangeException(string.Format("Invalid neume base height: {0}", baseHeight));
			
			//	Lowercase x format resolving
			var pattern = @"(x(?:(?:\+|-)\d)?)";
			var match = Regex.Match(fmt, pattern);
			while (match.Success)
			{
				var toReplace = match.Groups[0].ToString();
				var expression = match.Groups[1].ToString();

				expression = expression.Replace("x", baseHeight.ToString());
				var modifier = Solve(expression);
				if (modifier < 0 || modifier > 12)
				{
					throw new ArgumentOutOfRangeException(
						string.Format("Modifier should belong to [0, 12] range and is {0}", modifier));
				}

				fmt = fmt.Replace(toReplace, "" + Convert.ToChar('a' + modifier));
				match = Regex.Match(fmt, pattern);
			}

			//	Uppercase X formatresolving
			pattern = @"(X(?:(?:\+|-)\d)?)";
			match = Regex.Match(fmt, pattern);
			while (match.Success)
			{
				var toReplace = match.Groups[0].ToString();
				var expression = match.Groups[1].ToString();

				expression = expression.Replace("X", baseHeight.ToString());
				var modifier = Solve(expression);

				fmt = fmt.Replace(toReplace, "" + Convert.ToChar('A' + modifier));
				match = Regex.Match(fmt, pattern);
			}

			//	Uppercase Q formatresolving
			pattern = @"(Q(?:(?:\+|-)\d)?)";
			match = Regex.Match(fmt, pattern);
			while (match.Success)
			{
				var toReplace = match.Groups[0].ToString();
				var expression = match.Groups[1].ToString();

				expression = expression.Replace("Q", GetClefIndicator(false));

				fmt = fmt.Replace(toReplace, expression);
				match = Regex.Match(fmt, pattern);
			}

			//	Uppercase N formatresolving
			pattern = @"(N(?:(?:\+|-)\d)?)";
			match = Regex.Match(fmt, pattern);
			while (match.Success)
			{
				var toReplace = match.Groups[0].ToString();
				var expression = match.Groups[1].ToString();

				int bHeight;

				if (PositionY <= 22)
					bHeight = 5;
				else if (PositionY <= 28)
					bHeight = 6;
				else if (PositionY <= 34)
					bHeight = 3;
				else if (PositionY <= 40)
					bHeight = 4;
				else if (PositionY <= 46)
					bHeight = 1;
				else
					bHeight = 2;
				
				expression = expression.Replace("N", bHeight.ToString());
				var modifier = Solve(expression);

				fmt = fmt.Replace(toReplace, modifier.ToString());
				match = Regex.Match(fmt, pattern);
			}

			part.Height = fmt;
		}

		public int Solve(string expression)
		{
			var resultString = expression;

			var match = Regex.Match(expression, @"(\d+)(\+|-)(\d+)");
			if (match.Success)
			{
				var sub1 = int.Parse(match.Groups[1].ToString());
				var sub2 = int.Parse(match.Groups[3].ToString());
				var sign = match.Groups[2].ToString();
				int result;

				result = (sign.Equals("+"))? sub1 + sub2 : sub1 - sub2;

				resultString = expression.Replace(match.Groups[0].ToString(), result.ToString());
				return Solve(resultString);
			}

			return int.Parse(resultString);
		}

		public bool Overlaps(GRG2NeumeForGabc otherNeume)
		{
			var overlap = NeumePart.RangeSubset(this.PositionX, this.PositionX + this.SizeX,
				otherNeume.PositionX, otherNeume.PositionX + otherNeume.SizeX);

			if (overlap > 1) // Margin of error
			{
				return true;
			}

			return false;
		}

		public override string ToString()
		{
			return string.Format("[NeumeForGabc:({0})'{1}'@{2},{3}]", Id, Caption, PositionX, PositionY);
		}

		public readonly Dictionary<int, NeumeFormat> NeumeFormats = new Dictionary<int, NeumeFormat>
		{
			//	(deprecated) String format: {sx=06}{0|X|>|6}
			//	Description:
			//	 - {sx=06} means that X size of neume is equal 6px
			//	 - {0|c|X|>|6} means that this part of whole neume:
			//		 - starts from '0px' offset
			//		 - has sign 'c' before height specializer
			//		 - should be place at given height
			//			 - x => [a-m],
			//			 - X => [A-M],
			//			 - Q => [1-4] for clefs,
			//			 - N => [1-6] for movable lines (separations)
			//		 - height specializer is followed by '>'
			//		 - has a X size of '6px'
			{  0, new NeumeFormat( 6, 10,	new NeumePart( 0,  0, "",	"X",	">",	 6, 10))}, // Apostropha TODO check!
			{  1, new NeumeFormat( 1, 27,	new NeumePart( 0,  5, ";",	"",		"",		 1, 22))}, // 1/2 Barre
			{  2, new NeumeFormat( 1, 14,	new NeumePart( 0,  6, ",",	"",		"",		 1,  8))}, // 1/4 Barre
			{  3, new NeumeFormat( 6, 46,	new NeumePart( 0,  9, "::",	"",		"",		 6, 35))}, // Double Barre
			{  4, new NeumeFormat( 1, 46,	new NeumePart( 0,  9, ":",	"",		"",		 1, 35))}, // Barre
			{  5, new NeumeFormat( 6, 13,	new NeumePart( 0,  1, "",	"x-1",	"x",	 6, 12))}, // Bemol
			{  6, new NeumeFormat( 7, 23,	new NeumePart( 0,  3, "",	"Q",	"",		 7, 20))}, // Cle do (special!)
			{  7, new NeumeFormat(12, 19,	new NeumePart( 0,  3, "",	"Q",	"",		12, 16))}, // Cle fa (special!)

			{  8, new NeumeFormat(12, 19,	new NeumePart( 0,  0, "",	"x",	"v",	 6,  7),
											new NeumePart( 7,  5, "!",	"X-1",	"",		 5, 10))}, // Climacus2
			
			{  9, new NeumeFormat(17, 20,	new NeumePart( 0,  0, "",	"x",	"v",	 6,  7),
											new NeumePart( 6,  5, "!",	"X-1",	"",		 5, 10),
											new NeumePart(10, 11, "",	"X-2",	"",		 5, 10))}, // Climacus3
			
			{ 10, new NeumeFormat(21, 26,	new NeumePart( 0,  0, "",	"x",	"v",	 6,  7),
											new NeumePart( 6,  5, "!",	"X-1",	"",		 5, 10),
											new NeumePart(10, 11, "",	"X-2",	"",		 5, 10),
											new NeumePart(14, 17, "",	"X-3",	"",		 5, 10))}, // Climacus4
			
			{ 11, new NeumeFormat(24, 33,	new NeumePart( 0,  0, "",	"x",	"v",	 6,  7),
											new NeumePart( 6,  5, "!",	"X-1",	"",		 5, 10),
											new NeumePart(10, 11, "",	"X-2",	"",		 5, 10),
											new NeumePart(14, 17, "",	"X-3",	"",		 5, 10),
											new NeumePart(18, 23, "",	"X-4",	"",		 5, 10))}, // Climacus5
			
			{ 12, new NeumeFormat(11, 17,	new NeumePart( 0,  2, "",	"x",	"",		 6,  7),
											new NeumePart( 5,  6, "",	"x-1",	"",		 6,  7))}, // Clivis2
			
			{ 13, new NeumeFormat(11, 19,	new NeumePart( 0,  0, "",	"x",	"",		 6,  7),
											new NeumePart( 5, 12, "",	"x-2",	"",		 6,  7))}, // Clivis3
			
			{ 14, new NeumeFormat(11, 25,	new NeumePart( 0,  0, "",	"x",	"",		 6,  7),
											new NeumePart( 5, 18, "",	"x-3",	"",		 6,  7))}, // Clivis4
			
			{ 15, new NeumeFormat(11, 32,	new NeumePart( 0,  0, "",	"x",	"",		 6,  7),
											new NeumePart( 5, 24, "",	"x-4",	"",		 6,  7))}, // Clivis5
			
			{ 16, new NeumeFormat( 7, 10,	new NeumePart( 0,  0, "",	"x",	"o",	 7, 10))}, // Oriscus TODO check! Duplicate 25?

			{ 17, new NeumeFormat(11, 16,	new NeumePart( 0,  6, "",	"x-1",	"o",	 7, 10),
											new NeumePart( 5,  0, "",	"x",	"",		 6,  7))}, // Pes Quassus 2
			
			{ 18, new NeumeFormat(11, 22,	new NeumePart( 0, 12, "",	"x-2",	"o",	 7, 10),
											new NeumePart( 5,  0, "",	"x",	"",		 6,  7))}, // Pes Quassus 3
			
			{ 19, new NeumeFormat( 6, 14,	new NeumePart( 0,  0, "",	"x-1",	"",		 6,  7),
											new NeumePart( 0,  8, "",	"x",	"",		 6,  7))}, // Podatus2
			
			{ 20, new NeumeFormat( 6, 19,	new NeumePart( 0,  0, "",	"x-2",	"",		 6,  7),
											new NeumePart( 0, 13, "",	"x",	"",		 6,  7))}, // Podatus3
			
			{ 21, new NeumeFormat( 6, 25,	new NeumePart( 0,  0, "",	"x-3",	"",		 6,  7),
											new NeumePart( 0, 19, "",	"x",	"",		 6,  7))}, // Podatus4
			
			{ 22, new NeumeFormat( 6, 31,	new NeumePart( 0,  0, "",	"x-4",	"",		 6,  7),
											new NeumePart( 0, 25, "",	"x",	"",		 6,  7))}, // Podatus5
			
			{ 23, new NeumeFormat( 3,  6,	new NeumePart( 0,  3, ".",	"",		"",		 3,  3))}, // Point
			{ 24, new NeumeFormat( 6,  7,	new NeumePart( 0,  0, "",	"x",	"",		 6,  7))}, // Punctum
			{ 25, new NeumeFormat( 6,  7,	new NeumePart( 0,  7, "",	"x",	"w",	 6,  7))}, // Quilisma TODO check! Duplicate of 16?

			{ 26, new NeumeFormat( 6, 14,	new NeumePart( 0, 0, "",	"x-1",	"w",	 6,  7),
											new NeumePart( 0, 7, "",	"x",	"",		 6,  7))}, // Quilisma-pes
			
			{ 27, new NeumeFormat(16, 19,	new NeumePart( 0, 12, "",	"x-2",	"",		 6,  7),
											new NeumePart( 5,  6, "!",	"x-1",	"o",	 7, 10),
											new NeumePart(10, 0, "",	"x",	"",		 6,  7))}, // Salicus2
			
			{ 28, new NeumeFormat(16, 25,	new NeumePart( 0, 18, "",	"x-3",	"",		 6,  7),
											new NeumePart( 5, 12, "!",	"x-2",	"o",	 7, 10),
											new NeumePart(10, 0, "",	"x",	"",		 6,  7))}, // Salicus3
			
			{ 29, new NeumeFormat(16, 13,	new NeumePart( 0,  6, "",	"x-1",	"",		 6,  7),
											new NeumePart( 5,  0, "",	"x",	"",		 6,  7),
											new NeumePart(10,  6, "",	"x-1",	"",		 6,  7))}, // torculus22
			
			{ 30, new NeumeFormat(16, 19,	new NeumePart( 0,  6, "",	"x-1",	"",		 6,  7),
											new NeumePart( 5,  0, "",	"x",	"",		 6,  7),
											new NeumePart(10, 12, "",	"x-2",	"",		 6,  7))}, // torculus23
			
			{ 31, new NeumeFormat(16, 25,	new NeumePart( 0,  6, "",	"x-1",	"",		 6,  7),
											new NeumePart( 5,  0, "",	"x",	"",		 6,  7),
											new NeumePart(10, 18, "",	"x-3",	"",		 6,  7))}, // torculus24
			
			{ 32, new NeumeFormat(16, 31,	new NeumePart( 0,  6, "",	"x-1",	"",		 6,  7),
											new NeumePart( 5,  0, "",	"x",	"",		 6,  7),
											new NeumePart(10, 24, "",	"x-4",	"",		 6,  7))}, // torculus25
			
			{ 33, new NeumeFormat(16, 19,	new NeumePart( 0, 12, "",	"x-2",	"",		 6,  7),
											new NeumePart( 5,  0, "",	"x",	"",		 6,  7),
											new NeumePart(10,  6, "",	"x-1",	"",		 6,  7))}, // torculus32
			
			{ 34, new NeumeFormat(16, 19,	new NeumePart( 0, 12, "",	"x-2",	"",		 6,  7),
											new NeumePart( 5,  0, "",	"x",	"",		 6,  7),
											new NeumePart(10, 12, "",	"x-2",	"",		 6,  7))}, // torculus33
			
			{ 35, new NeumeFormat(16, 25,	new NeumePart( 0, 12, "",	"x-2",	"",		 6,  7),
											new NeumePart( 5,  0, "",	"x",	"",		 6,  7),
											new NeumePart(10, 18, "",	"x-3",	"",		 6,  7))}, // torculus34
			
			{ 36, new NeumeFormat(16, 31,	new NeumePart( 0, 12, "",	"x-2",	"",		 6,  7),
											new NeumePart( 5,  0, "",	"x",	"",		 6,  7),
											new NeumePart(10, 24, "",	"x-4",	"",		 6,  7))}, // torculus35
			
			{ 37, new NeumeFormat(16, 25,	new NeumePart( 0, 18, "",	"x-3",	"",		 6,  7),
											new NeumePart( 5,  0, "",	"x",	"",		 6,  7),
											new NeumePart(10,  6, "",	"x-1",	"",		 6,  7))}, // torculus42
			
			{ 38, new NeumeFormat(16, 25,	new NeumePart( 0, 18, "",	"x-3",	"",		 6,  7),
											new NeumePart( 5,  0, "",	"x",	"",		 6,  7),
											new NeumePart(10, 12, "",	"x-2",	"",		 6,  7))}, // torculus43
			
			{ 39, new NeumeFormat(16, 25,	new NeumePart( 0, 18, "",	"x-3",	"",		 6,  7),
											new NeumePart( 5,  0, "",	"x",	"",		 6,  7),
											new NeumePart(10, 18, "",	"x-3",	"",		 6,  7))}, // torculus44
			
			{ 40, new NeumeFormat(16, 31,	new NeumePart( 0, 18, "",	"x-3",	"",		 6,  7),
											new NeumePart( 5,  0, "",	"x",	"",		 6,  7),
											new NeumePart(10, 24, "",	"x-4",	"",		 6,  7))}, // torculus45
			
			{ 41, new NeumeFormat(16, 31,	new NeumePart( 0, 24, "",	"x-4",	"",		 6,  7),
											new NeumePart( 5,  0, "",	"x",	"",		 6,  7),
											new NeumePart(10,  6, "",	"x-1",	"",		 6,  7))}, // torculus52
			
			{ 42, new NeumeFormat(16, 31,	new NeumePart( 0, 24, "",	"x-4",	"",		 6,  7),
											new NeumePart( 5,  0, "",	"x",	"",		 6,  7),
											new NeumePart(10, 12, "",	"x-2",	"",		 6,  7))}, // torculus53
			
			{ 43, new NeumeFormat(16, 31,	new NeumePart( 0, 24, "",	"x-4",	"",		 6,  7),
											new NeumePart( 5,  0, "",	"x",	"",		 6,  7),
											new NeumePart(10, 18, "",	"x-3",	"",		 6,  7))}, // torculus54
			
			{ 44, new NeumeFormat(16, 31,	new NeumePart( 0, 24, "",	"x-4",	"",		 6,  7),
											new NeumePart( 5,  0, "",	"x",	"",		 6,  7),
											new NeumePart(10, 24, "",	"x-4",	"",		 6,  7))}, // torculus55
			
			{ 45, new NeumeFormat( 6, 17,	new NeumePart( 0,  0, "",	"x",	"v",	 6,  6))}, // Virga
			//{ 46, new NeumeFormat( 3, 15,	new NeumePart( 0,  0, "",	"x",	"+",	 3,  6)}, // Guidon1 TODO check! No gabc symbol??
			{ 47, new NeumeFormat( 3, 13,	new NeumePart( 0,  6, "",	"x-1",	"+",	 3,  6))}, // Gidon2
			{ 48, new NeumeFormat( 6, 10,	new NeumePart( 0,  0, "",	"x",	"v",	 6,  6))}, // Virga2 TODO check! No gabc symbol??
			{ 50, new NeumeFormat( 6, 10,	new NeumePart( 0,  0, "",	"X",	"",		 5, 10))}, // Stopha TODO check!
			{ 51, new NeumeFormat( 6, 10,	new NeumePart( 0,  0, "",	"X",	"",		 5, 10))}, // Stropha1 TODO check!

			{ 52, new NeumeFormat(10, 15,	new NeumePart( 0,  0, "",	"X",	"",		 5, 10),
											new NeumePart( 5,  5, "",	"X-1",	"",		 5, 10))}, // Stropha2 TODO check!
			
			{ 53, new NeumeFormat(14, 21,	new NeumePart( 0,  0, "",	"X",	"",		 5, 10),
											new NeumePart( 4,  5, "",	"X-1",	"",		 5, 10),
											new NeumePart( 8, 11, "",	"X-2",	"",		 5, 10))}, // Stropha3 TODO check!
			
			{ 54, new NeumeFormat( 6, 10,	new NeumePart( 0,  0, "",	"X",	"~",	 5,  8))}, // StrophaLiq TODO check!
			{ 55, new NeumeFormat( 6, 10,	new NeumePart( 0,  0, "",	"X",	"~",	 5,  8))}, // StrophaLiq1 TODO check!

			{ 56, new NeumeFormat(10, 15,	new NeumePart( 0,  0, "",	"X",	"~",	 5,  8),
											new NeumePart( 5,  5, "",	"X-1",	"~",	 5,  8))}, // StrophaLiq2 TODO check!
			
			{ 57, new NeumeFormat(14, 21,	new NeumePart( 0,  0, "",	"X",	"~",	 5,  8),
											new NeumePart( 4,  5, "",	"X-1",	"~",	 5,  8),
											new NeumePart( 8, 11, "",	"X-2",	"~",	 5,  8))}, // StrophaLiq3 TODO check!
			
			{ 60, new NeumeFormat(16, 13,	new NeumePart( 0,  0, "",	"x",	"",		 6,  7),
											new NeumePart(10,  8, "",	"x-1",	"",		 6,  7),
											new NeumePart(10,  0, "",	"x",	"",		 6,  7))}, // Porrectus2
			
			//{ 61, new NeumeFormat(16, 14,	new NeumePart( 0,  0, "",	"x",	"",		 6,  7),
			//								new NeumePart(10,  8, "",	"x-1",	"+",	 6,  7))}, // Porrectus20 TODO no gabc symbol?

			//{ 62, new NeumeFormat(16, 13,	new NeumePart( 0,  0, "",	"x",	"",		 6,  7),
			//								new NeumePart(10,  8, "",	"x-1",	"",		 6,  7))}, // Porrectus2 TODO no gabc symbol?

			//{ 63, new NeumeFormat(16, 20,	new NeumePart( 0,  0, "",	"x",	"",		 6,  7),
			//								new NeumePart(10, 14, "",	"x-2",	"",		 6,  7))}, // Porrectus3 TODO no gabc symbol?

			{ 64, new NeumeFormat(16, 20,	new NeumePart( 0,  0, "",	"x",	"",		 6,  7),
											new NeumePart(10, 14, "",	"x-2",	"",		 6,  7),
											new NeumePart(10,  6, "",	"x-1",	"",		 6,  7))}, // Porrectus32
			
			{ 65, new NeumeFormat(16, 20,	new NeumePart( 0,  0, "",	"x",	"",		 6,  7),
											new NeumePart(10, 14, "",	"x-2",	"",		 6,  7),
											new NeumePart(10,  0, "",	"x",	"",		 6,  7))}, // Porrectus33
			
			{ 67, new NeumeFormat(16, 20,	new NeumePart( 0,  0, "",	"x",	"",		 6,  7),
											new NeumePart(10, 14, "",	"x-2",	"",		 6,  7),
											new NeumePart(10,  6, "",	"x-1",	"",		 6,  7))}, // Porrectus032 TODO no gabc symbol?
			
			//{ 68, new NeumeFormat(16, 13,	new NeumePart( 0,  0, "",	"x",	"",		 6,  7))}
			//								new NeumePart(10,  6, "",	"x-1",	"+",	 6,  7))}, // Porrectus2 TODO no gabc symbol?

			{ 69, new NeumeFormat(16, 13,	new NeumePart( 0,  0, "",	"x",	"",		 6,  7),
											new NeumePart(10,  8, "",	"x-1",	"",		 6,  7),
											new NeumePart(10,  0, "",	"x",	"",		 6,  7))}, // Porrectus2 TODO no gabc symbol?

			{ 70, new NeumeFormat( 6,  7,	new NeumePart( 0,  0, "",	"x",	">",	 6,  7))}, // puncliq1
			{ 71, new NeumeFormat( 6,  7,	new NeumePart( 0,  0, "",	"x",	"r",	 6,  7))}, // Punctum blanc
			{ 80, new NeumeFormat( 1,  9,	new NeumePart( 0,  3, "'",	"",		"",		 1,  6))}, // Ictus (special!)
			{ 81, new NeumeFormat( 6,  4,	new NeumePart( 0,  3, "_",	"",		"",		 6,  1))}, // Episeme (special00!)
			{ 82, new NeumeFormat(12,  4,	new NeumePart( 0,  3, "_",	"",		"",		12,  1))}, // Ligne (special!)

			{100, new NeumeFormat( 6, 13,	new NeumePart( 0,  6, "",	"x-1",	"",		 6,  7),
											new NeumePart( 0,  0, "",	"x",	"~",	 6,  3))}, // podLiq2
			
			{101, new NeumeFormat( 6, 18,	new NeumePart( 0, 12, "",	"x-2",	"",		 6,  7),
											new NeumePart( 0,  0, "",	"x",	"~",	 6,  3))}, // podLiq3
			
			{103, new NeumeFormat( 6, 24,	new NeumePart( 0, 18, "",	"x-3",	"",		 6,  7),
											new NeumePart( 0,  0, "",	"x",	"~",	 6,  3))}, // podLiq4
			
			{104, new NeumeFormat(11, 12,	new NeumePart( 0,  5, "-",	"x-1",	"",		 6,  6),
											new NeumePart( 5,  0, "",	"x",	"",		 6,  7))}, // PodDeb2
			
			{105, new NeumeFormat(11, 17,	new NeumePart( 0, 10, "-",	"x-2",	"",		 6,  6),
											new NeumePart( 5,  0, "",	"x",	"",		 6,  7))}, // PodDeb3
			
			{106, new NeumeFormat( 6, 31,	new NeumePart( 0, 24, "",	"x-4",	"",		 6,  7),
											new NeumePart( 0,  0, "",	"x",	"~",	 6,  3))}, // podLiq5
			
			{110, new NeumeFormat( 3,  5,	new NeumePart( 0,  0, "`",	"",		"",		 3,  5))}, // Virgule

			{112, new NeumeFormat( 6, 16,	new NeumePart( 0,  0, "",	"x",	"",		 6,  7),
											new NeumePart( 0,  5, "",	"x-1",	"~",	 6,  7))}, // ClivisLiq2
			
			{113, new NeumeFormat( 6, 19,	new NeumePart( 0,  0, "",	"x",	"",		 6,  7),
											new NeumePart( 0, 12, "",	"x-2",	"~",	 6,  7))}, // ClivisLiq3
			
			{114, new NeumeFormat( 6, 25,	new NeumePart( 0,  0, "",	"x",	"",		 6,  7),
											new NeumePart( 0, 17, "",	"x-3",	"~",	 6,  7))}, // ClivisLiq4
			
			{120, new NeumeFormat(12, 19,	new NeumePart( 0,  0, "",	"x",	"v",	 6,  7),
											new NeumePart( 7,  5, "!",	"X-1",	"~",	 5,  8))}, // Climacus2
			
			{121, new NeumeFormat(21, 26,	new NeumePart( 0,  0, "",	"x",	"v",	 6,  7),
											new NeumePart( 6,  5, "!",	"X-1",	"~",	 5,  8),
											new NeumePart(10,  11, "",	"X-2",	"~",	 5,  8),
											new NeumePart(14,  17, "",	"X-3",	"~",	 5,  8))}, // Climacus4
			
			{122, new NeumeFormat(17, 20,	new NeumePart( 0,  0, "",	"x",	"v",	 6,  7),
											new NeumePart( 6,  5, "!",	"X-1",	"~",	 5,  8),
											new NeumePart(10, 11, "",	"X-2",	"~",	 5,  8))}, // Climacus3
			
			{123, new NeumeFormat(25, 32,	new NeumePart( 0,  0, "",	"x",	"v",	 6,  7),
											new NeumePart( 6,  5, "!",	"X-1",	"~",	 5,  8),
											new NeumePart(10, 11, "",	"X-2",	"~",	 5,  8),
											new NeumePart(14, 17, "",	"X-3",	"~",	 5,  8),
											new NeumePart(18, 23, "",	"X-4",	"~",	 5,  8))}, // Climacus5
			
			{130, new NeumeFormat( 5,  8,	new NeumePart( 0,  0, "",	"x",	"y",	 5,  8))}, // becar

			// All glyphs below seem to be parts for building other complex neumes.
			// Probably they're not use directly in any GRG2 file
			//{131, new NeumeFormat( 5,  8,	new NeumePart( 0,  0, "",	"",		"",		 5,  8))}, // Diese TODO no gabc symbol?
			//{160, new NeumeFormat( 6,  7,	new NeumePart( 0,  0, "",	"",		"",		 6,  7))}, // 7 TODO no gabc symbol?
			//{161, new NeumeFormat( 7,  7,	new NeumePart( 0,  0, "",	"",		"",		 7,  7))}, // Eliz TODO no gabc symbol?
			//{201, new NeumeFormat( 6, 10,	new NeumePart( 0,  2, "",	"",		"",		 6,  8))}, // Debilis2 TODO not present in AllInOne???
			{202, new NeumeFormat( 6,  7,	new NeumePart( 0,  0, "",	"x",	"<",	 6,  7))}, // Liquescent1
			//{203, new NeumeFormat( 6, 12,	new NeumePart( 0,  5, "",	"",		"",		 6,  7))}, // Debilis TODO no gabc symbol?
			//{205, new NeumeFormat( 6, 12,	new NeumePart( 0,  5, "",	"",		"",		 6,  7))}, // Debilis TODO no gabc symbol?
			{211, new NeumeFormat( 1,  8,	new NeumePart( 0,  0, ",",	"",		"",		 1,  8))}, // Lien 1 TODO no gabc symbol?
			{212, new NeumeFormat( 1, 15,	new NeumePart( 0,  0, ";",	"N",	"",		 1, 15))}, // Lien2 TODO no gabc symbol?;
			{213, new NeumeFormat( 1, 22,	new NeumePart( 0,  0, ";",	"N",	"",		 1, 22))}, // Lien 3 TODO no gabc symbol?
			{214, new NeumeFormat( 1, 29,	new NeumePart( 0,  0, ";",	"N",	"",		 1, 29))}, // Lien 4 TODO no gabc symbol?
			{215, new NeumeFormat( 1, 36,	new NeumePart( 0,  0, ":",	"",		"",		 1, 36))}, // Lien 5 TODO no gabc symbol?
		};
	}
}

