using System;
using System.Linq;
using GregoireConverter.GRG;
using GregoireConverter.gabc;
using System.Net.WebSockets;
using System.IO;
using System.Text.RegularExpressions;

namespace GregoireConverter
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			if (args.Length <= 0)
			{
				Console.WriteLine("Goodbye! Bring any prameters with you next time, please ;)");
				System.Environment.Exit(0);
			}

			if (args.Contains("-vv"))
				Logger.Initialize(Logger.Level.VERBOSE);
			else if (args.Contains("-v"))
				Logger.Initialize(Logger.Level.DEBUG);
			else if (args.Contains("-s"))
				Logger.Initialize(Logger.Level.WARNING);
			else
				Logger.Initialize(Logger.Level.INFO);

			foreach (var indicator in new []{"-h", "/h", "--h", "-help", "--help"})
			{
				if (args.Contains(indicator))
				{
					Console.WriteLine("Usage is: GregoireConverter.exe [options] [GRG files list]\n\n" +
						"Available options:\n" +
						"\t-h\tShow program help\n" +
						"\t-s\tSilent mode (show only errors and warnings)\n" +
						"\t-v\tEnable debug logs\n" +
						"\t-vv\tShow every possible information (verbose)\n");
					System.Environment.Exit(0);
				}
			}

			Logger.LogInfo("Welcome to GregoireConverter by Artur Warejko!");

			int res = 0;
			foreach (var arg in args.Where((a) => !a.StartsWith("-")))
			{
				var file = new FileInfo(arg);

				if (!file.Exists)
				{
					Logger.LogError("File '{0}' not found!", arg);
					continue;
				}
				
				var match = Regex.Match(file.Name, @"(.*)\.[a-zA-Z0-9]{2,5}");// Typically *.GRG
				if (!match.Success)
				{
					Logger.LogError("Wrong file name format!");
				}
				var name = match.Groups[1].Value.ToString();

				if (arg.Contains(" "))
					Logger.LogWarning("File name or path contains one or more spaces!\n" +
						"\tPlease be cautious as it might cause Gragorio compilation failures.");

				var chant = GRG2Factory.FromFile(arg);

				res = new GabcExporter().Save(chant, Path.Combine(file.DirectoryName, name), name, true);
				Logger.LogInfo("Export exited with code {0}", res);
			}

			System.Environment.Exit(res);
		}
	}
}
