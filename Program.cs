using System.Net;
using System.Text;

using Fardin;
using Mahi.Properties;
using Mahi.Core;

using Mahi.Settings;

namespace Mahi
{
    internal class Program
	{
		const string ip = "127.0.0.1";
		const int port = 1010;
		static readonly Logger logger = new Logger();

		static void Main()
		{
			Console.Title = "Mahi 1.0";

			// true means that created a template successfully
			if (Templates.Template.CreateFromArguments())
				return;
			
			// load settings
			AppConfig.LoadConfigs();

			// watch appconfig.yaml
			AppConfig.StartConfigWatcher();

			// start server
			var server = new HttpServer(IPAddress.Parse(ip), port, "cert.pfx", Resources.CertificationPassword);
			//var server = new HttpServer(IPAddress.Parse(ip), port);
			server.BaseDirectory = "wwwapp";

			try
			{
				server.Start();
			}
			catch (Exception ex)
			{
				logger.Log("&c" + ex.ToString());
				return;
			}

			logger.Log("[&2Info&r] &fStarting server ...");
			logger.Log($"[&2Info&r] &fServer started and binded on &7http{(server.IsTlsSecure ? "s" : "")}://{ip}:{port}/");

			RequestHandler.Process(server);

			logger.Dispose();
		}

		internal static void Log(string message, bool newLine = true)
			=> logger.Log(message, newLine);
	}
}