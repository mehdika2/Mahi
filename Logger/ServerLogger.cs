using System.Text;
using System.Text.RegularExpressions;

namespace Mahi.Logger
{
	public class ServerLogger
	{
		private readonly Stream consoleOutputStream;
		private readonly StreamWriter outputWriter;
		private readonly StreamWriter fileWriter;

		public ServerLogger()
		{
			consoleOutputStream = Console.OpenStandardOutput();

			var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

			outputWriter = new StreamWriter(consoleOutputStream, utf8NoBom)
			{
				AutoFlush = true
			};

			string logsPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
			if (!Directory.Exists(logsPath)) Directory.CreateDirectory(logsPath);
			fileWriter = new StreamWriter(Path.Combine(logsPath, DateTime.Now.ToString("yyyy-MM-dd") + ".log"), true);
			AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
		}

		// Write directly to the console output stream
		internal void LogStandardOutput(string message)
		{
			WriteLog($"[STDO] {message}");
		}

		public void Log(string message)
		{
			WriteLog($"[Logger] {message}");
		}

		private void WriteLog(string message)
		{
			try
			{
				fileWriter.Write($"[{DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.ff")}] ");

				for (int i = 0; i < message.Length; i++)
				{
					if (message[i] == '&' && message.Length >= i && Regex.Match(message[i + 1].ToString(), @"^[0-9A-Ra-r]+$").Success)
					{
						if (char.ToLower(message[i + 1]) == 'r')
							Console.ResetColor();
						else Console.ForegroundColor = (ConsoleColor)Convert.ToInt32(message[i + 1].ToString(), 16);
						i++;
						if (i + 1 >= message.Length)
							break;
						continue;
					}
					fileWriter.Write(message[i]);
					outputWriter.Write(message[i]);
				}

				fileWriter.WriteLine();
				outputWriter.WriteLine();

				fileWriter.Flush();
			}
			catch (Exception ex)
			{
				outputWriter.WriteLine($"Failed to write to log: {ex.Message}");
			}
		}

		private void OnProcessExit(object sender, EventArgs e)
		{
			Dispose();
		}

		public void Dispose()
		{
			outputWriter?.Dispose();
			fileWriter?.Close();
			consoleOutputStream?.Close();
		}
	}
}
