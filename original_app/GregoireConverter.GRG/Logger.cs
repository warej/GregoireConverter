using System;
using System.Runtime.Remoting.Messaging;

namespace GregoireConverter.GRG
{
	/// <summary>
	/// GregoireConverter logger class.
	/// </summary>
	public class Logger
	{
		public enum Level : int
		{
			ERROR = 0,
			WARNING,
			INFO,
			DEBUG,
			VERBOSE
		}

		private static object syncObject = new object();
		private static Logger _instance = null;

		/// <summary>
		/// Gets Logger's object.
		/// </summary>
		/// <value>Logger's instance</value>
		public static Logger Instance
		{
			get
			{
				lock (syncObject)
				{
					if (_instance == null)
					{
						_instance = new Logger();
					}
				}
				return _instance;
			}
		}

		/// <summary>
		/// Format for log
		/// </summary>
		private readonly string _format;

		/// <summary>
		/// Log level for this logger.
		/// </summary>
		public readonly Level _level;

		private Logger (Level level = Level.WARNING)
		{
			_format = "%utcdate|%p|{0}\n";
			_level = level;
		}

		/// <summary>
		/// Initialize the Logger with a non-default log level. Default level is: WARNING.
		/// </summary>
		/// <param name="level">Log level</param>
		public static void Initialize(Level level)
		{
			lock(syncObject)
			{
				_instance = new Logger(level);
			}

			return;
		}

		public void Log(string message, Level level)
		{
			if (level <= _level)
			{
				var fmt = _format
					.Replace("%utcdate", DateTime.UtcNow.ToString())
					.Replace("%p", level.ToString());
				if (level <= Level.WARNING)
					Console.Error.Write(fmt, message);
				else
					Console.Write(fmt, message);
			}
		}

		public static void LogError(string fmt, params object[] args)
		{
			Instance.Log(string.Format(fmt, args), Level.ERROR);
		}

		public static void LogError(Exception e)
		{
			if (Instance._level <= Level.INFO)
				Instance.Log(string.Format("Exception occured: {0}", e.Message), Level.ERROR);
			else
				Instance.Log(string.Format("Exception occured: {0}", e.ToString()), Level.ERROR);
		}
		
		public static void LogWarning(string fmt, params object[] args)
		{
			Instance.Log(string.Format(fmt, args), Level.WARNING);
		}

		public static void LogInfo(string fmt, params object[] args)
		{
			Instance.Log(string.Format(fmt, args), Level.INFO);
		}

		public static void LogDebug(string fmt, params object[] args)
		{
			Instance.Log(string.Format(fmt, args), Level.DEBUG);
		}

		public static void LogVerbose(string fmt, params object[] args)
		{
			Instance.Log(string.Format(fmt, args), Level.VERBOSE);
		}
	}
}

