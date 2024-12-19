using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Fardin;
using NLua;
using Mahi.HtmLua;
using Mahi.Properties;
using static Mahi.Core.Logger;

namespace Mahi.Core
{
	internal static class RequestHandler
	{
		public static void Process(HttpServer server)
		{
			while (true)
				try
				{
					using (var context = server.GetContext())
						try
						{
							var request = context.Request;
							var response = context.Response;
							var stream = response.ResponseStream;

							HandleContext(request, response, stream);
						}
						catch (Exception ex)
						{
							HandleException(ex, context.Response.ResponseStream);
						}
				}
				catch (Exception ex)
				{
					HandleException(ex);
				}
		}

		static void HandleContext(HttpRequest request, HttpResponse response, MemoryStream stream)
		{
			response.Headers.Add("x-powered-by", "Mahi-" + Resources.Version);

			Program.Log($"[&8Log&r] ", false);
			switch (request.Method)
			{
				case "GET":
					Program.Log("&f", false);
					break;
				case "POST":
					Program.Log("&a", false);
					break;
				case "PUT":
					Program.Log("&6", false);
					break;
				case "DELETE":
					Program.Log("&3", false);
					break;
				default:
					Program.Log("&1? ", false);
					break;
			}
			Program.Log(request.Method + " &r" + request.Url);

			// Routing
			string filename = request.Url.TrimStart('/');
			if (File.Exists(filename))
				if (filename.EndsWith(".htmlua"))
					try
					{
						HtmLuaParser htmluaParser = new HtmLuaParser();
						var script = htmluaParser.ToLua(File.ReadAllText(filename));

						LuaInvoker.Run(script, stream, request, response);
					}
					catch (Exception ex)
					{
						HandleException(ex, stream);
					}
				else
				{
					// Returning file
					response.Headers.Add("content-type", MimeTypeHelper.GetMimeType(filename));
					stream.Write(File.ReadAllBytes(filename));
				}
			else
			{
				response.StatusCode = 404;
				response.StatusText = "Not found";
				stream.Write(Encoding.UTF8.GetBytes(Resources.Http404.Replace("{Url}", request.Url).Replace("{Description}", "404 Page not found")));
			}
		}

		static void HandleException(Exception ex)
		{
			if (!Directory.Exists("Errors"))
				Directory.CreateDirectory("Errors");

			File.WriteAllText(Path.Combine("Errors", DateTime.Now.ToString("yyyyMMdd_hhmmss_fffffff") + ".txt"), ex.ToString());
		}

		static void HandleException(Exception ex, MemoryStream responseStream)
		{
			responseStream.Write(Encoding.UTF8.GetBytes(Resources.Http500.Replace("{Error}", "Internal server error").Replace("{Type}", ex.GetType().Name)
				.Replace("{Description}", WebUtility.HtmlEncode(ex.Message)).Replace("{Exception}", WebUtility.HtmlEncode(ex.ToString()))
				.Replace("{DotnetVersion}", "dotnet " + Environment.Version.ToString()).Replace("{MahiVersion}", "Mahi " + Resources.Version)));
			HandleException(ex);
		}
	}
}
