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
								var config = AppConfig.Instance;
								config.ErrorPages.TryGetValue(response.StatusCode.ToString(), out string page);

								if (response.StatusCode == 404 && !File.Exists(Path.GetFullPath("wwwapp") + '\\' + page))
								{
									response.StatusText = "Not Found";
									response.ResponseStream.Write(Encoding.UTF8.GetBytes(Resources.Http404.Replace("{Url}", request.Uri.AbsolutePath).Replace("{Description}"
										, LastError is PageNotFoundException ? LastError.Message : "404 Page not found.")));
									continue;
								}
								else if (response.StatusCode == 500 && !File.Exists(Path.GetFullPath("wwwapp") + '\\' + page))
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

								if (config.RedirectErrorPage)
								{
									if (!config.ExtentionRequired && config.NotExtentionInUrl && page != null && page.EndsWith(".htmlua"))
										page = page.Remove(page.Length - 7, 7);

									response.StatusCode = 302;
									response.Headers.Add("Location", '/' + page.Trim('/'));
									continue;
								}

								CallLuaInvoker(Path.Combine(Path.GetFullPath(config.BaseDirectory), page), true, request, response, out _);
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
			string librariesPath = Path.Combine(wwwappPath, ".libraries");

			// Default pages
			bool defaultPageFound = false;
			if (request.Uri.AbsolutePath.Trim('/') == "")
			{
				foreach (var defaultPage in config.DefaultPages)
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
			var httpModules = config.HttpModules;
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
					if(Directory.Exists(filename))
					{
						string compareName = filename.ToLower();
						if (!config.DirectoryBrowsing || compareName.StartsWith(modulesPath.ToLower().TrimEnd('\\')) ||
							compareName.StartsWith(librariesPath.ToLower().TrimEnd('\\')) ||
							compareName.StartsWith(controllersPath.ToLower().TrimEnd('\\')))
						{
							response.StatusCode = 404;
							return;
						}

						string rows = CreateDirectoryBrowsintTable(filename, request.Uri.AbsolutePath);
						response.ResponseStream.Write(Encoding.UTF8.GetBytes(Resources.DirectoryBrowsing.Replace("{Rows}", rows)
							.Replace("{Directory}", request.Uri.AbsolutePath).Replace("{ParentDirectory}"
							, Path.GetDirectoryName(request.Uri.AbsolutePath).Replace('\\', '/'))));

						return;
					}
					else if (config.ExtentionRequired)
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
					string compareName = filename.ToLower();
					if (compareName.StartsWith(modulesPath.ToLower().TrimEnd('\\')) ||
						compareName.StartsWith(librariesPath.ToLower().TrimEnd('\\')) ||
						compareName.StartsWith(controllersPath.ToLower().TrimEnd('\\')))
					{
						response.StatusCode = 404;
						return;
					}
					// Returning file
					response.Headers.Add("content-type", MimeTypeHelper.GetMimeType(filename));
					response.ResponseStream.Write(File.ReadAllBytes(filename));
					return;
				}
				else if ((config.ExtentionRequired && !request.Uri.AbsolutePath.EndsWith(".htmlua") || (!File.Exists(filename) && config.ExtentionRequired))
					|| (!defaultPageFound && !config.ExtentionRequired && config.NotExtentionInUrl && request.Uri.AbsolutePath.EndsWith(".htmlua")))
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

			var config = AppConfig.Instance;

			string page;
			if (config.ErrorPages.TryGetValue("500", out page))
			{
				response.StatusCode = 500;

				if (config.RedirectErrorPage)
				{
					if (!config.ExtentionRequired && config.NotExtentionInUrl && page.EndsWith(".htmlua"))
						page = page.Remove(page.Length - 7, 7);

					response.Headers.Add("Location", page);
				}
				return;
			}

			response.ResponseStream.Write(Encoding.UTF8.GetBytes(Resources.Http500.Replace("{Error}", "Internal server error").Replace("{Type}", ex.GetType().Name)
				.Replace("{Description}", WebUtility.HtmlEncode(ex.Message)).Replace("{Exception}", WebUtility.HtmlEncode(ex.ToString()))
				.Replace("{DotnetVersion}", "dotnet " + Environment.Version.ToString()).Replace("{MahiVersion}", "Mahi " + Resources.Version)));
		}

		private static string CreateDirectoryBrowsintTable(string filename,string path)
		{
			StringBuilder sb = new StringBuilder();

			string[] directories = Directory.GetDirectories(filename);
			foreach (var directory in directories)
			{
				DirectoryInfo info = new DirectoryInfo(directory);
				string name = Path.GetFileName(directory);
				sb.AppendLine(@$"<tr>
    <td><img src=""/icons/folder.gif"" alt=""[DIR]""></td>
    <td><a href=""{(path == "/" ? "" : path + "/")}{name}"">{name}/</a></td>
    <td>{info.LastWriteTime.ToString("yyyy/MM/dd hh:mm")}</td>
    <td>-</td>
</tr>");
			}

			string[] files = Directory.GetFiles(filename);
			foreach(var  file in files)
			{
				FileInfo info = new FileInfo(file);
				string name = Path.GetFileName(file);
				sb.AppendLine(@$"<tr>
    <td><img src=""/icons/layout.gif"" alt=""[DIR]""></td>
    <td><a href=""{(path == "/" ? "" : path + "/")}{name}"">{name}</a></td>
    <td>{info.LastWriteTime.ToString("yyyy/MM/dd hh:mm")}</td>
    <td>{FormatSize(info.Length)}</td>
</tr>");
			}
			return sb.ToString();
		}

		static string FormatSize(long bytes)
		{
			const long KB = 1024;
			const long MB = KB * 1024;
			const long GB = MB * 1024;
			const long TB = GB * 1024;

			if (bytes >= TB)
				return $"{(double)bytes / TB:F2} TB";
			if (bytes >= GB)
				return $"{(double)bytes / GB:F2} GB";
			if (bytes >= MB)
				return $"{(double)bytes / MB:F2} MB";
			if (bytes >= KB)
				return $"{(double)bytes / KB:F2} KB";
			return $"{bytes} B";
		}
	}
}
