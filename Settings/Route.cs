using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mahi.Settings
{
	public class Route
	{
		public string Type { get; set; }
		public string Url { get; set; }
		public string RoutePath { get; set; }
		public string Default { get; set; }
		public string Redirect { get; internal set; }
	}
}
