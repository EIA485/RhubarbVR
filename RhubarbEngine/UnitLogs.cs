﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace RhubarbEngine
{
	public class UnitLogs
	{
		private Engine engine;

		public string logFile = DateTime.Now.ToString().Replace("/", "-").Replace(":", "_") + ".txt";

		public string logDir = AppDomain.CurrentDomain.BaseDirectory + @"Logs";

		public FileStream objFilestream;

		public StreamWriter objStreamWriter;

		public UnitLogs(Engine _engine)
		{
			engine = _engine;
			if (!Directory.Exists(logDir))
			{
				Directory.CreateDirectory(logDir);
			}
			objFilestream = new FileStream(Path.Combine(logDir, logFile), FileMode.OpenOrCreate, FileAccess.ReadWrite);
			objStreamWriter = new StreamWriter((Stream)objFilestream);
		}

		public void Log(string _log, bool _alwaysLog = false)
		{
			Console.WriteLine(String.Format("{0}: {1}", DateTime.Now, _log));
			if (_alwaysLog || engine.verbose)
			{
				writeLog(String.Format("{0}: {1}", DateTime.Now, _log));
			}
		}
		public bool writeLog(string strMessage)
		{
			try
			{
				objStreamWriter.WriteLine(strMessage);
				objStreamWriter.FlushAsync();
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		public void cleanUP()
		{
			try
			{
				objStreamWriter.Close();
				objFilestream.Close();
			}
			catch
			{
			}
		}
	}
}
