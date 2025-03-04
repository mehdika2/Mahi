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
			string[] bindInfo = AppConfig.Instance.BindHost.Split(':');
			//var server = new HttpServer(IPAddress.Parse(bindInfo[0]), short.Parse(bindInfo[1]), "cert.pfx", Resources.CertificationPassword);
			var server = new HttpServer(IPAddress.Parse(bindInfo[0]), short.Parse(bindInfo[1]));
			server.BaseDirectory = AppConfig.Instance.BaseDirectory;

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
			logger.Log($"[&2Info&r] &fServer started and binded on &7http{(server.IsTlsSecure ? "s" : "")}://{bindInfo[0]}:{bindInfo[1]}/");

			RequestHandler.Process(server);

			logger.Dispose();
		}

		internal static void Log(string message, bool newLine = true)
			=> logger.Log(message, newLine);
	}
}