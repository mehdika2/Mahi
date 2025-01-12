using Mahi.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Mahi.Settings
{
	public class AppConfig
	{
		private const string _filename = "appconfig.yml";
		private static FileSystemWatcher _fileWatcher;

        public string BaseDirectory { get; set; }
        public string[] DefaultPages { get; set; }
		public bool ExtentionRequired { get; set; }
		public bool NotExtentionInUrl { get; set; }
		public Dictionary<string, string> ConnectionStrings { get; set; }
		public Dictionary<string, Route> Routes { get; set; }
		public bool RedirectErrorPage { get; set; }
		public Dictionary<string, string> ErrorPages { get; internal set; }
		public Dictionary<string, string> HttpModules { get; internal set; }

		private static AppConfig _instance;
		private static readonly object _lock = new object();
		public static AppConfig Instance
		{
			get
			{
				lock (_lock)
				{
					LoadConfigs();
					return _instance;
				}
			}
		}

		public static void LoadConfigs()
		{
			if (!File.Exists(_filename))
				throw new FileNotFoundException(_filename + " config file not found!");

			_instance = ConfigParser.ParseYaml(File.ReadAllText(_filename));
		}

		public static void StartConfigWatcher()
		{
			string filePath = Path.Combine(Directory.GetCurrentDirectory(), _filename);

			_fileWatcher = new FileSystemWatcher
			{
				Path = Path.GetDirectoryName(filePath),
				Filter = Path.GetFileName(filePath),
				NotifyFilter = NotifyFilters.LastWrite
			};

			_fileWatcher.Changed += (sender, e) =>
			{
				if (e.Name.ToLower() != _filename)
					return;

				// Delay to ensure the file is fully written
				Thread.Sleep(500);
				LoadConfigs();
			};

			_fileWatcher.EnableRaisingEvents = true;
		}
	}
}
