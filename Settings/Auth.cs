using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mahi.Settings
{
	internal class Auth : Dictionary<string, string>
	{
		public Auth(Dictionary<string, string> dictionary, IEqualityComparer<string> comparer) : base(dictionary, comparer) {  }

		public string Name => this["Name"];
		public int? Timeout { get { if (TryGetValue("Timeout", out string value)) return int.Parse(value); return null; } }
		public string Path => this["Path"];
		public string Key => this["Key"];
		public string DefaultUrl => this["DefaultUrl"];
		public string LoginUrl => this["LoginUrl"];
		public string SameSite => this["SameSite"];
		public string RoleManager => this["RoleManager"];

        public string this[string key]
		{
			get
			{
                if (TryGetValue(key, out string value))
					return value;
				return null;
			}
		}

        byte[] keyBytes;
		public byte[] GetKeyBytes()
		{
			if (keyBytes != null)
				return keyBytes;

			string hex = Key;
			keyBytes = Enumerable.Range(0, hex.Length / 2)
						 .Select(i => Convert.ToByte(hex.Substring(i * 2, 2), 16))
						 .ToArray();
			return keyBytes;
		}
	}
}
