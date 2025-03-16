using System.Security.Cryptography;
using YamlDotNet.RepresentationModel;

namespace Mahi.Settings
{
    internal class ConfigParser
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

            bool foundExtentionRequired = false;
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
                    case "bindhost":
                        config.BindHost = entry.Value.ToString();
                        break;
                    case "basedirectory":
                        config.BaseDirectory = entry.Value.ToString();
                        break;
                    case "directorybrowsing":
                        config.DirectoryBrowsing = bool.Parse(((YamlScalarNode)entry.Value).Value);
                        break;
                    case "defaultpages":
                        config.DefaultPages = ReadStringArray((YamlSequenceNode)entry.Value);
                        break;
                    case "extentionrequired":
                        config.ExtentionRequired = bool.Parse(((YamlScalarNode)entry.Value).Value);
                        foundExtentionRequired = true;
                        break;
                    case "notextentioninurl":
                        config.NotExtentionInUrl = bool.Parse(((YamlScalarNode)entry.Value).Value);
                        break;
                    case "connectionstrings":
                        config.ConnectionStrings = ReadDictionary((YamlMappingNode)entry.Value);
                        break;
                    case "auth":
                        config.Auth = new Auth(ReadDictionary((YamlMappingNode)entry.Value), StringComparer.OrdinalIgnoreCase);
                        break;
                    case "routes":
                        config.Routes = ReadRouteDictionary((YamlMappingNode)entry.Value);
                        break;
                    case "frobiddenpaths":
                        config.FrobiddenPaths = ReadStringArray((YamlSequenceNode)entry.Value);
                        break;
                    case "redirecterrorcode":
                        config.RedirectErrorPage = bool.Parse(((YamlScalarNode)entry.Value).Value);
                        break;
                    case "errorpages":
                        config.ErrorPages = ReadDictionary((YamlMappingNode)entry.Value);
                        foreach (var page in config.ErrorPages)
                            if (!File.Exists(Path.GetFullPath("wwwapp") + '\\' + page.Value))
                                throw new FileNotFoundException("Cannot find error page for status code " + page.Key + " with filename: " + page.Value);
                        break;
                    case "httpmodules":
                        config.HttpModules = ReadDictionary((YamlMappingNode)entry.Value);
                        break;
                }
            }

            if (config.ExtentionRequired)
                config.NotExtentionInUrl = false;
            if (config.DefaultPages == null)
                config.DefaultPages = new string[0];
            if (config.FrobiddenPaths == null)
                config.FrobiddenPaths = new string[0];
            if (config.Routes == null)
                config.Routes = new Dictionary<string, Route>();
            if (config.ErrorPages == null)
                config.ErrorPages = new Dictionary<string, string>();
            if (config.HttpModules == null)
                config.HttpModules = new Dictionary<string, string>();
            if (config.Auth == null)
            {
                byte[] key = new byte[32];
                RandomNumberGenerator.Fill(key);
                Dictionary<string, string> authData = new Dictionary<string, string>();
                authData.Add("Key", BitConverter.ToString(key).Replace("-", ""));
                var auth = new Auth(authData, StringComparer.OrdinalIgnoreCase);
                config.Auth = auth;
            }

            if (string.IsNullOrEmpty(config.BaseDirectory))
                config.BaseDirectory = "wwwapp";
            if (!foundExtentionRequired)
                config.ExtentionRequired = true;

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
            var dictionary = new Dictionary<string, string>();
            foreach (var entry in mapping.Children)
            {
                var key = ((YamlScalarNode)entry.Key).Value;
                var value = ((YamlScalarNode)entry.Value).Value;
                dictionary[key] = value;
            }
            dictionary.TryGetValue("name", out string values);
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
                    Type = GetValueCaseInsensitive(routeNode, "type"),
                    Url = GetValueCaseInsensitive(routeNode, "url"),
                    RoutePath = GetValueCaseInsensitive(routeNode, "route"),
                };

                routes[key] = route;
            }
            return routes;
        }

        private static string GetValueCaseInsensitive(YamlMappingNode node, string key)
        {
            var entry = node.Children.FirstOrDefault(
                x => string.Equals(((YamlScalarNode)x.Key).Value, key, StringComparison.OrdinalIgnoreCase)
            );

            if (entry.Equals(default(KeyValuePair<YamlNode, YamlNode>)))
                return null;

            return ((YamlScalarNode)entry.Value).Value;
        }
    }
}
