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
						if (Regex.Match(uri.AbsoluteUri, route.Value.Url).Success)
						{
							if (route.Value.RoutePath != null)
								filename = route.Value.RoutePath;
							else if (route.Value.Redirect != null)
								filename = "~" + route.Value.Redirect;
							else throw new InvalidDataException("No route or redirect path set for regex route: " + route.Key);
							return true;
						}
						break;

					case "static":
						if (uri.AbsolutePath.ToLower() != route.Value.Url.ToLower())
							break;
						if (route.Value.RoutePath != null)
							filename = route.Value.RoutePath;
						else if (route.Value.Redirect != null)
							filename = "~" + route.Value.Redirect;
						else throw new InvalidDataException("No route or redirect path set for static route: " + route.Key);
						return true;

					case "dynamic":

						break;

					default:
						throw new InvalidDataException("Invalid route type: " + route.Value.Type);
				}
			}

			filename = Path.GetFullPath("wwwapp") + '\\' + uri.AbsolutePath.Trim('/').Replace('/', '\\');
			return false;
		}
	}
}
