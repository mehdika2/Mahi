﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Fardin;
using NLua;
using Mahi.HtmLua;
using Mahi.Properties;
using NLua.Exceptions;

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

			string log = "[&8Log&r] ";
			switch (request.Method)
			{
				case "GET":
					log += "&f";
					break;
				case "POST":
					log += "&a";
					break;
				case "PUT":
					log += "&6";
					break;
				case "DELETE":
					log += "&3";
					break;
				default:
					log += "&1? ";
					break;
			}
			Program.Log(log += request.Method + " &r" + request.Uri.AbsolutePath);

			// Routing in config ...
			if (request.Uri.AbsolutePath == "/")
			{
				UriBuilder uriBuilder = new UriBuilder(request.Uri);
				uriBuilder.Path = "/index.htmlua";
				request.Items["R_URI"] = uriBuilder.Uri;
			}

			string filename = Path.GetFullPath("wwwapp") + '\\' + request.Uri.AbsolutePath.Trim('/');

			if (File.Exists(filename))
				if (filename.EndsWith(".htmlua"))
				{
					string script;
					try
					{
						HtmLuaParser htmluaParser = new HtmLuaParser();
						script = htmluaParser.ToLua(File.ReadAllText(filename));

						LuaInvoker.Run(script, stream, request, response);
					}
					catch (LuaScriptException ex)
					{
						response.StatusCode = 500;
						response.StatusText = "Internal Server Error";
						HandleException(ex.InnerException ?? ex, stream);
					}
					catch (Exception ex)
					{
						response.StatusCode = 500;
						response.StatusText = "Internal Server Error";
						HandleException(ex, stream);
					}
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
				response.StatusText = "Not Found";
				stream.Write(Encoding.UTF8.GetBytes(Resources.Http404.Replace("{Url}", request.Uri.AbsolutePath).Replace("{Description}", "404 Page not found")));
			}
		}

		static void HandleException(Exception ex)
		{
			if (!Directory.Exists("Errors"))
				Directory.CreateDirectory("Errors");

			File.WriteAllText(Path.Combine("Errors", DateTime.Now.ToString("yyyyMMdd_hhmmss_fffffff") + ".txt"), ex.ToString());

			Program.Log("&r[&4Error&r] " + ex.Message);
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
