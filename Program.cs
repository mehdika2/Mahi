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
using static Mahi.Core.Logger;
using Mahi.Core;

namespace Mahi
{
	internal class Program
	{
		const string ip = "127.0.0.1";
		const int port = 1010;
		static readonly Logger logger = new Logger();

		static void Main()
		{
#if UNITTEST
			UnitTest.Run();
			return;
#endif

			//ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			Console.Title = "Mahi1.0.0";

			//var server = new HttpServer(IPAddress.Parse(ip), port, "cert.pfx", Resources.CertificationPassword);
			var server = new HttpServer(IPAddress.Parse(ip), port);

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
			logger.Log($"[&2Info&r] &fServer started and binded on &7http{(server.IsTlsSecure ? "s" : "")}//{ip}:{port}/");

			RequestHandler.Process(server);

			logger.Dispose();
		}

		internal static void Log(string message, bool newLine = true)
			=> logger.Log(message, newLine);
	}
}