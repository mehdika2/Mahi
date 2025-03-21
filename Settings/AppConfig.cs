﻿namespace Mahi.Settings
{
    internal class AppConfig
    {
        private const string _filename = "appconfig.yml";
        private static FileSystemWatcher _fileWatcher;

		public string BindHost { get; set; }
		public string BaseDirectory { get; set; }
        public string[] DefaultPages { get; set; }
        public bool DirectoryBrowsing { get; set; }
        public bool ExtentionRequired { get; set; }
        public bool NotExtentionInUrl { get; set; }
        public Dictionary<string, string> ConnectionStrings { get; set; }
        public Dictionary<string, Route> Routes { get; set; }
        public string[] FrobiddenPaths { get; internal set; }
        public bool RedirectErrorPage { get; set; }
        public Dictionary<string, string> ErrorPages { get; internal set; }
        public Dictionary<string, string> HttpModules { get; internal set; }
        public Auth Auth { get; set; }

        private static AppConfig _instance;
        private static readonly object _lock = new object();
        public static AppConfig Instance
        {
            get
            {
                return _instance;
            }
        }

        public static void LoadConfigs()
        {
            lock (_lock)
            {
                if (!File.Exists(_filename))
                    throw new FileNotFoundException(_filename + " config file not found!");

                try
                {
                    _instance = ConfigParser.ParseYaml(File.ReadAllText(_filename));
                }
                catch (Exception ex)
                {
					Program.Logger.Log("&r[&4Error&r] Faild to reload config file: " + ex.Message);
                }
            }
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
                // Delay to ensure the file is fully written
                Thread.Sleep(500);
                LoadConfigs();
            };

            _fileWatcher.EnableRaisingEvents = true;
        }
    }
}
