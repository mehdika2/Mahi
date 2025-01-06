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
        private const string _filePath = "appconfig.yaml";
        private static FileSystemWatcher _fileWatcher;

        public string[] DefaultPages { get; set; }
        public bool ExtentionRequired { get; set; }
        public Dictionary<string, string> ConnectionStrings { get; set; }
        public Dictionary<string, Route> Routes { get; set; }

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
			if (!File.Exists(_filePath))
				throw new FileNotFoundException(_filePath + " config file not found!");
            _instance = ConfigParser.ParseYaml(File.ReadAllText(_filePath));
		}

		public static void StartConfigWatcher()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), _filePath);

            _fileWatcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(filePath),
                Filter = Path.GetFileName(filePath),
                NotifyFilter = NotifyFilters.LastWrite
            };

            _fileWatcher.Changed += (sender, e) =>
            {
                // Delay to ensure the file is fully written
                Thread.Sleep(500);
                LoadConfigs();
            };

            _fileWatcher.EnableRaisingEvents = true;
        }
    }
}
