using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace Mahi;

class Program
{
	const string ip = "0.0.0.0";
	const int port = 1010;

	static void Main()
	{
		new UnitTest().htmlTemplate();
		return;

		ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		Console.Title = "Mahi 1.0.0";

		var server = new HttpServer(IPAddress.Parse(ip), port, "cert.pfx", "987654321");
		//var server = new HttpServer(IPAddress.Parse(ip), port);
		try
		{
			server.Start();
		}
		catch (Exception ex)
		{
			Log("&c" + ex.ToString());
			return;
		}

		Log("[&2Info&r] &fStarting server ...");
		Log($"[&2Info&r] &fServer started and binded on &7http{(server.IsTlsSecure ? "s": "")}//{ip}:{port}/");

		while (true)
			using (var context = server.GetContext())
			{
				var request = context.Request;
				var response = context.Response;

				Log($"[&8Log&r] ", false);
				switch (request.Method)
				{
					case "GET":
						Log("&f", false);
						break;
					case "POST":
						Log("&a", false);
						break;
					case "PUT":
						Log("&6", false);
						break;
					case "DELETE":
						Log("&3", false);
						break;
					default:
						Log("&1? ", false);
						break;
				}
				Log(request.Method + " &r" + request.Url);

				
			}
	}

	static void Log(string text, bool newLine = true)
	{
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == '&' && text.Length >= i && Regex.Match(text[i + 1].ToString(), @"^[0-9A-Ra-r]+$").Success)
			{
				if (char.ToLower(text[i + 1]) == 'r')
					Console.ResetColor();
				else Console.ForegroundColor = (ConsoleColor)Convert.ToInt32(text[i + 1].ToString(), 16);
				i++;
				if (i + 1 >= text.Length)
					break;
				continue;
			}
			Console.Write(text[i]);
		}
		if (newLine) Console.WriteLine();
	}
}
