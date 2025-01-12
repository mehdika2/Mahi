using Mahi.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mahi.Core
{
	internal class RouteUri
	{
		public static bool TryFindRoute(Uri uri, out string filename)
		{
			var routes = AppConfig.Instance.Routes;
			foreach (var route in routes)
			{
				switch (route.Value.Type.ToLower())
				{
					case "regex":
						if (Regex.Match(uri.AbsolutePath, route.Value.Url).Success)
						{
							if (route.Value.RoutePath != null)
								filename = route.Value.RoutePath;
							else throw new InvalidDataException("No route or redirect path set for regex route: " + route.Key);
							return true;
						}
						break;

					case "static":
						if (uri.AbsolutePath.ToLower() != route.Value.Url.ToLower())
							break;
						if (route.Value.RoutePath != null)
							filename = route.Value.RoutePath;
						else throw new InvalidDataException("No route or redirect path set for static route: " + route.Key);
						return true;

					case "dynamic":

						if (route.Value.RoutePath != null)
							filename = ProcessDynamicRoute(route.Value.Url, uri.AbsolutePath, route.Value.RoutePath);
						//else if (route.Value.Redirect != null)
						//{
						//	filename = ProcessDynamicRoute(route.Value.Url, uri.AbsolutePath, route.Value.Redirect);
						//	if (filename == null)
						//		break;
						//	filename = filename.Insert(0, ">");
						//}
						//else if (route.Value.Controller != null)
						//	filename = "=" + route.Value.Controller;
						else throw new InvalidDataException("No route or redirect path set for dynamic route: " + route.Key);

						return true;

					default:
						throw new InvalidDataException("Invalid route type: " + route.Value.Type);
				}
			}

			filename = Path.GetFullPath("wwwapp") + '\\' + uri.AbsolutePath.Trim('/').Replace('/', '\\');
			return false;
		}

		private static string ProcessDynamicRoute(string pattern, string url, string outputTemplate)
		{
			string regexPattern = Regex.Replace(pattern, @"\{([^}]+)\}", @"([^/]+)");

			Regex regex = new Regex($"^{regexPattern}$");

			Match match = regex.Match(url);

			if (match.Success)
			{
				var parameters = new Dictionary<string, string>();

				var paramNames = Regex.Matches(pattern, @"\{([^}]+)\}");
				for (int i = 0; i < paramNames.Count; i++)
				{
					string paramName = paramNames[i].Groups[1].Value;
					string paramValue = match.Groups[i + 1].Value;
					parameters[paramName] = paramValue;
				}

				string result = outputTemplate;
				foreach (var param in parameters)
					result = result.Replace($"{{{param.Key}}}", param.Value);

				return result;
			}
			else return null;
		}
	}
}
