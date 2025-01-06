using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

using Fardin;
using NLua;
using Mahi.HtmLua;
using System.Runtime.Versioning;
using Mahi.Properties;
using static Mahi.Logger;
using Mahi.Core;

using CookieCollection = Fardin.CookieCollection;
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
			// ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			Console.Title = "Mahi1.0.0";

			// installing default modules
			InstallModules();
			
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

		static void InstallModules()
		{
			string moduesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "modules");
			if (!Directory.Exists(moduesDirectory))
				Directory.CreateDirectory(moduesDirectory);

			string jsonModulePath = Path.Combine(moduesDirectory, "json.lua");
			if (!File.Exists(jsonModulePath))
				File.WriteAllText(jsonModulePath, Encoding.UTF8.GetString(Resources.dkjson));

			string mssqlModulePath = Path.Combine(moduesDirectory, "mssql.lua");
			if (!File.Exists(mssqlModulePath))
				File.WriteAllText(mssqlModulePath, Encoding.UTF8.GetString(Resources.mssql));

			string hashModulePath = Path.Combine(moduesDirectory, "hash.lua");
			if (!File.Exists(hashModulePath))
				File.WriteAllText(hashModulePath, Encoding.UTF8.GetString(Resources.hash));
		}

		internal static void Log(string message, bool newLine = true)
			=> logger.Log(message, newLine);
	}
}