using System.Net;

using Fardin;
using Mahi.Core;
using Mahi.Logger;
using Mahi.Settings;

namespace Mahi
{
	internal class Program
	{
		static readonly ServerLogger logger = new ServerLogger();
		static readonly StandardOutputLogger stdLogger = new StandardOutputLogger(logger);

		public static ServerLogger Logger => logger;

		static void Main()
		{
			Console.Title = "Mahi 1.0";

			// set standard output logger
			Console.SetOut(stdLogger);

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

			RequestHandler.Process(server, logger);

			logger.Dispose();
		}
	}
}