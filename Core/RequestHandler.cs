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
using NLua.Exceptions;
using Mahi.Settings;
using System.IO;
using System.Runtime.InteropServices;

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

							HandleContext(request, response);

							if (response.StatusCode != 200)
							{
								AppConfig.Instance.ErrorPages.TryGetValue(response.StatusCode.ToString(), out string page);

								if (response.StatusCode == 404 && !File.Exists(Path.GetFullPath("wwwapp") + '\\' + page))
								{
									response.StatusText = "Not Found";
									response.ResponseStream.Write(Encoding.UTF8.GetBytes(Resources.Http404.Replace("{Url}", request.Uri.AbsolutePath).Replace("{Description}"
										, LastError is PageNotFoundException ? LastError.Message : "404 Page not found.")));
									continue;
								}
								else if(response.StatusCode == 500 && !File.Exists(Path.GetFullPath("wwwapp") + '\\' + page))
								{
									response.StatusText = "Internal Error";
									response.ResponseStream.Write(Encoding.UTF8.GetBytes(Resources.Http500.Replace("{Url}", request.Uri.AbsolutePath).Replace("{Description}"
										, LastError is PageNotFoundException ? LastError.Message : "404 Page not found.")));
									continue;
								}

								int redirectCode = response.StatusCode - 300;
								if (redirectCode >= 0 && redirectCode < 100)
								{
									// redirected before
									continue;
								}

								if (AppConfig.Instance.RedirectErrorPage)
								{
									if (!AppConfig.Instance.ExtentionRequired && AppConfig.Instance.NotExtentionInUrl && page != null && page.EndsWith(".htmlua"))
										page = page.Remove(page.Length - 7, 7);

									response.StatusCode = 302;
									response.Headers.Add("Location", '/' + page.Trim('/'));
									continue;
								}

								CallLuaInvoker(Path.Combine(Path.GetFullPath(AppConfig.Instance.BaseDirectory), page), true, request, response, out _);
							}
						}
						catch (Exception ex)
						{
							HandleException(ex, context.Response);
						}
				}
				catch (Exception ex)
				{
					HandleException(ex);
				}
		}

		static void HandleContext(HttpRequest request, HttpResponse response)
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

			// Detect irectory paths
			var config = AppConfig.Instance;
			string wwwappPath = Path.GetFullPath(config.BaseDirectory);
			string controllersPath = Path.Combine(wwwappPath, ".controllers");
			string modulesPath = Path.Combine(wwwappPath, ".modules");

			// Default pages
			bool defaultPageFound = false;
			if (request.Uri.AbsolutePath.Trim('/') == "")
			{
				foreach (var defaultPage in AppConfig.Instance.DefaultPages)
					if (File.Exists(Path.Combine(wwwappPath, defaultPage)))
					{
						UriBuilder uriBuilder = new UriBuilder(request.Uri);
						uriBuilder.Path = '/' + defaultPage;
						request.Items["R_URI"] = uriBuilder.Uri;
						defaultPageFound = true;
					}
				if (!defaultPageFound)
				{
					response.StatusCode = 404;
					LastError = new PageNotFoundException($"Default page file not exists!");
					return;
				}
			}

			// Http modules first
			var httpModules = AppConfig.Instance.HttpModules;
			foreach (var httpModule in httpModules)
			{
				string modulePath = Path.Combine(modulesPath, httpModule.Value.Trim('~').Replace('/', '\\').Trim('\\'));
				if (!CallLuaInvoker(modulePath, false, request, response, out object result) ||
					(result != null && (result as object[]).Length > 0 && (bool)(result as object[])[0]))
					return;
			}

			// Routing by config
			string filename;
			if (RouteUri.TryFindRoute(request.Uri, out filename))
			{
				if (filename.StartsWith(">")) // redirect
				{
					response.StatusCode = 302;
					response.Headers.Add("Location", (filename.StartsWith(">/") ? "" : "/") + filename.Trim('>'));
					return;
				}
				else if (filename.StartsWith("="))
				{
					filename = Path.Combine(controllersPath, filename.Trim('=').Replace('/', '\\').Trim('\\'));
					CallLuaInvoker(filename, false, request, response, out _);
					return;
				}
				else // route to file
					filename = Path.Combine(wwwappPath, filename.Trim('/').Replace('/', '\\'));
			}
			else
			{
				if (!File.Exists(filename))
				{
					if (AppConfig.Instance.ExtentionRequired)
					{
						response.StatusCode = 404;
						LastError = new PageNotFoundException("url \"" + request.Uri.AbsolutePath + "\" not found!");
						return;
					}
					filename = Path.Combine(wwwappPath, request.Uri.AbsolutePath.Trim('/').Replace('/', '\\'), ".htmlua");
					if (!File.Exists(filename))
					{
						response.StatusCode = 404;
						LastError = new PageNotFoundException("url \"" + request.Uri.AbsolutePath + "\" not found!");
						return;
					}
				}
				else if (!filename.EndsWith(".htmlua"))
				{
					// Returning file
					response.Headers.Add("content-type", MimeTypeHelper.GetMimeType(filename));
					response.ResponseStream.Write(File.ReadAllBytes(filename));
					return;
				}
				else if ((AppConfig.Instance.ExtentionRequired && !request.Uri.AbsolutePath.EndsWith(".htmlua") || (!File.Exists(filename) && AppConfig.Instance.ExtentionRequired))
					|| (!defaultPageFound && !AppConfig.Instance.ExtentionRequired && AppConfig.Instance.NotExtentionInUrl && request.Uri.AbsolutePath.EndsWith(".htmlua")))
				{
					response.StatusCode = 404;
						LastError = new PageNotFoundException("url \"" + request.Uri.AbsolutePath + "\" not found!");
					return;
				}
			}

			// Run htmlua scripts
			CallLuaInvoker(filename, true, request, response, out _);
		}

		static bool CallLuaInvoker(string filename, bool htmluaParse, HttpRequest request, HttpResponse response, out object result)
		{
			try
			{
				string script;
				if (htmluaParse)
				{
					HtmLuaParser htmluaParser = new HtmLuaParser();
					script = htmluaParser.ToLua(File.ReadAllText(filename));
				}
				else script = File.ReadAllText(filename);

				result = LuaInvoker.Run(script, request, response);
				return true;
			}
			catch (LuaScriptException ex)
			{
				HandleException(ex.InnerException ?? ex, response);
			}
			catch (Exception ex)
			{
				HandleException(ex, response);
			}
			result = null;
			return false;
		}

		public static Exception LastError;

		static void HandleException(Exception ex)
		{
			string errorPath = Path.Combine(Directory.GetCurrentDirectory(), "Errors");
			if (!Directory.Exists(errorPath)) Directory.CreateDirectory(errorPath);

			File.WriteAllText(Path.Combine(errorPath, DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss_fffffff") + ".txt"), ex.ToString());

			Program.Log("&r[&4Error&r] " + ex.Message);

			LastError = ex;
		}

		static void HandleException(Exception ex, HttpResponse response)
		{
			HandleException(ex);

			string page;
			if (AppConfig.Instance.ErrorPages.TryGetValue("500", out page))
			{
				response.StatusCode = 500;

				if (AppConfig.Instance.RedirectErrorPage)
				{
					if (!AppConfig.Instance.ExtentionRequired && AppConfig.Instance.NotExtentionInUrl && page.EndsWith(".htmlua"))
						page = page.Remove(page.Length - 7, 7);

					response.Headers.Add("Location", page);
				}
				return;
			}

			response.ResponseStream.Write(Encoding.UTF8.GetBytes(Resources.Http500.Replace("{Error}", "Internal server error").Replace("{Type}", ex.GetType().Name)
				.Replace("{Description}", WebUtility.HtmlEncode(ex.Message)).Replace("{Exception}", WebUtility.HtmlEncode(ex.ToString()))
				.Replace("{DotnetVersion}", "dotnet " + Environment.Version.ToString()).Replace("{MahiVersion}", "Mahi " + Resources.Version)));
		}
	}
}
