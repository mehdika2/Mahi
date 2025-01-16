using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mahi.Settings
{
	public class Auth : Dictionary<string, string>
	{
		public Auth(Dictionary<string, string> dictionary) : base(dictionary) { }

		public string Name => this["Name"];
		public int? Timeout => int.Parse(this["Timeout"]);
		public string Path => this["Path"];
		public string Key => this["Key"];
		public string DefaultUrl => this["DefaultUrl"];
		public string LoginUrl => this["LoginUrl"];
		public string SameSite => this["SameSite"];

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
