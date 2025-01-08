﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Mahi.Settings
{
	public class ConfigParser
	{
		public static AppConfig ParseYaml(string yamlContent)
		{
			var config = new AppConfig
			{
				ConnectionStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
				Routes = new Dictionary<string, Route>(StringComparer.OrdinalIgnoreCase)
			};

			var yaml = new YamlStream();
			yaml.Load(new StringReader(yamlContent));

			var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

			var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (var entry in mapping.Children)
			{
				var key = ((YamlScalarNode)entry.Key).Value;

				if (seenKeys.Contains(key))
				{
					throw new InvalidOperationException($"Duplicate key detected: {key}");
				}

				seenKeys.Add(key);

				switch (key.ToLowerInvariant())
				{
					case "defaultpages":
						config.DefaultPages = ReadStringArray((YamlSequenceNode)entry.Value);
						break;
					case "extentionrequired":
						config.ExtentionRequired = bool.Parse(((YamlScalarNode)entry.Value).Value);
						break;
					case "notextentioninurl":
						config.NotExtentionInUrl = bool.Parse(((YamlScalarNode)entry.Value).Value);
						break;
					case "connectionstrings":
						config.ConnectionStrings = ReadDictionary((YamlMappingNode)entry.Value);
						break;
					case "routes":
						config.Routes = ReadRouteDictionary((YamlMappingNode)entry.Value);
						break;
					case "redirecterrorcode":
						config.RedirectErrorPage = bool.Parse(((YamlScalarNode)entry.Value).Value);
						break;
					case "errorpages":
						config.ErrorPages = ReadDictionary((YamlMappingNode)entry.Value);
						foreach (var page in config.ErrorPages)
							if(!File.Exists(Path.GetFullPath("wwwapp") + '\\' + page.Value))
								throw new FileNotFoundException("Cannot find error page for status code " + page.Key + " with filename: " + page.Value);
						break;
					case "httpmodules":
						config.HttpModules = ReadDictionary((YamlMappingNode)entry.Value);
						break;
					default:
						// Handle unknown properties if necessary
						break;
				}
			}

			if (config.ExtentionRequired)
				config.NotExtentionInUrl = false;
			if (config.Routes == null)
				config.Routes = new Dictionary<string, Route>();
			if (config.ErrorPages == null)
				config.ErrorPages = new Dictionary<string, string>();
			if (config.HttpModules == null)
				config.HttpModules = new Dictionary<string, string>();

			return config;
		}

		private static string[] ReadStringArray(YamlSequenceNode sequence)
		{
			var list = new List<string>();
			foreach (var item in sequence.Children)
			{
				list.Add(((YamlScalarNode)item).Value);
			}
			return list.ToArray();
		}

		private static Dictionary<string, string> ReadDictionary(YamlMappingNode mapping)
		{
			var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			foreach (var entry in mapping.Children)
			{
				var key = ((YamlScalarNode)entry.Key).Value;
				var value = ((YamlScalarNode)entry.Value).Value;
				dictionary[key] = value;
			}
			return dictionary;
		}

		private static Dictionary<string, Route> ReadRouteDictionary(YamlMappingNode mapping)
		{
			var routes = new Dictionary<string, Route>(StringComparer.OrdinalIgnoreCase);
			foreach (var entry in mapping.Children)
			{
				var key = ((YamlScalarNode)entry.Key).Value;
				var routeNode = (YamlMappingNode)entry.Value;

				var route = new Route
				{
					Type = routeNode["type"].ToString(),
					Url = routeNode["url"].ToString(),
					RoutePath = routeNode.Children.ContainsKey("route") ? routeNode["route"].ToString() : null,
					Redirect = routeNode.Children.ContainsKey("redirect") ? routeNode["redirect"].ToString() : null,
					Controller = routeNode.Children.ContainsKey("controller") ? routeNode["controller"].ToString() : null
				};

				routes[key] = route;
			}
			return routes;
		}
	}
}
