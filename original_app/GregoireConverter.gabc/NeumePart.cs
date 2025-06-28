using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Diagnostics.CodeAnalysis;

namespace GregoireConverter.gabc
{
	public class NeumePart
	{
		public int StartX;
		public int StartY;
		public string Before;
		public string Height;
		public string After;
		public int SizeX;
		public int SizeY;

		public NeumePart ()
		{
		}

		public NeumePart (int startX, int startY, string before, string height, string after, int sizeX, int sizeY)
		{
			StartX = startX;
			StartY = startY;
			Before = before;
			Height = height;
			After = after;
			SizeX = sizeX;
			SizeY = sizeY;
		}

		public NeumePart FindClosestPart(GRG2NeumeForGabc thisNeume, GRG2NeumeForGabc mainNeume)
		{
			NeumePart candidateAbove = null;
			NeumePart candidateBelow = null;

			foreach (var part in mainNeume._format.Parts)
			{
				if (Above(thisNeume, this, mainNeume, part))
				{
					if (candidateAbove == null)
						candidateAbove = part;
					else if (((mainNeume.PositionY + part.StartY) - (thisNeume.PositionY + this.StartY + this.SizeY))
						< ((mainNeume.PositionY + candidateAbove.StartY) - (thisNeume.PositionY + this.StartY + this.SizeY)))
						candidateAbove = part;
				}
				if (Below(thisNeume, this, mainNeume, part))
				{
					if (candidateBelow == null)
						candidateBelow = part;
					else if (((thisNeume.PositionY + this.StartY + this.SizeY) - (mainNeume.PositionY + part.StartY))
						< ((thisNeume.PositionY + this.StartY + this.SizeY) - (mainNeume.PositionY + candidateBelow.StartY)))
						candidateBelow = part;
				}
			}

			// Challenge!
			if (candidateAbove != null && candidateBelow != null)
			{
				if (((mainNeume.PositionY + candidateAbove.StartY) - (thisNeume.PositionY + this.StartY + this.SizeY))
				    > ((thisNeume.PositionY + this.StartY + this.SizeY) - (mainNeume.PositionY + candidateBelow.StartY)))
					return candidateAbove;
				else
					return candidateBelow;
			}
			else if (candidateAbove != null)
				return candidateAbove;
			else if (candidateBelow != null)
				return candidateBelow;
			else
				return mainNeume._format.Parts.LastOrDefault();
		}

		public static bool Above(GRG2NeumeForGabc aboveNeume, NeumePart abovePart,
			GRG2NeumeForGabc secondNeume, NeumePart secondPart)
		{
			// Check horizontally
			var subsetX = RangeSubset(
				              aboveNeume.PositionX + abovePart.StartX,
				              aboveNeume.PositionX + abovePart.StartX + abovePart.SizeX,
				              secondNeume.PositionX + secondPart.StartX,
				              secondNeume.PositionX + secondPart.StartX + secondPart.SizeX);

			// If there is only accidental hover don't take it into account
			if (subsetX < Min(2, secondPart.SizeX / 2)) // Error margin
				return false;

			// Check vertically
			if ((aboveNeume.PositionY + abovePart.StartY < secondNeume.PositionY + secondPart.StartY)
				&& (RangeSubset(
						aboveNeume.PositionY + abovePart.StartY,
						aboveNeume.PositionY + abovePart.StartY + abovePart.SizeY,
						secondNeume.PositionY + secondPart.StartY,
						secondNeume.PositionY + secondPart.StartY + secondPart.SizeY)
					< 2)) // Error margin
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool Below(GRG2NeumeForGabc belowNeume, NeumePart belowPart,
			GRG2NeumeForGabc secondNeume, NeumePart secondPart)
		{
			// Check horizontally
			var subsetX = RangeSubset(
				belowNeume.PositionX + belowPart.StartX,
				belowNeume.PositionX + belowPart.StartX + belowPart.SizeX,
				secondNeume.PositionX + secondPart.StartX,
				secondNeume.PositionX + secondPart.StartX + secondPart.SizeX);

			// If there is only accidental hover don't take it into account
			if (subsetX < Min(2, secondPart.SizeX / 2)) // Error margin
				return false;

			// Check vertically
			if ((belowNeume.PositionY + belowPart.StartY > secondNeume.PositionY + secondPart.StartY)
				&& (RangeSubset(
						belowNeume.PositionY + belowPart.StartY,
						belowNeume.PositionY + belowPart.StartY + belowPart.SizeY,
						secondNeume.PositionY + secondPart.StartY,
						secondNeume.PositionY + secondPart.StartY + secondPart.SizeY)
					< 2)) // Error margin
			{
				return true;
			}
			else
			{
				return false;
			}
		}


		public static int RangeSubset(int al, int ar, int bl, int br)
		{
			// var leftMost = (al <= bl)? al : bl;
			var rightOne = (al <= bl)? ar : br;
			var secondLeft = (al <= bl)? bl : al;
			var secondRight = (al <= bl)? br : ar;

			if (rightOne < secondLeft)
			{	// ---[leftMost---rightOne]---[secondLeft---secondRight]---
				return rightOne - secondLeft;
			}

			if (rightOne > secondRight)
			{	// ---[leftMost---[secondLeft---secondRight]---rightOne]---
				return secondRight - secondLeft;
			}

			// ---[leftMost---rightOne]---
			// ---------[secondLeft---secondRight]---
			return rightOne - secondLeft;
		}

		public static int Min(int a, int b) { return (a < b) ? a : b; }

		public static int Max(int a, int b) { return (a > b) ? a : b; }
	}
}

