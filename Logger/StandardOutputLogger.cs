using System.Text;

namespace Mahi.Logger
{
	public class StandardOutputLogger : TextWriter
	{
		private readonly ServerLogger _logger;

		public StandardOutputLogger(ServerLogger logger)
		{
			_logger = logger;
		}

		public override void Write(string value)
		{
			_logger.LogStandardOutput(value);
		}

		public override void WriteLine(string value)
		{
			_logger.LogStandardOutput(value);
		}

		public override Encoding Encoding => Encoding.UTF8;
	}
}
